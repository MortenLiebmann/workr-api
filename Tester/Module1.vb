Imports System.Globalization
Imports System.Net
Imports Npgsql
Imports WorkrServer

Module Module1
    Private workr As New WorkrServer.WorkrServer
    Sub Main()
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Cyan
        workr.Start()
        While True
            Console.ReadLine()
        End While
    End Sub
End Module
