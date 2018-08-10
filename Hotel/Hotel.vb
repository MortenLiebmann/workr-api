Imports Npgsql

Public Class Hotel
    Private WithEvents Controller As HttpController
    Public Event jr(ByVal json As String)
    Public Sub Start()
        Dim map As New Dictionary(Of String, Object)
        Dim con As New NpgsqlConnection()
        con.ConnectionString = "Server=127.0.0.1;User Id=postgres;" &
                                "Password=123;Database=hoteldb;"
        con.Open()

        Dim rooms As New DbToRoom(con, "room")
        Dim guests As New DbToGuest(con, "guest")
        Dim bookings As New DbToBooking(con, "booking")
        Dim charges As New DbToCharge(con, "charge")
        Dim reservations As New DbToReservation(con, "reservation")
        map.Add(rooms.Path, rooms)
        map.Add(guests.Path, guests)
        map.Add(bookings.Path, bookings)
        map.Add(charges.Path, charges)
        map.Add(reservations.Path, reservations)
        Controller = New HttpController({"http://127.0.0.1:9877/"}, map)
        Controller.StartListening()
    End Sub

    Sub GotJSon(ByVal json As String) Handles Controller.JsonRecieved
        RaiseEvent jr(json)
    End Sub
End Class
