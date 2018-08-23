Imports System.Data.Entity
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports WorkrServer.Entity

Public Class HttpController
    Public Event OnRequest(ByVal msg As String)
    Private Listener As HttpListener
    Private ListenThread As Thread

    Public Sub New(ByVal baseUrl As String, ByRef Map As Dictionary(Of String, Object))
        Helper.Map = Map
        Listener = New HttpListener()
        Listener.Prefixes.Add(baseUrl)
    End Sub

    Public Sub New(ByVal baseURLs As String(), ByRef Map As Dictionary(Of String, Object))
        Helper.Map = Map
        Listener = New HttpListener()
        For Each url As String In baseURLs
            Listener.Prefixes.Add(url)
        Next
    End Sub

    Public Sub StartListening()
        Listener.Start()
        ListenThread = New Thread(AddressOf Listen)
        ListenThread.Start()
    End Sub

    Private Sub Listen()
        Dim context As HttpListenerContext
        Dim request As HttpListenerRequest
        Dim response As HttpListenerResponse = Nothing
        Dim path As String() = {}
        Dim data As String = Nothing
        Dim file As MemoryStream = Nothing
        Dim responseData As Object

        While True
            While Listener.IsListening
                Try
                    context = Listener.GetContext
                    request = context.Request
                    response = context.Response
                    path = request.Url.AbsolutePath.Split({"/"}, StringSplitOptions.RemoveEmptyEntries)
                    If request.ContentLength64 > 5000000 Then Throw New ContentSizeLimitExceededException
                    If path.Length < 1 Then Throw New NoResourceGivenException
                    ProcessInput(request.ContentEncoding, request.InputStream, request.ContentType, data, file)
                    responseData = NavigateMap(context, data, file)
                    RaiseEvent OnRequest(String.Format("{0} - {1} : {2}" & vbCrLf & "{3}" & vbCrLf & vbCrLf & "{4}",
                                                       Now().ToShortTimeString,
                                                       request.HttpMethod,
                                                       request.Url.AbsoluteUri,
                                                       CStr(request.ContentType),
                                                       data))
                    If responseData.GetType = GetType(MemoryStream) Then
                        SendResponse(response, DirectCast(responseData, MemoryStream))
                    Else
                        SendResponse(response, responseData.ToString)
                    End If
                Catch ex As Exception
                    HandleRequestException(response, ex, path)
                End Try
            End While
            Thread.Sleep(3000)
            Listener.Start()
        End While
    End Sub

    Private Function NavigateMap(ByRef context As HttpListenerContext, data As String, file As MemoryStream) As Object
        Dim response = Nothing
        Dim path = context.Request.Url.AbsolutePath.Split({"/"}, StringSplitOptions.RemoveEmptyEntries)
        Try
            Select Case context.Request.HttpMethod
                Case "GET"
                    If context.Request.Url.Query = "?file" OrElse
                        GetContentType(context.Request.ContentType).StartsWith("image/") Then
                        Return Map(path(0)).View(path(1), path(2))
                    End If
                    If path.Length > 1 Then response = Map(path(0)).GetByID(path(1)) : Exit Select
                    response = Map(path(0)).GetAll()
                Case "POST"
                    If path.Length > 1 Then response = Map(path(0)).GetByID(path(1)) : Exit Select
                    response = Map(path(0)).Search(data)
                Case "PUT"
                    If file IsNot Nothing Then response = Map(path(0)).PutFile(file, path(1)) : Exit Select
                    response = Map(path(0)).Put(data)
                Case "DELETE"
                    response = Map(path(0)).Delete(path(1))
                Case "PATCH"
                    response = Map(path(0)).Patch(path(1), data)
                Case "VIEW"
                    response = Map(path(0)).View(path(1), path(2)) : Return response
            End Select
        Catch ex As IndexOutOfRangeException
            Throw New MalformedUrlException
        End Try

        Return JsonConvert.SerializeObject(response, JSONSettings)
    End Function

    Private Overloads Sub SendResponse(ByRef response As HttpListenerResponse, ByVal data As String)
        Try
            Dim responseBytes As Byte() = Encoding.UTF8.GetBytes(data)
            response.ContentType = "application/json"
            response.ContentLength64 = responseBytes.Length
            response.OutputStream.Write(responseBytes, 0, responseBytes.Length)
            response.Close()
        Catch ex As Exception
            response.StatusCode = 500
            SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
        End Try
    End Sub

    Private Overloads Sub SendResponse(ByRef response As HttpListenerResponse, ByVal data As MemoryStream)
        Dim dataStream As MemoryStream = data
        dataStream.Position = 0
        response.ContentType = "image/png"
        response.ContentLength64 = dataStream.Length
        dataStream.WriteTo(response.OutputStream)
        dataStream.Close()
        response.Close()
    End Sub

    Private Function ErrorResponse(errorCode As Integer, errorMessage As String) As String
        Return String.Format("{{ ""ErrorCode"" : {0}, ""ErrorMessage"" : ""{1}"" }}", errorCode, errorMessage)
    End Function

    Private Function GetContentType(contentType As String) As String
        If contentType Is Nothing Then Return ""
        Return contentType.Split(";")(0)
    End Function


    Private Sub HandleRequestException(ByRef response As HttpListenerResponse, ex As Exception, path As String())
        Select Case ex.GetType
            Case GetType(ContentSizeLimitExceededException)
                response.StatusCode = 413
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case GetType(NoResourceGivenException)
                response.StatusCode = 400
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case GetType(KeyNotFoundException)
                response.StatusCode = 404
                SendResponse(response, ErrorResponse(response.StatusCode, String.Format("The requested resource '{0}' was not found.", path(0))))
            Case GetType(Entity.FileNotFoundException)
                response.StatusCode = 404
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case GetType(IndexOutOfRangeException)
                response.StatusCode = 405
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case GetType(InvalidOperationException)
                response.StatusCode = 400
                SendResponse(response, ErrorResponse(response.StatusCode, "Given ID was not found."))
            Case GetType(IdNotFoundException)
                response.StatusCode = 400
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case GetType(MalformedUrlException)
                response.StatusCode = 400
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case GetType(Exception)
                response.StatusCode = 500
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case Else
                response.StatusCode = 500
                SendResponse(response, ErrorResponse(response.StatusCode, "Unknown Error."))
        End Select
    End Sub

    Public Class ContentSizeLimitExceededException
        Inherits Exception

        Public Overrides ReadOnly Property Message As String
            Get
                Return "Request content was bigger than 5MB."
            End Get
        End Property
    End Class

    Public Class NoResourceGivenException
        Inherits Exception

        Public Overrides ReadOnly Property Message As String
            Get
                Return "No resource given."
            End Get
        End Property
    End Class

    Public Class MalformedUrlException
        Inherits Exception

        Public Overrides ReadOnly Property Message As String
            Get
                Return "URL was wrong for given resource"
            End Get
        End Property
    End Class
End Class



'$.ajax({
'    url: "http://127.0.0.1:9877/lol",
'    type: "POST",
'    data: "{id : 1, name : 'rune'}",
'    contentType: "application/json",
'    success: Function (data) {
'		Console.log(data);
'    }
'});

'$.ajax({
'    url: "http://127.0.0.1:9877/room/search",
'    type: "POST",
'    data: "{size : 50, view : 1}",
'    contentType: "application/json",
'    success: Function (data) {
'		Console.log(data);
'    }
'});
