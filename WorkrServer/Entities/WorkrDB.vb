Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure

Public Class WorkrDB
    Inherits DbContext

    Public Sub New()
        MyBase.New("name=PGSQL")
        'Me.Database.Log = New Action(Of String)(Sub(e) Debug.WriteLine(e))
    End Sub

    Protected Overrides Sub OnModelCreating(modelBuilder As DbModelBuilder)
        modelBuilder.HasDefaultSchema("public")
    End Sub

    Public Sub DiscardTrackedEntityByID(ent As Entity)
        Try
            Me.ChangeTracker.Entries.Where(Function(e) e.Entity.ID = ent.ID).First.State = EntityState.Detached
        Catch ex As Exception
        End Try
    End Sub

    Public Property Users As DbSet(Of User)
    Public Property UserImages As DbSet(Of UserImage)
    Public Property Posts As DbSet(Of Post)
    Public Property PostImages As DbSet(Of PostImage)
    Public Property Chats As DbSet(Of Chat)
    Public Property Messages As DbSet(Of Message)
    Public Property Ratings As DbSet(Of Rating)
    Public Property PostTags As DbSet(Of PostTag)
    Public Property PostTagReferences As DbSet(Of PostTagReference)
    Public Property PostBids As DbSet(Of PostBid)
End Class

