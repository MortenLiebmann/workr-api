Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity

<Table("ratings")>
Public Class Rating
    Inherits Entity
    <Key>
    Public Property ID As Guid
    Public Property UserID As Guid
    Public Property RatedByUserID As Guid
    Public Property PostID As Guid
    Public Property CreatedDate As DateTime
    Public Property Score As Int16
    Public Property Text As String
End Class

Public Class RatingTable
    Inherits Table(Of Rating)

    Private m_DbSet As DbSet(Of Rating)

    Public Sub New(ByRef dbset As DbSet(Of Rating))
        m_DbSet = dbset
    End Sub

    Public Overrides ReadOnly Property DbSet As DbSet(Of Rating)
        Get
            Return m_DbSet
        End Get
    End Property
End Class