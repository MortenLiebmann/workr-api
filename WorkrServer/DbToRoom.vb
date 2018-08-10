Imports Hotel
Imports Npgsql

Public Class DbToRoom
    Inherits DbToItem(Of Room)

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByRef dbCon As NpgsqlConnection, ByVal path As String)
        MyBase.New(dbCon, path)
    End Sub

    Public Overrides Function ReaderToItems() As Dictionary(Of Guid, Room)
        Dim result As New Dictionary(Of Guid, Room)

        While Reader.Read
            result.Add(Reader("id"),
               New Room(Reader("id"),
                        Reader("number"),
                        Reader("view"),
                        Reader("bedtype"),
                        Reader("pricing"),
                        Reader("state"),
                        Reader("extras"))
            )
        End While

        Reader.Close()
        Return result
    End Function

    Public Overrides Function Equals(x As Room, y As Room) As Boolean
        Return x.Equals(y)
    End Function

    Public Overrides Sub DBGet()
        Cmd.CommandText = "SELECT * FROM rooms"
        Reader = Cmd.ExecuteReader

        Items.Clear()
        Items = ReaderToItems()
    End Sub

    Public Overrides Sub DBRemove(obj As Room)
        Cmd.CommandText = "DELETE FROM rooms WHERE id = '" & obj.ID.ToString & "'"
        Cmd.ExecuteNonQuery()
    End Sub

    Public Overrides Sub DBModify(obj As Room)
        Dim cmdText As String = "UPDATE rooms SET(number,view,bedtype,pricing,state,extras) = ($$VALUES$$) WHERE id = '" & obj.ID.ToString & "'"
        cmdText = cmdText.Replace("$$VALUES$$", obj.AsSqlString(False))
        Cmd.CommandText = cmdText
        Cmd.ExecuteNonQuery()
    End Sub

    Public Overrides Function DBAdd(obj As Room) As Room
        Try
            If obj.ID = Guid.Empty Then obj.ID = Guid.NewGuid

            Dim cmdText As String = "INSERT INTO rooms VALUES($$VALUES$$)"
            cmdText = cmdText.Replace("$$VALUES$$", obj.AsSqlString(True))
            Cmd.CommandText = cmdText
            Cmd.ExecuteNonQuery()
            Return obj
        Catch ex As Exception
        End Try

        Return Nothing
    End Function

End Class
