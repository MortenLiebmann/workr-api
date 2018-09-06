Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Reflection
Imports Newtonsoft.Json

<Table("postbids")>
Public Class PostBid
    Inherits Entity
    <Key>
    Public Overrides Property ID As Guid?
    Public Property PostID As Guid?
    Public Property CreatedByUserID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property Text As String
    Public Property Price As Decimal
    Public Property Flags As Int64?

    <Flags>
    Public Enum PostBidFlags As Int64
        POSTED = 0
        DELETED = 1
        REJECTED = 2
        ACCEPTED = 4
        COMPLETED = 8
    End Enum

    <NotMapped>
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


    Public Overrides ReadOnly Property TableName As String
        Get
            Return "postbids"
        End Get
    End Property

    Public Overrides ReadOnly Property FileUploadAllowed As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
        CreatedByUserID = AuthUser.ID
    End Sub

    Public Overrides Function OnPatch(Optional params As Object = Nothing) As Boolean
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If AuthUser.ID = Me.Post.CreatedByUserID Then
            Dim patchBid As PostBid = TryCast(params, PostBid)
            If patchBid Is Nothing Then Throw New NotAuthorizedException
            Me.Flags = patchBid.Flags
            Return False
        End If

        If AuthUser.ID <> Me.CreatedByUserID Then Throw New NotAuthorizedException
        Return True
    End Function

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function
End Class
