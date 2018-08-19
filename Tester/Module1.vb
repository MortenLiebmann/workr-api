Imports System.Globalization
Imports System.Net
Imports Npgsql
Imports WorkrServer

Module Module1
    Private workr As New WorkrServer.WorkrServer
    Sub Main()
        workr.Start()
        Console.ReadLine()
    End Sub
End Module
