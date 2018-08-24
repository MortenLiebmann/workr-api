Imports System.Data.Entity
Imports Newtonsoft.Json
Imports Npgsql

Public Class WorkrServer
    Private WithEvents Controller As HttpController

    Public Sub Start()
        Dim map As New Dictionary(Of String, Object) From {
            {"users", New Table(Of User)(DB.Users)},
            {"posts", New Table(Of Post)(DB.Posts)},
            {"postimages", New Table(Of PostImage)(DB.PostImages)},
            {"chats", New Table(Of Chat)(DB.Chats)},
            {"messages", New Table(Of Message)(DB.Messages)},
            {"ratings", New Table(Of Rating)(DB.Ratings)},
            {"posttags", New Table(Of PostTag)(DB.PostTags)},
            {"posttagreferences", New Table(Of PostTagReferences)(DB.PostTagReferences)},
            {"postbids", New Table(Of PostBid)(DB.PostBids)}
        }

        Controller = New HttpController({"http://127.0.0.1:9877/", "http://192.168.1.88:9877/"}, map)
        Controller.StartListening()
    End Sub

    Private Sub OnRequest(ByVal data As String) Handles Controller.OnRequest
        Console.WriteLine("--------------------------------------------------------------------------")
        Console.WriteLine(data)
        Console.WriteLine("--------------------------------------------------------------------------")
    End Sub
End Class
