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
        Try
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
                        path = {}
                        data = Nothing
                        file = Nothing
                        responseData = Nothing
                        context = Listener.GetContext
                        request = context.Request
                        response = context.Response
                        path = request.Url.AbsolutePath.Split({"/"}, StringSplitOptions.RemoveEmptyEntries)
                        If request.ContentLength64 > 5000000 Then Throw New ContentSizeLimitExceededException
                        If path.Length < 1 Then Throw New NoResourceGivenException
                        ProcessInput(request.ContentEncoding, request.InputStream, request.ContentType, data, file)
                        responseData = NavigateMap(context, data, file)
                        'RaiseEvent OnRequest(String.Format("{0} - {1} : {2}" & vbCrLf & "{3}" & vbCrLf & vbCrLf & "{4}",
                        '                                   Now().ToShortTimeString,
                        '                                   request.HttpMethod,
                        '                                   request.Url.AbsoluteUri,
                        '                                   CStr(request.ContentType),
                        '                                   data))
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
                Thread.Sleep(3000)
                Listener.Start()
            End While
        Catch ex As Exception

        End Try

    End Sub

    Private Function NavigateMap(ByRef context As HttpListenerContext, data As String, file As MemoryStream) As Object
        Dim response = Nothing
        Dim path = context.Request.Url.AbsolutePath.Split({"/"}, StringSplitOptions.RemoveEmptyEntries)
        Try
            Select Case context.Request.HttpMethod
                Case "GET"
                    If context.Request.Url.Query = "?file" OrElse
                        GetContentType(context.Request.ContentType).StartsWith("image/") Then
                        Return Map(path(0)).GetFile(path(1), path(2))
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
                    response = Map(path(0)).GetFile(path(1), path(2)) : Return response
            End Select
        Catch ex As IndexOutOfRangeException
            Throw New MalformedUrlException
        End Try

        Return JsonConvert.SerializeObject(response, JSONSettings)
    End Function

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

    Private Overloads Sub SendResponse(ByRef response As HttpListenerResponse, data As MemoryStream)
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

    'Protected Function GetHashAsHex(ByVal data As Byte()) As String
    '    Dim hasher As SHA256 = SHA256.Create
    '    Return BitConverter.ToString(hasher.ComputeHash(data)).Replace("-", "").ToLower
    'End Function

    'Function AuthLogin(ByVal collection As FormCollection) As ActionResult
    '    Dim U As User = Nothing
    '    Dim username As String = Nothing
    '    Dim password As String = Nothing
    '    Try
    '        username = Request("username").ToLower
    '        password = Request("password")

    '        If Not String.IsNullOrEmpty(username) Or Not String.IsNullOrEmpty(password) Then
    '            U = (From e As User In C.Users
    '                 Where e.NetLogin.ToLower = username And e.LocalPhone = password
    '                 Select e).AsNoTracking.FirstOrDefault
    '        End If

    '        If U Is Nothing Then
    '            Return RedirectToAction("Login")
    '        End If

    '        Dim rand As New RNGCryptoServiceProvider()
    '        Dim tokenBytes As Byte() = New Byte(23) {} '32 base64 chars
    '        Dim selectorBytes As Byte() = New Byte(8) {} '12 base64 chars
    '        Dim base64Token As String
    '        Dim base64Selector As String
    '        Dim token As New Token
    '        Dim cookie As New HttpCookie("SmartDirAuthToken")

    '        rand.GetBytes(tokenBytes)
    '        rand.GetBytes(selectorBytes)
    '        base64Token = Convert.ToBase64String(tokenBytes)
    '        base64Selector = Convert.ToBase64String(selectorBytes)

    '        token.TokenHash = GetHashAsHex(tokenBytes)
    '        token.Selector = base64Selector
    '        token.UserID = U.UniqueID
    '        token.Expires = DateTime.UtcNow().AddDays(30)

    '        C.Tokens.Add(token)
    '        C.SaveChanges()

    '        Session("user") = U

    '        cookie.Value = base64Selector & "." & base64Token
    '        cookie.Expires = DateTime.UtcNow.AddDays(30)
    '        cookie.HttpOnly = True

    '        Response.SetCookie(cookie)

    '        If Request.Cookies("SmartDirFirstLogin") Is Nothing Then
    '            Dim firstlogin As New HttpCookie("SmartDirFirstLogin")
    '            firstlogin.Expires = DateTime.UtcNow.AddYears(10)
    '            Response.SetCookie(firstlogin)
    '            Return RedirectToAction("Help")
    '        End If

    '        Return RedirectToAction("Index")
    '    Catch ex As Exception
    '        LogMessage("Home", vbCrLf &
    '            "***************************" & vbCrLf &
    '            "AuthLogin *** ERROR ***" & vbCrLf &
    '            "MSG: " & ex.Message & vbCrLf &
    '            "USER: " & JsonConvert.SerializeObject(U) & vbCrLf &
    '            "USERNAME INPUT: " & username & vbCrLf &
    '            "IP ADDRESS: " & Request.UserHostAddress & vbCrLf &
    '            "***************************"
    '        )
    '    End Try

    '    Return RedirectToAction("Login")
    'End Function

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
