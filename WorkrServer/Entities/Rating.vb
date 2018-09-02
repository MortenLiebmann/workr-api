Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity
Imports Newtonsoft.Json

<Table("ratings")>
Public Class Rating
    Inherits Entity

    <Key>
    Public Overrides Property ID As Guid?
    Public Property UserID As Guid?
    Public Property RatedByUserID As Guid?
    Public Property PostID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property Score As Int16
    Public Property Text As String
    Public Property Flags As Int64?

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

    Public ReadOnly Property RatedByUser() As User
        Get
            If Me.RatedByUserID Is Nothing Then Return Nothing
            Return (From e As User In DB.Users.AsNoTracking
                    Where e.ID = Me.RatedByUserID
                    Select e).First
        End Get
    End Property

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

    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If Not AuthUser.ID = Post.CreatedByUserID Then Throw New NotAuthorizedException("Can not rate work done on a post you did not create.")
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
        RatedByUserID = AuthUser.ID
    End Sub

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function
End Class