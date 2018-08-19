Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("messages")>
Public Class Message
    Inherits Entity
    <Key>
    Public Overrides Property ID As Guid?
    Public Property ChatID As Guid?
    Public Property SentByUserID As Guid?
    Public Property CreatedDate As DateTime
    Public Property UpdatedDate As DateTime?
    Public Property Text As String
    Public Property MessageFlags As Int64?

    Public Overrides Function Expand() As Object
        Return New With {.Message = Me, Chat(), SentByUser()
        }
    End Function

    Public Function Chat() As Chat
        Return (From e As Chat In DB.Chats
                Where e.ID = Me.ChatID
                Select e).First
    End Function

    Public Function SentByUser() As User
        Return (From e As User In DB.Users
                Where e.ID = Me.SentByUserID
                Select e).First
    End Function
End Class