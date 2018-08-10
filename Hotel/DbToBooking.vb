Imports Hotel
Imports Npgsql

Public Class DbToBooking
    Inherits DbToItem(Of Booking)

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByRef dbCon As NpgsqlConnection, ByVal path As String)
        MyBase.New(dbCon, path)
    End Sub

    Public Overrides Sub DBGet()
        Cmd.CommandText = "SELECT * FROM bookings"
        Reader = Cmd.ExecuteReader

        Items.Clear()
        Items = ReaderToItems()
    End Sub

    Public Overrides Sub DBRemove(obj As Booking)
        Cmd.CommandText = "DELETE FROM bookings WHERE id = '" & obj.ID.ToString & "'"
        Cmd.ExecuteNonQuery()
    End Sub

    Public Overrides Sub DBModify(obj As Booking)
        Dim cmdText As String = "UPDATE bookings SET(reservationids,guestids,creditcardname,creditcardtype,creditcardnumber,state) = ($$VALUES$$) WHERE id = '" & obj.ID.ToString & "'"
        cmdText = cmdText.Replace("$$VALUES$$", obj.AsSqlString(False))
        Cmd.CommandText = cmdText
        Cmd.ExecuteNonQuery()
    End Sub

    Public Overrides Function Equals(x As Booking, y As Booking) As Boolean
        Return x.Equals(y)
    End Function

    Public Overrides Function ReaderToItems() As Dictionary(Of Guid, Booking)
        Dim result As New Dictionary(Of Guid, Booking)

        While Reader.Read
            result.Add(Reader("id"),
               New Booking(Reader("id"),
                           CType(Reader("reservationids"), Guid()),
                           CType(Reader("guestids"), Guid()),
                           Reader("creditcardname"),
                           Reader("creditcardtype"),
                           Reader("creditcardnumber"),
                           Reader("state")
                )
            )
        End While

        Reader.Close()
        Return result
    End Function

    Public Overrides Function DBAdd(obj As Booking) As Booking
        Try
            If obj.ID = Guid.Empty Then obj.ID = Guid.NewGuid

            Dim cmdText As String = "INSERT INTO bookings VALUES($$VALUES$$)"
            cmdText = cmdText.Replace("$$VALUES$$", obj.AsSqlString(True))
            Cmd.CommandText = cmdText
            Cmd.ExecuteNonQuery()
            Return obj
        Catch ex As Exception
        End Try

        Return Nothing
    End Function

End Class
