Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity

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

    'Public Overrides Function Expand() As Object
    '    Return New With {.Rating = Me, User(), RatedByUser(), Post()
    '    }
    'End Function

    Public ReadOnly Property User() As User
        Get
            If Me.UserID Is Nothing Then Return Nothing
            Return (From e As User In DB.Users
                    Where e.ID = Me.UserID
                    Select e).First
        End Get
    End Property

    Public ReadOnly Property RatedByUser() As User
        Get
            If Me.RatedByUserID Is Nothing Then Return Nothing
            Return (From e As User In DB.Users
                    Where e.ID = Me.RatedByUserID
                    Select e).First
        End Get
    End Property

    Public ReadOnly Property Post() As Post
        Get
            If Me.PostID Is Nothing Then Return Nothing
            Return (From e As Post In DB.Posts
                    Where e.ID = Me.PostID
                    Select e).First
        End Get
    End Property
End Class