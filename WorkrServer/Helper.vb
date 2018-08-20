Imports System.Data.Entity
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Newtonsoft.Json
Imports WorkrServer

''' <summary>
''' Static helper class.
''' </summary>
Module Helper
    Private m_DB As New WorkrDB
    Private m_Map As New Dictionary(Of String, Object)
    Private m_JSONSettings As New JsonSerializerSettings() With {.MissingMemberHandling = MissingMemberHandling.Ignore,
        .DateFormatString = "yyyy-MM-ddTHH:mm:ssZ"}

    ''' <summary>
    ''' Dictionary of Table objects and a string key. The key is used in the URL of HTTP requests.
    ''' </summary>
    ''' <returns></returns>
    Public Property Map As Dictionary(Of String, Object)
        Get
            Return m_Map
        End Get
        Set(value As Dictionary(Of String, Object))
            m_Map = value
        End Set
    End Property

    Public Property JSONSettings As JsonSerializerSettings
        Get
            Return m_JSONSettings
        End Get
        Set(value As JsonSerializerSettings)
            m_JSONSettings = value
        End Set
    End Property

    Public Property DB As WorkrDB
        Get
            Return m_DB
        End Get
        Set(value As WorkrDB)
            m_DB = value
        End Set
    End Property

    ''' <summary>
    ''' Returns true if the superset array contains all items in subset array.
    ''' </summary>
    ''' <param name="subset"></param>
    ''' <param name="superset"></param>
    ''' <returns>Boolean</returns>
    Public Function IsSubsetOf(ByVal subset As Object(), ByVal superset As Object()) As Boolean
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

    Public Sub SaveFile(enc As Encoding, boundary As String, input As Stream)
        Dim boundaryBytes As Byte() = enc.GetBytes(boundary)
        Dim boundaryLen As Int32 = boundaryBytes.Length

        Using output As FileStream = New FileStream("postimages\" & Guid.NewGuid.ToString & ".png", FileMode.Create, FileAccess.Write)
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
                    If endPos > 0 Then output.Write(buffer, 0, endPos - 2)
                    Exit While
                ElseIf len <= boundaryLen Then
                    Throw New Exception("End Boundaray Not Found")
                Else
                    output.Write(buffer, 0, len - boundaryLen)
                    Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen)
                    len = input.Read(buffer, boundaryLen, 1024 - boundaryLen) + boundaryLen
                End If
            End While
        End Using
    End Sub

    Public Function GetBoundary(contentType As String) As String
        Return "--" & contentType.Split(";")(1).Split("=")(1)
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
