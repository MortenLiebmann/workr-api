Imports System.Globalization
Imports System.Net
Imports Hotel
Imports Npgsql

Module Module1
    Private WithEvents h As New Hotel.WorkrServer
    Sub Main()
        'Dim x As New NpgsqlConnection()
        'x.ConnectionString = "Server=127.0.0.1;User Id=postgres;" &
        '                        "Password=123;Database=myDB;"
        'x.Open()
        'Dim reader As NpgsqlDataReader = New NpgsqlCommand("SELECT * FROM users", x).ExecuteReader

        'While reader.Read
        '    Console.WriteLine(reader("id") & " - " & reader("name"))
        'End While

        'Console.ReadLine()


        'Dim webClient As New WebClient
        'Dim params As New Specialized.NameValueCollection()
        h.Start()
        Console.ReadLine()
        'params.Add("p1", "value1")
        'webClient.UploadValues("http://127.0.0.1:9877/test", "POST", params)
        'Console.ReadLine()



    End Sub

    Sub lol(ByVal json As String) Handles h.jr
        Console.WriteLine(json)
    End Sub

End Module
