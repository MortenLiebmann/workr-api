Imports System.Data.Entity

''' <summary>
''' This class inherits EntityFramework and handeles database communication.
''' I holds references to the tables in the database in the form of DbSets(Of Entity)
''' </summary>
Public Class WorkrDB
    Inherits DbContext

    Public Sub New()
        'Sets the name of the connectionstring in App.Config to use
        MyBase.New("name=PGSQL")
    End Sub

    Protected Overrides Sub OnModelCreating(modelBuilder As DbModelBuilder)
        'Sets the default schema.
        'A PostgreSQL database has a default schema called "public"
        modelBuilder.HasDefaultSchema("public")
    End Sub

    ''' <summary>
    ''' Used to discard entitys that have been added but should not be saved.
    ''' </summary>
    ''' <param name="ent">the entity to discard</param>
    Public Sub DiscardTrackedEntity(ent As Entity)
        Try
            Me.ChangeTracker.Entries.Where(Function(e) e.Entity.ID = ent.ID).First.State = EntityState.Detached
        Catch ex As Exception
        End Try
    End Sub

    'These are DbSets(Of Entity) that are mapped to the appropriate tables in the database
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

