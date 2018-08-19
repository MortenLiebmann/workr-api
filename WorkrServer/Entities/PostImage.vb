Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("postimages")>
Public Class PostImage
    Inherits Entity
    <Key>
    Public Overrides Property ID As Guid?
    Public Property PostID As Guid?
    Public Property Path As String
    Public Property Description As String

    Public Overrides Function Expand() As Object
        Throw New NotImplementedException()
    End Function
End Class