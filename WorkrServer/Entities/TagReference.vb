Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("tagreferences")>
Public Class TagReference
    Inherits Entity
    <Key>
    Public Overrides Property ID As Guid?
    Public Property TagID As Guid?
    Public Property PostID As Guid?

    Public Overrides Function Expand() As Object
        Throw New NotImplementedException()
    End Function
End Class
