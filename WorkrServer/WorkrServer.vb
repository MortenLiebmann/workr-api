Imports System.Data.Entity
Imports Newtonsoft.Json
Imports Npgsql

Public Class WorkrServer
    Private WithEvents Controller As HttpController
    Public Event jr(ByVal json As String)
    Public Sub Start()
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

        'Dim U As User() = map("users").GetAll()

        'For Each e As User In U
        '    Console.WriteLine("{0} - {1}", e.Name, e.Email)
        'Next

        'U = map("users").Search(JsonConvert.SerializeObject(U(0)))

        'For Each e As User In U
        '    Console.WriteLine("{0} - {1}", e.Name, e.Email)
        'Next

        Controller = New HttpController({"http://127.0.0.1:9877/", "http://192.168.1.88:9877/"}, map)
        Controller.StartListening()
    End Sub

    Sub GotJSon(ByVal json As String) Handles Controller.JsonRecieved
        RaiseEvent jr(json)
    End Sub
End Class
