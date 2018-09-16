Imports System.Data.Entity
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Text
Imports Newtonsoft.Json

''' <summary>
''' Static helper class.
''' </summary>
Module Helper
    Private m_DB As New WorkrDB
    Private m_JSONSettings As New JsonSerializerSettings() With {.MissingMemberHandling = MissingMemberHandling.Ignore,
        .DateFormatString = "yyyy-MM-ddTHH:mm:ssZ"}

    ''' <summary>
    ''' Dictionary of Table objects and a string key. The key is used in the URL of HTTP requests.
    ''' </summary>
    ''' <returns></returns>
    Public Property Map As Dictionary(Of String, Object)

    ''' <summary>
    ''' Json serialzation settings
    ''' </summary>
    ''' <returns></returns>
    Public Property JSONSettings As JsonSerializerSettings
        Get
            Return m_JSONSettings
        End Get
        Set(value As JsonSerializerSettings)
            m_JSONSettings = value
        End Set
    End Property

    ''' <summary>
    ''' The connection to WORKR database
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property DB As WorkrDB
        Get
            Return m_DB
        End Get
    End Property

    'The login information for the FTP file storage
    Private Property FTPCredentials As New NetworkCredential("workr-api", "workr123")

    ''' <summary>
    ''' Creates a FTP request
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="ftpMethod"></param>
    ''' <returns>FtpWebRequest insance</returns>
    Public Function CreateFtpRequest(path As String, ftpMethod As String) As FtpWebRequest
        Dim _ftp As FtpWebRequest = FtpWebRequest.Create("ftp://rune-nas/home/" & path.Replace("\", "/"))
        With _ftp
            .EnableSsl = False
            .Credentials = FTPCredentials
            .KeepAlive = False
            .UseBinary = True
            .UsePassive = True
            .Method = ftpMethod
        End With
        Return _ftp
    End Function

    ''' <summary>
    ''' Downloads a file from the FTP file storage
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Public Function FTPDownload(path As String) As MemoryStream
        Dim output As New MemoryStream
        Dim ftpRequest As FtpWebRequest = CreateFtpRequest(path, WebRequestMethods.Ftp.DownloadFile)
        Dim ftpResponse As FtpWebResponse = ftpRequest.GetResponse
        Dim responseStream As Stream = ftpResponse.GetResponseStream
        Dim buffer(2047) As Byte
        Dim read As Integer = 0

        Do
            read = responseStream.Read(buffer, 0, buffer.Length)
            output.Write(buffer, 0, read)
        Loop Until read = 0
        responseStream.Close()
        ftpResponse.Close()

        Return output
    End Function

    ''' <summary>
    ''' Uploads a file to the FTP file storage
    ''' </summary>
    ''' <param name="folders"></param>
    ''' <param name="filename"></param>
    ''' <param name="file"></param>
    Public Sub FTPUpload(folders As String(), filename As String, file As MemoryStream)
        Dim ftpRequest As FtpWebRequest = Nothing
        Dim ftpResponse As FtpWebResponse = Nothing
        Dim ftpResponseStream As Stream = Nothing
        Dim webRequest As New WebClient With {
                .Credentials = FTPCredentials
            }

        For i As Integer = 1 To folders.Length
            Try
                ftpRequest = CreateFtpRequest(String.Join("/", folders.Take(i)), WebRequestMethods.Ftp.MakeDirectory)
                ftpRequest.GetResponse()
            Catch ex As Exception
            End Try
        Next

        webRequest.UploadData("ftp://skurk.info/home/" & String.Join("/", folders) & "/" & filename, "STOR", file.ToArray)
    End Sub

    ''' <summary>
    ''' Downloads the first file in a folder on the FTP file storage
    ''' </summary>
    ''' <param name="dirPath"></param>
    ''' <returns></returns>
    Public Function FTPDownloadFirstFile(dirPath As String) As MemoryStream
        Dim output As New MemoryStream
        Dim filename As String
        Dim ftpRequest As FtpWebRequest = CreateFtpRequest(dirPath.Replace("\", "/"), WebRequestMethods.Ftp.ListDirectory)
        Dim ftpResponse As FtpWebResponse = CType(ftpRequest.GetResponse, FtpWebResponse)
        Dim ftpResponseStreamReader As New StreamReader(ftpResponse.GetResponseStream)

        filename = ftpResponseStreamReader.ReadLine.Split("/")(1)
        ftpRequest = CreateFtpRequest(dirPath.Replace("\", "/") & "/" & filename, WebRequestMethods.Ftp.DownloadFile)
        ftpResponse = CType(ftpRequest.GetResponse, FtpWebResponse)
        Dim ftpResponseStream As Stream = ftpResponse.GetResponseStream
        Using ftpResponseStream
            Dim buffer(2047) As Byte
            Dim read As Integer = 0
            Do
                read = ftpResponseStream.Read(buffer, 0, buffer.Length)
                output.Write(buffer, 0, read)
            Loop Until read = 0
        End Using
        ftpResponseStreamReader.Close()
        ftpResponseStream.Close()
        ftpResponse.Close()

        Return output
    End Function

    ''' <summary>
    ''' Using in entity searching
    ''' Compares each property of two entitys.
    ''' </summary>
    ''' <param name="jsonEntity"></param>
    ''' <param name="dbEntity"></param>
    ''' <param name="prop"></param>
    ''' <returns></returns>
    Public Function CompareEntityProperty(jsonEntity As Entity, dbEntity As Entity, prop As PropertyInfo) As Boolean
        If prop.GetValue(jsonEntity) Is Nothing Then Return True
        If prop.Name = "HttpMethod" Then Return True
        If prop.PropertyType.IsArray And prop.GetType() IsNot GetType(Guid?) Then
            Return IsSubsetOf(prop.GetValue(jsonEntity), prop.GetValue(dbEntity))
        End If
        If prop.PropertyType Is GetType(Entity) Then
            Return True
        End If
        If prop.PropertyType Is GetType(DateTime) Then
            Dim t1 As UInt64 = Math.Round(CDate(prop.GetValue(jsonEntity)).Ticks / TimeSpan.TicksPerSecond, 0) * TimeSpan.TicksPerSecond
            Dim t2 As UInt64 = Math.Round(CDate(prop.GetValue(dbEntity)).Ticks / TimeSpan.TicksPerSecond, 0) * TimeSpan.TicksPerSecond
            Return t1 = t2
        End If
        Return prop.GetValue(jsonEntity) = prop.GetValue(dbEntity)
    End Function

    ''' <summary>
    ''' Returns true if the superset array contains all items in subset array.
    ''' </summary>
    ''' <param name="subset"></param>
    ''' <param name="superset"></param>
    ''' <returns>Boolean</returns>
    Public Function IsSubsetOf(ByVal subset As Object(), ByVal superset As Object()) As Boolean
        If superset Is Nothing Then Return False
        Dim supersetList As List(Of Object) = superset.ToList
        Dim subsetList As List(Of Object) = subset.ToList
        For Each subItem In subsetList
            Dim found As Boolean = False
            For Each supItem In supersetList
                If IsArray(supItem) Then
                    If IsSubsetOf(subItem, supItem) Then found = True : supersetList.Remove(supItem) : Exit For
                Else
                    If subItem = supItem Then found = True : supersetList.Remove(supItem) : Exit For
                End If
            Next
            If Not found Then Return False
        Next

        Return True
    End Function

    ''' <summary>
    ''' Reads the HTTP body information of an API call
    ''' </summary>
    ''' <param name="enc"></param>
    ''' <param name="input"></param>
    ''' <param name="contentType"></param>
    ''' <param name="outStringDate"></param>
    ''' <param name="outFile"></param>
    Public Sub ProcessInput(enc As Encoding, input As Stream, contentType As String, Optional ByRef outStringDate As String = "", Optional ByRef outFile As MemoryStream = Nothing)
        Dim boundary As String = GetBoundary(contentType)
        If boundary = "" Then
            Using streamReader As New StreamReader(input)
                outStringDate = streamReader.ReadToEnd()
                outFile = Nothing
                Exit Sub
            End Using
        End If
        outFile = New MemoryStream()
        Dim boundaryBytes As Byte() = enc.GetBytes(boundary)
        Dim boundaryLen As Int32 = boundaryBytes.Length
        Dim buffer As Byte() = New Byte(1023) {}
        Dim len As Int32 = input.Read(buffer, 0, 1024)
        Dim startPos As Int32 = -1

        While True

            If len = 0 Then
                Throw New Exception("Start Boundaray Not Found")
            End If

            startPos = IndexOf(buffer, len, boundaryBytes)

            If startPos >= 0 Then
                Exit While
            Else
                Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen)
                len = input.Read(buffer, boundaryLen, 1024 - boundaryLen)
            End If
        End While

        For i As Int32 = 0 To 4 - 1

            While True

                If len = 0 Then
                    Throw New Exception("Preamble not Found.")
                End If

                startPos = Array.IndexOf(buffer, enc.GetBytes(vbLf)(0), startPos)

                If startPos >= 0 Then
                    startPos += 1
                    Exit While
                Else
                    len = input.Read(buffer, 0, 1024)
                End If
            End While
        Next

        Array.Copy(buffer, startPos, buffer, 0, len - startPos)
        len = len - startPos

        While True
            Dim endPos As Int32 = IndexOf(buffer, len, boundaryBytes)

            If endPos >= 0 Then
                If endPos > 0 Then outFile.Write(buffer, 0, endPos - 2)
                Exit While
            ElseIf len <= boundaryLen Then
                Throw New Exception("End Boundaray Not Found")
            Else
                outFile.Write(buffer, 0, len - boundaryLen)
                Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen)
                len = input.Read(buffer, boundaryLen, 1024 - boundaryLen) + boundaryLen
            End If
        End While
    End Sub

    'Used for multipart image uploads
    Public Function GetBoundary(contentType As String) As String
        Try
            If CStr(contentType).StartsWith("multipart/form-data") Then
                Return "--" & contentType.Split(";")(1).Split("=")(1)
            End If
        Catch ex As Exception
        End Try
        Return ""
    End Function

    Public Function IndexOf(buffer As Byte(), len As Int32, boundaryBytes As Byte()) As Int32
        For i As Int32 = 0 To len - boundaryBytes.Length
            Dim match As Boolean = True
            Dim j As Int32 = 0

            While j < boundaryBytes.Length AndAlso match
                match = buffer(i + j) = boundaryBytes(j)
                j += 1
            End While

            If match Then
                Return i
            End If
        Next

        Return -1
    End Function
End Module
