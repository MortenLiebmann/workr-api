Imports System.Data.Entity
Imports Newtonsoft.Json
Imports Npgsql

Public Class WorkrServer
    Private WithEvents Controller As HttpController

    Public Sub Start()

        Dim map As New Dictionary(Of String, Object) From {
            {"users", New Resource(Of User)(DB.Users)},
            {"userimages", New Resource(Of UserImage)(DB.UserImages)},
            {"posts", New Resource(Of Post)(DB.Posts)},
            {"postimages", New Resource(Of PostImage)(DB.PostImages)},
            {"chats", New Resource(Of Chat)(DB.Chats)},
            {"messages", New Resource(Of Message)(DB.Messages)},
            {"ratings", New Resource(Of Rating)(DB.Ratings)},
            {"posttags", New Resource(Of PostTag)(DB.PostTags)},
            {"posttagreferences", New Resource(Of PostTagReference)(DB.PostTagReferences)},
            {"postbids", New Resource(Of PostBid)(DB.PostBids)}
        }

        Controller = New HttpController({"http://127.0.0.1:9877/", "http://10.0.0.37:9877/", "http://skurk.info:9877/"}, map)
        Controller.StartListening()
    End Sub

    Private Sub OnRequest(ByVal data As String) Handles Controller.OnRequest
        Console.WriteLine(data)
        Console.WriteLine("--------------------------------------------------------------------------")
    End Sub
End Class


