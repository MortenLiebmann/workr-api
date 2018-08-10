Imports Hotel
Imports Npgsql

Public Class DbToReservation
    Inherits DbToItem(Of Reservation)

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByRef dbCon As NpgsqlConnection, ByVal path As String)
        MyBase.New(dbCon, path)
    End Sub

    Public Overrides Sub DBGet()
        Cmd.CommandText = "SELECT * FROM reservations"
        Reader = Cmd.ExecuteReader

        Items.Clear()
        Items = ReaderToItems()
    End Sub

    Public Overrides Sub DBRemove(obj As Reservation)
        Cmd.CommandText = "DELETE FROM reservations WHERE id = '" & obj.ID.ToString & "'"
        Cmd.ExecuteNonQuery()
    End Sub

    Public Overrides Sub DBModify(obj As Reservation)
        Dim cmdText As String = "UPDATE reservations SET(starttime,endtime,roomid) = ($$VALUES$$) WHERE id = '" & obj.ID.ToString & "'"
        cmdText = cmdText.Replace("$$VALUES$$", obj.AsSqlString(False))
        Cmd.CommandText = cmdText
        Cmd.ExecuteNonQuery()
    End Sub

    Public Overrides Function Equals(x As Reservation, y As Reservation) As Boolean
        Return x.Equals(y)
    End Function

    Public Overrides Function ReaderToItems() As Dictionary(Of Guid, Reservation)
        Dim result As New Dictionary(Of Guid, Reservation)

        While Reader.Read
            result.Add(Reader("id"),
               New Reservation(Reader("id"),
                               Reader("starttime"),
                               Reader("endtime"),
                               CType(Reader("roomid"), Guid)
                )
            )
        End While

        Reader.Close()
        Return result
    End Function

    Public Overrides Function DBAdd(obj As Reservation) As Reservation
        Try
            If obj.ID = Guid.Empty Then obj.ID = Guid.NewGuid

            Dim cmdText As String = "INSERT INTO reservations VALUES($$VALUES$$)"
            cmdText = cmdText.Replace("$$VALUES$$", obj.AsSqlString(True))
            Cmd.CommandText = cmdText
            Cmd.ExecuteNonQuery()
            Return obj
        Catch ex As Exception
        End Try

        Return Nothing
    End Function
End Class
