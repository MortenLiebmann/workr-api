Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Newtonsoft.Json

''' <summary>
''' The Rating entity class
''' This class is mapped to the "ratings" table in the database
''' Contains the database table "ratings" fields as propterties, marked with the attribute "Key"
''' Properties marked with the attribute "NotMapped" are mapped to a field in this entitys assosiated database table
''' Properties marked with the attribute "JsonIgnore" are not serialized or deserialized
''' </summary>
<Table("ratings")>
Public Class Rating
    Inherits Entity

    'These are the table columns
    <Key>
    Public Overrides Property ID As Guid?
    Public Property UserID As Guid?
    Public Property RatedByUserID As Guid?
    Public Property PostID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property Score As Int16?
    Public Property Text As String
    Public Property Flags As Int64?

    ''' <summary>
    ''' Fetches the full User entity using the "UserID" property
    ''' </summary>
    ''' <returns>A User entity</returns>
    <JsonIgnore>
    <NotMapped>
    Public ReadOnly Property User() As User
        Get
            Try
                If Me.UserID Is Nothing Then Return Nothing
                Return (From e As User In DB.Users
                        Where e.ID = Me.UserID
                        Select e).First
            Catch ex As InvalidOperationException
                Throw New IdNotFoundException(Me.UserID.ToString, "users")
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Fetches the full User entity using the "RatedByUserID" property
    ''' </summary>
    ''' <returns>A User entity</returns>
    <NotMapped>
    Public ReadOnly Property RatedByUser() As User
        Get
            If Me.RatedByUserID Is Nothing Then Return Nothing
            Return (From e As User In DB.Users.AsNoTracking
                    Where e.ID = Me.RatedByUserID
                    Select e).First
        End Get
    End Property

    ''' <summary>
    ''' Fetches the full Post entity using the "PostID" property
    ''' </summary>
    ''' <returns>A Post Entity</returns>
    <JsonIgnore>
    <NotMapped>
    Public ReadOnly Property Post() As Post
        Get
            If Me.PostID Is Nothing Then Return Nothing
            Return (From e As Post In DB.Posts.AsNoTracking
                    Where e.ID = Me.PostID
                    Select e).First
        End Get
    End Property

    Public Overrides ReadOnly Property FileUploadAllowed As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "ratings"
        End Get
    End Property

    ''' <summary>
    ''' Rating specific code for Put operations
    ''' </summary>
    ''' <param name="params"></param>
    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If Not AuthUser.ID = Post.CreatedByUserID Then Throw New NotAuthorizedException("Can not rate work done on a post you did not create.")
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
        RatedByUserID = AuthUser.ID
    End Sub

    Public Overrides Function OnPatch(Optional params As Object = Nothing) As Boolean
        Return True
    End Function

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function
End Class