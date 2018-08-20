Imports System.Data.Entity
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

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
        Dim path As String()
        Dim data As String = Nothing
        Dim responseString As String

        While True
            While Listener.IsListening
                Try
                    context = Listener.GetContext
                    request = context.Request
                    response = context.Response
                    path = request.Url.AbsolutePath.Split({"/"}, StringSplitOptions.RemoveEmptyEntries)

                    If context.Request.ContentType.StartsWith("multipart/form-data") Then
                        SaveFile(context.Request.ContentEncoding, GetBoundary(context.Request.ContentType), context.Request.InputStream)
                        data = ""
                        responseString = "200 OK"
                    Else
                        data = New StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd
                        responseString = NavigateMap(path, request.HttpMethod, data)
                    End If

                    RaiseEvent OnRequest(String.Format("{0} - {1} : {2}" & vbCrLf & "{3}" & vbCrLf & vbCrLf & "{4}",
                                                       Now().ToShortTimeString,
                                                       context.Request.HttpMethod,
                                                       request.Url.AbsoluteUri,
                                                       context.Request.ContentType.Split(";")(0),
                                                       data))
                    SendResponse(response, responseString)
                Catch ex As KeyNotFoundException
                    response.StatusCode = 404
                    SendResponse(response, ex.Message)
                Catch ex As IndexOutOfRangeException
                    response.StatusCode = 405
                    SendResponse(response, ex.Message)
                Catch ex As InvalidOperationException
                    response.StatusCode = 404
                    SendResponse(response, "ID not found.")
                Catch ex As Exception
                    response.StatusCode = 500
                    SendResponse(response, ex.Message)
                End Try


            End While
            Thread.Sleep(3000)
            Listener.Start()
        End While
    End Sub

    Private Function NavigateMap(path As String(), method As String, Optional data As String = "") As String
        Dim response = Nothing

        Select Case method
            Case "GET"
                If path.Length > 1 Then response = Map(path(0)).GetByID(path(1)) : Exit Select
                response = Map(path(0)).GetAll()
            Case "POST"
                response = Map(path(0)).Search(data)
            Case "PUT"
                response = Map(path(0)).Put(data)
            Case "DELETE"
                response = Map(path(0)).Delete(path(1))
            Case "PATCH"
                response = Map(path(0)).Patch(path(1), data)
        End Select

        Return JsonConvert.SerializeObject(response, JSONSettings)
    End Function

    Private Sub SendResponse(ByRef response As HttpListenerResponse, ByVal data As String)
        Dim responseBytes As Byte() = Encoding.UTF8.GetBytes(data)
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
