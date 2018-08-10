Imports Hotel
Imports Npgsql

Public Class DbToGuest
    Inherits DbToItem(Of Guest)

    Public Overrides Function ReaderToItems() As Dictionary(Of Guid, Guest)
        Dim result As New Dictionary(Of Guid, Guest)

        While Reader.Read
            result.Add(Reader("id"),
               New Guest(Reader("id"),
                         Reader("gender"),
                         Reader("firstname"),
                         Reader("lastname"),
                         Reader("fullname"),
                         Reader("company"),
                         Reader("address"),
                         Reader("zip"),
                         Reader("city"),
                         Reader("country"),
                         CType(Reader("roomid"), Guid),
                         Reader("ischeckedin")
               )
            )

        End While

        Reader.Close()
        Return result
    End Function

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByRef dbCon As NpgsqlConnection, ByVal path As String)
        MyBase.New(dbCon, path)
    End Sub

    Public Overrides Sub DBGet()
        Cmd.CommandText = "SELECT * FROM guests"
        Reader = Cmd.ExecuteReader

        Items.Clear()
        Items = ReaderToItems()
    End Sub

    Public Overrides Sub DBRemove(obj As Guest)
        Cmd.CommandText = "DELETE FROM guests WHERE id = '" & obj.ID.ToString & "'"
        Cmd.ExecuteNonQuery()
    End Sub

    Public Overrides Sub DBModify(obj As Guest)
        Dim cmdText As String = "UPDATE guests SET(gender,firstname,lastname,fullname,company,address,zip,city,country,roomid,ischeckedin) = ($$VALUES$$) WHERE id = '" & obj.ID.ToString & "'"
        cmdText = cmdText.Replace("$$VALUES$$", obj.AsSqlString(False))
        Cmd.CommandText = cmdText
        Cmd.ExecuteNonQuery()
    End Sub

    Public Overrides Function Equals(x As Guest, y As Guest) As Boolean
        Return x.Equals(y)
    End Function

    Public Overrides Function DBAdd(obj As Guest) As Guest
        Try
            If obj.ID = Guid.Empty Then obj.ID = Guid.NewGuid

            Dim cmdText As String = "INSERT INTO guests VALUES($$VALUES$$)"
            cmdText = cmdText.Replace("$$VALUES$$", obj.AsSqlString(True))
            Cmd.CommandText = cmdText
            Cmd.ExecuteNonQuery()
            Return obj
        Catch ex As Exception
        End Try

        Return Nothing
    End Function
End Class
