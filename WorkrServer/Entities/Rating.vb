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
    Public Property CreatedDate As DateTime
    Public Property Score As Int16
    Public Property Text As String

    Public Overrides Function Expand() As Object
        Return New With {.Rating = Me, User(), RatedByUser(), Post()
        }
    End Function

    Public Function User() As User
        Return (From e As User In DB.Users
                Where e.ID = Me.UserID
                Select e).First
    End Function

    Public Function RatedByUser() As User
        Return (From e As User In DB.Users
                Where e.ID = Me.RatedByUserID
                Select e).First
    End Function

    Public Function Post() As Post
        Return (From e As Post In DB.Posts
                Where e.ID = Me.PostID
                Select e).First
    End Function
End Class