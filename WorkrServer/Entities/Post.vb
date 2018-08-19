Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity
Imports System.Reflection

<Table("posts")>
Public Class Post
    Inherits Entity
    <Key>
    Public Overrides Property ID As Guid?
    Public Property CreatedByUserID As Guid?
    Public Property CreatedDate As DateTime
    Public Property Title As String
    Public Property Description As String
    Public Property Address As String
    Public Property JobEndDate As DateTime?
    Public Property PostFlags As Int64?

    Public Overrides Function Expand() As Object
        Return New With {.Post = Me, CreatedByUser()
        }
    End Function

    Public Function CreatedByUser() As User
        Return (From e As User In DB.Users
                Where e.ID = Me.CreatedByUserID
                Select e).First
    End Function
End Class