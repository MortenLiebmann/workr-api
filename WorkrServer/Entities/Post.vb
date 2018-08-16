Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("posts")>
Public Class Post
    Inherits Entity
    <Key>
    Public Property ID As Guid
    Public Property CreatedByUserID As Guid
    Public Property CreatedDate As DateTime
    Public Property Title As String
    Public Property Description As String
    Public Property Address As String
    Public Property JobEndDate As DateTime
    Public Property PostFlags As Int64
End Class