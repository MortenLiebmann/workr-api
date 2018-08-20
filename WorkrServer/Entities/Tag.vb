Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("tags")>
Public Class Tag
    Inherits Entity
    <Key>
    Public Overrides Property ID As Guid?
    Public Property Name As String
    Public Property TagFlags As Int64?

    'Public Overrides Function Expand() As Object
    '    Return Me
    'End Function
End Class
