Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Newtonsoft.Json

<Table("postimages")>
Public Class PostImage
    Inherits Entity

    <Key>
    Public Overrides Property ID As Guid?
    Public Property PostID As Guid?

    <JsonIgnore>
    Public ReadOnly Property Post() As Post
        Get
            Try
                If Me.PostID Is Nothing Then Return Nothing
                Return (From e As Post In DB.Posts
                        Where e.ID = Me.PostID
                        Select e).First
            Catch ex As InvalidOperationException
                Throw New IdNotFoundException(Me.PostID.ToString, "posts")
            End Try
        End Get
    End Property

    Public Overrides ReadOnly Property FileUploadAllowed As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "postimages"
        End Get
    End Property

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        If params Is Nothing Then Throw New Exception("params is nothing")
        Return CreateFileAssociatedPostImage(params.associatedEntityID)
    End Function

    Private Function CreateFileAssociatedPostImage(postID As Guid) As PostImage
        Return New PostImage With {
            .ID = Guid.NewGuid,
            .PostID = postID}
    End Function

    ''' <summary>
    ''' Should be called after a post images has been uploaded.
    ''' </summary>
    ''' <param name="params">An instance of PostImage</param>
    ''' <returns></returns>
    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Dim postImage As PostImage = Nothing
        Try
            If Not FileUploadAllowed Then Throw New FileUploadNotAllowedException
            postImage = DirectCast(params, PostImage)
            Dim dbPostImage As PostImage = DB.PostImages.Add(PostImage)
            DB.SaveChanges()
            Return dbPostImage
        Catch ex As Data.Entity.Infrastructure.DbUpdateException
            Throw New IdNotFoundException(postImage.PostID.ToString, postImage.Post.TableName)
        Catch ex As Exception
            Throw New OnFileUploadException
        End Try
    End Function
End Class