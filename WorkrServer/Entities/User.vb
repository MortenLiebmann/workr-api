Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity
Imports WorkrServer

<Table("users")>
Public Class User
    Inherits Entity
    <Key>
    Public Overrides Property ID As Guid?
    Public Property Name As String
    Public Property Email As String
    Public Property PasswordHash As String
    Public Property Salt As String
    Public Property Address As String
    Public Property Business As String
    Public Property Phone As String
    Public Property Company As String
    Public Property AccountFlags As Int64?

    'Public Overrides Function Expand() As Object
    '    Return Me
    'End Function
End Class