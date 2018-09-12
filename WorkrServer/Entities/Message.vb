Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

''' <summary>
''' The Message entity class
''' This class is mapped to the "messages" table in the database
''' Contains the database table "messages" fields as propterties, marked with the attribute "Key"
''' Properties marked with the attribute "NotMapped" are mapped to a field in this entitys assosiated database table
''' Properties marked with the attribute "JsonIgnore" are not serialized or deserialized
''' </summary>
<Table("messages")>
Public Class Message
    Inherits Entity

    'These are the table columns
    <Key>
    Public Overrides Property ID As Guid?
    Public Property ChatID As Guid?
    Public Property SentByUserID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property UpdatedDate As DateTime?
    Public Property Text As String
    Public Property Flags As Int64?

    ''' <summary>
    ''' Fetches the full Chat entity using the "ChatID" property
    ''' </summary>
    ''' <returns></returns>
    <NotMapped>
    Public ReadOnly Property Chat() As Chat
        Get
            If Me.ChatID Is Nothing Then Return Nothing
            Return (From e As Chat In DB.Chats.AsNoTracking
                    Where e.ID = Me.ChatID
                    Select e).First
        End Get
    End Property

    ''' <summary>
    ''' Fetches the full User entity using the "SentByUserID" property
    ''' </summary>
    ''' <returns></returns>
    <NotMapped>
    Public ReadOnly Property SentByUser() As User
        Get
            If Me.SentByUserID Is Nothing Then Return Nothing
            Return (From e As User In DB.Users.AsNoTracking
                    Where e.ID = Me.SentByUserID
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
            Return "messages"
        End Get
    End Property

    ''' <summary>
    ''' Message specific code for Put operations
    ''' </summary>
    ''' <param name="params"></param>
    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
        SentByUserID = AuthUser.ID
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