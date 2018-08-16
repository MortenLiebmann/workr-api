Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity
Imports WorkrServer

<Table("users")>
Public Class User
    Inherits Entity
    <Key>
    Public Property ID As Guid
    Public Property Name As String
    Public Property Email As String
    Public Property PasswordHash As String
    Public Property Salt As String
    Public Property Address As String
    Public Property Business As String
    Public Property Phone As String
    Public Property Company As String
    Public Property AccountFlags As Int64
End Class

Public Class UserTable
    Inherits Table(Of User)

    Private m_DbSet As DbSet(Of User)

    Public Sub New(ByRef dbset As DbSet(Of User))
        m_DbSet = dbset
    End Sub

    Public Overrides ReadOnly Property DbSet As DbSet(Of User)
        Get
            Return m_DbSet
        End Get
    End Property
End Class