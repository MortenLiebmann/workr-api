Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Newtonsoft.Json

''' <summary>
''' The PostImage entity class
''' This class is mapped to the "postimages" table in the database
''' Contains the database table "postimages" fields as propterties, marked with the attribute "Key"
''' Properties marked with the attribute "NotMapped" are mapped to a field in this entitys assosiated database table
''' Properties marked with the attribute "JsonIgnore" are not serialized or deserialized
''' </summary>
<Table("postimages")>
Public Class PostImage
    Inherits Entity

    'These are the table columns
    <Key>
    Public Overrides Property ID As Guid?
    Public Property PostID As Guid?

    ''' <summary>
    ''' Fetches the full Post entity using the "PostID" property
    ''' </summary>
    ''' <returns>A Post Entity</returns>
    <NotMapped>
    <JsonIgnore>
    Public ReadOnly Property Post() As Post
        Get
            Try
                If Me.PostID Is Nothing Then Return Nothing
                Return (From e As Post In DB.Posts.AsNoTracking
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

    ''' <summary>
    ''' PostImage specific code for Put operations
    ''' </summary>
    ''' <param name="params"></param>
    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If Not AuthUser.ID = Post.CreatedByUserID Then Throw New NotAuthorizedException("Can not add images to a post you didn't create.")
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
    End Sub

    ''' <summary>
    ''' Create a PostImage entity when an image is uploaded
    ''' </summary>
    ''' <param name="params"></param>
    ''' <returns></returns>
    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If params Is Nothing Then Throw New Exception("params is nothing")
        If Me.PostID Is Nothing Then Me.PostID = params.associatedEntityID
        If Not AuthUser.ID = Post.CreatedByUserID Then Throw New NotAuthorizedException("Can not add images to a post you didn't create.")
        Return CreateFileAssociatedPostImage(params.associatedEntityID)
    End Function

    ''' <summary>
    ''' Create a PostImage entity when an image is uploaded
    ''' </summary>
    ''' <param name="postID"></param>
    ''' <returns></returns>
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
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If Not AuthUser.ID = Post.CreatedByUserID Then Throw New NotAuthorizedException("Can not add images to a post you didn't create.")
        Dim postImage As PostImage = Nothing
        Try
            If Not FileUploadAllowed Then Throw New FileUploadNotAllowedException
            postImage = DirectCast(params, PostImage)
            Dim dbPostImage As PostImage = DB.PostImages.Add(postImage)
            DB.SaveChanges()
            Return dbPostImage
        Catch ex As Data.Entity.Infrastructure.DbUpdateException
            DB.DiscardTrackedEntity(params)
            Throw New IdNotFoundException(postImage.PostID.ToString, "posts")
        Catch ex As Exception
            DB.DiscardTrackedEntity(params)
            Throw New OnFileUploadException
        End Try
    End Function

    Public Overrides Function OnPatch(Optional params As Object = Nothing) As Boolean
        Return True
    End Function
End Class