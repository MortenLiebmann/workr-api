Imports Hotel
Imports Npgsql

Public Class DbToCharge
    Inherits DbToItem(Of Charge)

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByRef dbCon As NpgsqlConnection, ByVal path As String)
        MyBase.New(dbCon, path)
    End Sub

    Public Overrides Sub DBGet()
        Cmd.CommandText = "SELECT * FROM charges"
        Reader = Cmd.ExecuteReader

        Items.Clear()
        Items = ReaderToItems()
    End Sub

    Public Overrides Sub DBRemove(obj As Charge)
        Cmd.CommandText = "DELETE FROM charges WHERE id = '" & obj.ID.ToString & "'"
        Cmd.ExecuteNonQuery()
    End Sub

    Public Overrides Sub DBModify(obj As Charge)
        Dim cmdText As String = "UPDATE charges SET(type,roomid,chargetime,units,pricenet,pricevat,pricetotal,description) = ($$VALUES$$) WHERE id = '" & obj.ID.ToString & "'"
        cmdText = cmdText.Replace("$$VALUES$$", obj.AsSqlString(False))
        Cmd.CommandText = cmdText
        Cmd.ExecuteNonQuery()
    End Sub

    Public Overrides Function Equals(x As Charge, y As Charge) As Boolean
        Return x.Equals(y)
    End Function

    Public Overrides Function ReaderToItems() As Dictionary(Of Guid, Charge)
        Dim result As New Dictionary(Of Guid, Charge)

        While Reader.Read
            result.Add(Reader("id"),
                            New Charge(Reader("id"),
                                  Reader("type"),
                                  CType(Reader("roomid"), Guid),
                                  Reader("chargetime"),
                                  Reader("units"),
                                  Reader("pricenet"),
                                  Reader("pricevat"),
                                  Reader("pricetotal"),
                                  Reader("description")
                                )
                            )
        End While

        Reader.Close()
        Return result
    End Function

    Public Overrides Function DBAdd(obj As Charge) As Charge
        Try
            If obj.ID = Guid.Empty Then obj.ID = Guid.NewGuid

            Dim cmdText As String = "INSERT INTO charges VALUES($$VALUES$$)"
            cmdText = cmdText.Replace("$$VALUES$$", obj.AsSqlString(True))
            Cmd.CommandText = cmdText
            Cmd.ExecuteNonQuery()
            Return obj
        Catch ex As Exception
        End Try

        Return Nothing
    End Function

End Class
