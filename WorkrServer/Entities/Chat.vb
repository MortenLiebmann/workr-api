Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Newtonsoft.Json

''' <summary>
''' The Chat entity class
''' This class is mapped to the "chats" table in the database
''' Contains the database table "chats" fields as propterties, marked with the attribute "Key"
''' Properties marked with the attribute "NotMapped" are mapped to a field in this entitys assosiated database table
''' Properties marked with the attribute "JsonIgnore" are not serialized or deserialized
''' </summary>
<Table("chats")>
Public Class Chat
    Inherits Entity

    'These are the table columns
    <Key>
    Public Overrides Property ID As Guid?
    Public Property PostID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property ChatParty1UserID As Guid?
    Public Property ChatParty2UserID As Guid?
    Public Property Flags As Int64?

    ''' <summary>
    ''' Fetches the full Post entity using the "PostID" property
    ''' </summary>
    ''' <returns>a Post entity</returns>
    <JsonIgnore>
    <NotMapped>
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

    ''' <summary>
    ''' Fetches the full User entity using the "ChatParty1UserID" property
    ''' </summary>
    ''' <returns>A User entity</returns>
    <JsonIgnore>
    <NotMapped>
    Public ReadOnly Property ChatParty1User() As User
        Get
            Try
                If Me.ChatParty1UserID Is Nothing Then Return Nothing
                Return (From e As User In DB.Users.AsNoTracking
                        Where e.ID = Me.ChatParty1UserID
                        Select e).First
            Catch ex As InvalidOperationException
                Throw New IdNotFoundException(Me.ChatParty1UserID.ToString, "users")
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Fetches the full User entity using the "ChatParty2UserID" property
    ''' </summary>
    ''' <returns>A User Entity</returns>
    <JsonIgnore>
    <NotMapped>
    Public ReadOnly Property ChatParty2User() As User
        Get
            If Me.ChatParty2UserID Is Nothing Then Return Nothing
            Return (From e As User In DB.Users.AsNoTracking
                    Where e.ID = Me.ChatParty2UserID
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
            Return "chats"
        End Get
    End Property

    ''' <summary>
    ''' Chat specific code for Put operations
    ''' </summary>
    ''' <param name="params"></param>
    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
        ChatParty1UserID = params.ChatParty1UserID
        ChatParty2UserID = params.ChatParty2UserID
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