Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure

Public Class WorkrDB
    Inherits DbContext

    Public Sub New()
        MyBase.New("name=PGSQL")
    End Sub

    Protected Overrides Sub OnModelCreating(modelBuilder As DbModelBuilder)
        modelBuilder.HasDefaultSchema("public")
    End Sub

    Public Sub DiscardTrackedEntityByID(id As Guid)
        Try
            Me.ChangeTracker.Entries.Where(Function(e) e.Entity.ID = id).First.State = EntityState.Detached
        Catch ex As Exception
        End Try
    End Sub

    Public Property Users As DbSet(Of User)
    Public Property Posts As DbSet(Of Post)
    Public Property PostImages As DbSet(Of PostImage)
    Public Property Chats As DbSet(Of Chat)
    Public Property Messages As DbSet(Of Message)
    Public Property Ratings As DbSet(Of Rating)
    Public Property Tags As DbSet(Of Tag)
    Public Property TagReferences As DbSet(Of TagReference)
End Class

