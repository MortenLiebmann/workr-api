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
        Throw New NotImplementedException()
    End Function
End Class