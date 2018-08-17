Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("messages")>
Public Class Message
    Inherits Entity
    <Key>
    Public Overrides Property ID As Guid
    Public Property ChatID As Guid
    Public Property SentByUserID As Guid
    Public Property CreatedDate As DateTime
    Public Property UpdatedDate As DateTime
    Public Property Text As String
    Public Property MessageFlags As Int64
End Class