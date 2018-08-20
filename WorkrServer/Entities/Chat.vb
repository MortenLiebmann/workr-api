Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("chats")>
Public Class Chat
    Inherits Entity
    <Key>
    Public Overrides Property ID As Guid?
    Public Property PostID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property ChatParty1UserID As Guid?
    Public Property ChatParty2UserID As Guid?

    'Public Overrides Function Expand() As Object
    '    Return New With {.Chat = Me, Post(), ChatParty1User(), ChatParty2User()
    '    }
    'End Function

    Public ReadOnly Property Post() As Post
        Get
            If Me.PostID Is Nothing Then Return Nothing
            Return (From e As Post In DB.Posts
                    Where e.ID = Me.PostID
                    Select e).First
        End Get
    End Property

    Public ReadOnly Property ChatParty1User() As User
        Get
            If Me.ChatParty1UserID Is Nothing Then Return Nothing
            Return (From e As User In DB.Users
                    Where e.ID = Me.ChatParty1UserID
                    Select e).First
        End Get
    End Property

    Public ReadOnly Property ChatParty2User() As User
        Get
            If Me.ChatParty2UserID Is Nothing Then Return Nothing
            Return (From e As User In DB.Users
                    Where e.ID = Me.ChatParty2UserID
                    Select e).First
        End Get
    End Property
End Class