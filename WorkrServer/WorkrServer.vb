Imports System.Data.Entity
Imports Newtonsoft.Json
Imports Npgsql

Public Class WorkrServer
    Private WithEvents Controller As HttpController

    Public Sub Start()
        CreatePostImagesFolder()

        Dim map As New Dictionary(Of String, Object) From {
            {"users", New Table(Of User)(DB.Users)},
            {"posts", New Table(Of Post)(DB.Posts)},
            {"postimages", New Table(Of PostImage)(DB.PostImages)},
            {"chats", New Table(Of Chat)(DB.Chats)},
            {"messages", New Table(Of Message)(DB.Messages)},
            {"ratings", New Table(Of Rating)(DB.Ratings)},
            {"tags", New Table(Of Tag)(DB.Tags)},
            {"tagreferences", New Table(Of TagReference)(DB.TagReferences)}
        }

        Controller = New HttpController({"http://127.0.0.1:9877/", "http://skurk.info:9877/"}, map)
        Controller.StartListening()
    End Sub

    Private Sub OnRequest(ByVal data As String) Handles Controller.OnRequest
        Console.WriteLine("--------------------------------------------------------------------------")
        Console.WriteLine(data)
        Console.WriteLine("--------------------------------------------------------------------------")
    End Sub

    Private Sub CreatePostImagesFolder()
        If Not IO.Directory.Exists(Environment.CurrentDirectory & "\postimages") Then MkDir(Environment.CurrentDirectory & "\postimages")
    End Sub
End Class
