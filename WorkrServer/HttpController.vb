Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading
Imports Newtonsoft.Json
Imports WorkrServer.Entity

''' <summary>
''' Handles incomming HTTP request
''' </summary>
Public Class HttpController
    Public Event OnRequest(ByVal msg As String)
    Private Listener As HttpListener
    Private ListenThread As Thread

    Public Sub New(ByVal baseUrl As String, ByRef map As Dictionary(Of String, Object))
        Helper.Map = map
        Listener = New HttpListener()
        Listener.Prefixes.Add(baseUrl)
    End Sub

    Public Sub New(ByVal baseURLs As String(), ByRef map As Dictionary(Of String, Object))
        Helper.Map = map
        Listener = New HttpListener()
        For Each url As String In baseURLs
            Listener.Prefixes.Add(url)
        Next
    End Sub

    ''' <summary>
    ''' Starts the listening of the HttpController
    ''' </summary>
    Public Sub StartListening()
        Listener.Start()
        ListenThread = New Thread(AddressOf Listen)
        ListenThread.Start()
    End Sub

    ''' <summary>
    ''' Listens for incomming requests and proccesses them
    ''' Runs on it's own thread.
    ''' Contains an infinate loop as to keep listening for more request once it has handled one.
    ''' </summary>
    Private Sub Listen()
        Dim context As HttpListenerContext
        Dim request As HttpListenerRequest
        Dim response As HttpListenerResponse = Nothing
        Dim path As String() = {}
        Dim data As String = Nothing
        Dim file As MemoryStream = Nothing
        Dim responseData As Object

        While True
            Try
                While Listener.IsListening
                    Try
                        path = {}
                        data = Nothing
                        file = Nothing
                        responseData = Nothing
                        context = Listener.GetContext
                        request = context.Request
                        response = context.Response
                        path = request.Url.AbsolutePath.Split({"/"}, StringSplitOptions.RemoveEmptyEntries)
                        Authenticate(request.Headers("Authorization"))
                        If request.ContentLength64 > 5000000 Then Throw New ContentSizeLimitExceededException
                        If path.Length < 1 Then Throw New NoResourceGivenException
                        ProcessInput(request.ContentEncoding, request.InputStream, request.ContentType, data, file)
                        responseData = NavigateMap(context, path, data, file)
                        RaiseEvent OnRequest(CreateOnRequestString(request.HttpMethod, request.RemoteEndPoint.Address.ToString, request.Url.AbsoluteUri, CStr(request.ContentType), data))
                        If responseData.GetType = GetType(MemoryStream) Then
                            SendResponse(response, DirectCast(responseData, MemoryStream))
                        Else
                            SendResponse(response, responseData.ToString)
                        End If
                    Catch ex As Exception
                        HandleRequestException(response, ex, path)
                    End Try
                End While
            Catch ex As Exception
            End Try
            Thread.Sleep(3000)
            Listener.Start()
        End While
    End Sub

    ''' <summary>
    ''' Navigates the incomming API call the appropriate resource.
    ''' </summary>
    ''' <param name="context">The context of the HTTP request</param>
    ''' <param name="path">the request URL segments</param>
    ''' <param name="data">the request body data</param>
    ''' <param name="file">the request multipart formatted file</param>
    ''' <returns></returns>
    Private Function NavigateMap(ByRef context As HttpListenerContext, path As String(), data As String, file As MemoryStream) As Object
        Dim response = Nothing
        Try
            Select Case context.Request.HttpMethod
                Case "GET"
                    If path(0).ToLower = "auth" Then response = AuthUser : Exit Select
                    If context.Request.Url.Query = "?file" OrElse
                        GetContentType(context.Request.ContentType).StartsWith("image/") Then
                        If path.Length < 3 Then Return Map(path(0)).GetFile(path(1))
                        Return Map(path(0)).GetFile(path(1), path(2))
                    End If
                    If path.Length > 1 Then response = Map(path(0)).GetByID(path(1)) : Exit Select
                    response = Map(path(0)).GetAll()
                Case "POST"
                    If path.Length > 1 Then response = Map(path(0)).GetByID(path(1)) : Exit Select
                    response = Map(path(0)).Search(data)
                Case "PUT"
                    If path(0).ToLower = "register" Then response = Register(data, context.Request.Headers("Password")) : Exit Select
                    If file IsNot Nothing Then response = Map(path(0)).PutFile(file, path(1)) : Exit Select
                    response = Map(path(0)).Put(data)
                Case "DELETE"
                    response = Map(path(0)).Delete(path(1))
                Case "PATCH"
                    response = Map(path(0)).Patch(path(1), data)
                Case "VIEW"
                    response = Map(path(0)).GetFile(path(1), path(2)) : Return response
            End Select
        Catch ex As IndexOutOfRangeException
            Throw New MalformedUrlException
        End Try

        Return JsonConvert.SerializeObject(response, JSONSettings)
    End Function

    ''' <summary>
    ''' Returns the result of the API call over HTTP
    ''' </summary>
    ''' <param name="response"></param>
    ''' <param name="data"></param>
    Private Overloads Sub SendResponse(ByRef response As HttpListenerResponse, data As String)
        Try
            Dim responseBytes As Byte() = Encoding.UTF8.GetBytes(data)
            response.ContentType = "application/json"
            response.ContentLength64 = responseBytes.Length
            response.OutputStream.Write(responseBytes, 0, responseBytes.Length)
            response.Close()
        Catch ex As Exception
        End Try
    End Sub
    ''' <summary>
    ''' Returns the resulting image of the API call over HTTP
    ''' </summary>
    ''' <param name="response"></param>
    ''' <param name="data"></param>
    Private Overloads Sub SendResponse(ByRef response As HttpListenerResponse, data As MemoryStream)
        Dim dataStream As MemoryStream = data
        dataStream.Position = 0
        response.ContentType = "image/png"
        response.ContentLength64 = dataStream.Length
        dataStream.WriteTo(response.OutputStream)
        dataStream.Close()
        response.Close()
    End Sub

    ''' <summary>
    ''' Returns an error message if an error has occured
    ''' </summary>
    ''' <param name="errorCode"></param>
    ''' <param name="errorMessage"></param>
    ''' <returns></returns>
    Private Function ErrorResponse(errorCode As Integer, errorMessage As String) As String
        Return String.Format("{{ ""ErrorCode"" : {0}, ""ErrorMessage"" : ""{1}"" }}", errorCode, errorMessage)
    End Function

    ''' <summary>
    ''' Gets the content-type of an HTTP request
    ''' </summary>
    ''' <param name="contentType"></param>
    ''' <returns></returns>
    Private Function GetContentType(contentType As String) As String
        If contentType Is Nothing Then Return ""
        Return contentType.Split(";")(0)
    End Function

    ''' <summary>
    ''' Builds the data string used for the OnRequest event that is used to log request information
    ''' </summary>
    ''' <param name="method"></param>
    ''' <param name="origin"></param>
    ''' <param name="url"></param>
    ''' <param name="contenttype"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Private Function CreateOnRequestString(method As String, origin As String, url As String, contenttype As String, data As String) As String
        Return String.Format("REQUEST TIME" & vbTab & "{0}" & vbCrLf &
                             "ORIGIN" & vbTab & vbTab & "{1}" & vbCrLf &
                             "URL" & vbTab & vbTab & "{2} \ {3}" & vbCrLf &
                             "CONTENT-TYPE:" & vbTab & "{4}" & vbCrLf & vbCrLf &
                             "{5}",
                             Now().ToLongTimeString,
                             origin,
                             method,
                             url,
                             contenttype,
                             data)
    End Function

    ''' <summary>
    ''' Handels errors and sets the appropiate error code and message
    ''' </summary>
    ''' <param name="response"></param>
    ''' <param name="ex"></param>
    ''' <param name="path"></param>
    Private Sub HandleRequestException(ByRef response As HttpListenerResponse, ex As Exception, path As String())
        Select Case ex.GetType
            Case GetType(NotAuthorizedException)
                response.StatusCode = 401
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case GetType(UserRegistrationException)
                response.StatusCode = 400
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case GetType(PasswordRequirementException)
                response.StatusCode = 400
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case GetType(ContentSizeLimitExceededException)
                response.StatusCode = 413
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case GetType(NoResourceGivenException)
                response.StatusCode = 400
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
            Case GetType(KeyNotFoundException)
                response.StatusCode = 404
                SendResponse(response, ErrorResponse(response.StatusCode, String.Format("The requested resource '{0}' was not found.", path(0))))
            Case GetType(ForeignKeyFialationException)
                response.StatusCode = 400
                SendResponse(response, ErrorResponse(response.StatusCode, ex.Message))
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
            Case GetType(MalformedJsonException)
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

    'HttpController exceptions
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