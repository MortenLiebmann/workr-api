Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Newtonsoft.Json

<Table("messages")>
Public Class Message
    Inherits Entity

    <Key>
    Public Overrides Property ID As Guid?
    Public Property ChatID As Guid?
    Public Property SentByUserID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property UpdatedDate As DateTime?
    Public Property Text As String
    Public Property MessageFlags As Int64?

    Public ReadOnly Property Chat() As Chat
        Get
            If Me.ChatID Is Nothing Then Return Nothing
            Return (From e As Chat In DB.Chats.AsNoTracking
                    Where e.ID = Me.ChatID
                    Select e).First
        End Get
    End Property

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

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function
End Class