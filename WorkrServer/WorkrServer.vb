Imports System.Data.Entity
Imports Npgsql

Public Class WorkrServer
    Private WithEvents Controller As HttpController
    Public Event jr(ByVal json As String)
    Public Sub Start()
        Dim map As New Dictionary(Of String, Object)
        'Dim con As New NpgsqlConnection()
        'con.ConnectionString = "Server=127.0.0.1;User Id=postgres;" &
        '                        "Password=workr123;Database=workrdb;"
        'con.Open()

        'Dim rooms As New DbToRoom(con, "room")
        'Dim guests As New DbToGuest(con, "guest")
        'Dim bookings As New DbToBooking(con, "booking")
        'Dim charges As New DbToCharge(con, "charge")
        'Dim reservations As New DbToReservation(con, "reservation")
        'map.Add(rooms.Path, rooms)
        'map.Add(guests.Path, guests)
        'map.Add(bookings.Path, bookings)
        'map.Add(charges.Path, charges)
        'map.Add(reservations.Path, reservations)

        map.Add("users", New UserTable(DB.Users))
        map.Add("ratings", New RatingTable(DB.Ratings))
        'Dim x As DbSet = DB.Users

        'map.Add("users", New Table(Of User)(DB.Users))
        'map.Add("ratings", New Table(Of Entity)(DB.Ratings))

        Dim U As User() = (From e As User In CType(map("users").DbSet, DbSet(Of User))
                           Where e.AccountFlags = 0
                           Select e).ToArray
        For Each e As User In U
            Console.WriteLine(e.Name)
        Next
        'map.Add("posts", DB.Posts)
        'map.Add("postimages", DB.PostImages)
        'map.Add("chats", DB.Chats)
        'map.Add("messages", DB.Messages)
        'map.Add("ratings", DB.Ratings)
        'Controller = New HttpController({"http://127.0.0.1:9877/"}, map)
        'Controller.StartListening()
    End Sub

    Sub GotJSon(ByVal json As String) Handles Controller.JsonRecieved
        RaiseEvent jr(json)
    End Sub
End Class
