Imports System.Data.Entity
Imports Newtonsoft.Json
Imports Npgsql

Public Class WorkrServer
    Private WithEvents Controller As HttpController

    Public Sub Start()

        'Dim x As New IO.FileStream("C:\Users\rune\Pictures\33333.jpg", IO.FileMode.Open)
        'Dim y As New IO.MemoryStream
        'x.CopyTo(y)
        'FTPUpload({"f1", "f2"}, "img.png", y)
        'Dim x = FTPDownload("postimages/c46057dc-0061-4d1c-bb14-247eee3a0ee4/fc495a0f-abf2-4e50-9f11-4096aa726770.png")

        Dim map As New Dictionary(Of String, Object) From {
            {"users", New Table(Of User)(DB.Users)},
            {"userimages", New Table(Of UserImage)(DB.UserImages)},
            {"posts", New Table(Of Post)(DB.Posts)},
            {"postimages", New Table(Of PostImage)(DB.PostImages)},
            {"chats", New Table(Of Chat)(DB.Chats)},
            {"messages", New Table(Of Message)(DB.Messages)},
            {"ratings", New Table(Of Rating)(DB.Ratings)},
            {"posttags", New Table(Of PostTag)(DB.PostTags)},
            {"posttagreferences", New Table(Of PostTagReference)(DB.PostTagReferences)},
            {"postbids", New Table(Of PostBid)(DB.PostBids)}
        }

        Controller = New HttpController({"http://127.0.0.1:9877/", "http://skurk.info:9877/"}, map)
        Controller.StartListening()
    End Sub

    Private Sub OnRequest(ByVal data As String) Handles Controller.OnRequest
        Console.WriteLine(data)
        Console.WriteLine("--------------------------------------------------------------------------")
    End Sub
End Class
