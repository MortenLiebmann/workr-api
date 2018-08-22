Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("postimages")>
Public Class PostImage
    Inherits Entity

    <Key>
    Public Overrides Property ID As Guid?
    Public Property PostID As Guid?
    Public Property Path As String

    Public ReadOnly Property Post() As Post
        Get
            If Me.PostID Is Nothing Then Return Nothing
            Return (From e As Post In DB.Posts
                    Where e.ID = Me.PostID
                    Select e).First
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
            .PostID = postID,
            .Path = ""}
    End Function

    ''' <summary>
    ''' Should be called after a post images has been uploaded.
    ''' </summary>
    ''' <param name="params">An instance of PostImage</param>
    ''' <returns></returns>
    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Try
            If Not FileUploadAllowed Then Throw New FileUploadNotAllowedException
            Dim postImage As PostImage = DirectCast(params, PostImage)
            Dim dbPostImage As PostImage = DB.PostImages.Add(postImage)
            DB.SaveChanges()
            Return dbPostImage
        Catch ex As Exception
            If params.GetType.GetProperty("Path") IsNot Nothing Then
                If IO.File.Exists(params.Path) Then IO.File.Delete(params.Path)
            End If
            Throw New OnFileUploadException
        End Try
        Return Nothing
    End Function

    Public Class OnFileUploadException
        Inherits Exception

        Public Overrides ReadOnly Property Message As String
            Get
                Return "An error occurred uploading a file."
            End Get
        End Property
    End Class
    Public Class FileUploadNotAllowedException
        Inherits Exception

        Public Overrides ReadOnly Property Message As String
            Get
                Return "File uploading is not allowed for this entity."
            End Get
        End Property
    End Class
End Class