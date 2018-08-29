Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity
Imports System.Reflection
Imports Newtonsoft.Json

<Table("posts")>
Public Class Post
    Inherits Entity

    <Key>
    Public Overrides Property ID As Guid?
    Public Property CreatedByUserID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property Title As String
    Public Property Description As String
    Public Property Address As String
    Public Property JobEndDate As DateTime?
    Public Property Flags As Int64?

    Public ReadOnly Property CreatedByUser As User
        Get
            Try
                If Me.CreatedByUserID Is Nothing Then Return Nothing
                Return (From e As User In DB.Users.AsNoTracking
                        Where e.ID = Me.CreatedByUserID
                        Select e).First
            Catch ex As InvalidOperationException
                Throw New IdNotFoundException(Me.CreatedByUserID.ToString, "users")
            End Try
        End Get
    End Property

    Public ReadOnly Property PostImageIDs As Guid?()
        Get
            Return (From e As PostImage In DB.PostImages.AsNoTracking
                    Where e.PostID = Me.ID
                    Select e.ID).ToArray
        End Get
    End Property

    Public Property PostTags As PostTag()
        Get
            Return (From t As PostTag In DB.PostTags.AsNoTracking
                    Join tr As PostTagReference In DB.PostTagReferences.AsNoTracking
                    On tr.PostTagID Equals t.ID
                    Where tr.PostID = Me.ID
                    Select t).ToArray
        End Get
        Set(value As PostTag())
            CreatePostTagEntitys(value)
        End Set
    End Property

    Private m_Tags As String()
    Public Property Tags As String()
        Get
            If m_Tags IsNot Nothing Then Return m_Tags
            Return (From t As PostTag In DB.PostTags.AsNoTracking
                    Join tr As PostTagReference In DB.PostTagReferences.AsNoTracking
                    On tr.PostTagID Equals t.ID
                    Where tr.PostID = Me.ID
                    Select t.Name).ToArray
        End Get
        Set(value As String())
            m_Tags = value
        End Set
    End Property

    Private Sub CreatePostTagEntitys(postTags As PostTag())
        If Me.ID Is Nothing OrElse Me.ID = Guid.Empty Then Me.ID = Guid.NewGuid
        Dim newPostTags As New List(Of PostTag)
        For Each tag As PostTag In postTags
            If (From e As PostTag In DB.PostTags
                Where e.Name = tag.Name
                Select e).Count > 0 Then Continue For
            tag.ID = Guid.NewGuid
            Dim dbEntity As PostTag = Nothing
            Try
                dbEntity = DB.PostTags.Add(tag)
                newPostTags.Add(dbEntity)
            Catch ex As Exception
                DB.DiscardTrackedEntityByID(dbEntity.ID)
            End Try
        Next
        CreatePostTagReferenceEntitys(newPostTags.ToArray)
    End Sub

    Private Sub CreatePostTagReferenceEntitys(postTags As PostTag())
        For Each e As PostTag In postTags
            Dim dbEntity As PostTagReference = Nothing
            Try
                dbEntity = DB.PostTagReferences.Add(New PostTagReference With {
                                                    .ID = Guid.NewGuid,
                                                    .PostID = Me.ID,
                                                    .PostTagID = e.ID})
            Catch ex As Exception
                DB.DiscardTrackedEntityByID(dbEntity.ID)
            End Try
        Next
    End Sub

    Public Overrides ReadOnly Property FileUploadAllowed As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "posts"
        End Get
    End Property

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function
End Class