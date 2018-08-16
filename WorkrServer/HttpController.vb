Imports System.Data.Entity
Imports System.IO
Imports System.Net
Imports System.Threading
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class HttpController
    Public Event JsonRecieved(ByVal json As String)
    Private Listener As HttpListener
    Private ListenThread As Thread

    Public Sub New(ByVal baseUrl As String, ByRef Map As Dictionary(Of String, UserTable))
        Helper.Map = Map
        Listener = New HttpListener()
        Listener.Prefixes.Add(baseUrl)
    End Sub

    Public Sub New(ByVal baseURLs As String(), ByRef Map As Dictionary(Of String, UserTable))
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
        Dim response As HttpListenerResponse
        Dim path As String()
        Dim postData As String
        Dim responseString As String

        While True
            While Listener.IsListening
                Try
                    context = Listener.GetContext
                    request = context.Request
                    response = context.Response
                    path = request.Url.AbsolutePath.Split({"/"}, StringSplitOptions.RemoveEmptyEntries)
                    postData = New StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd
                    RaiseEvent JsonRecieved(postData)
                    responseString = NavigateMap(path, request.HttpMethod, postData)
                    SendResponse(response, responseString)
                Catch ex As Exception
                    response.StatusCode = 500
                    SendResponse(response, ex.Message)
                End Try
            End While
            Thread.Sleep(3000)
            Listener.Start()
        End While
    End Sub

    Private Function NavigateMap(ByVal path As String(), ByVal method As String, Optional ByVal postData As String = "") As String
        Dim response = Nothing

        'Select Case method
        '    Case "GET"
        '        response = Map(path(0)).GetAll()
        '    Case "POST"
        '        response = Map(path(0)).Search(postData)
        '    Case "PUT"
        '        response = Map(path(0)).Put(postData)
        '    Case "DELETE"
        '        Map(path(0)).Delete(path(1))
        '    Case "PATCH"
        '        Map(path(0)).Modify(postData)
        'End Select

        Return JsonConvert.SerializeObject(response, JSONSettings)
    End Function

    Private Sub SendResponse(ByRef response As HttpListenerResponse, ByVal json As String)
        Dim responseBytes As Byte() = Text.Encoding.UTF8.GetBytes(json)
        response.ContentType = "application/json"
        response.ContentLength64 = responseBytes.Length
        response.OutputStream.Write(responseBytes, 0, responseBytes.Length)
        response.Close()
    End Sub

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
