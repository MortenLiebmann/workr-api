Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("chats")>
Public Class Chat
    <Key>
    Public Property ID As Guid
    Public Property PostID As Guid
    Public Property CreatedDate As DateTime
    Public Property ChatParty1UserID As Guid
    Public Property ChatParty2UserID As Guid
End Class