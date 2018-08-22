Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("tagreferences")>
Public Class TagReference
    Inherits Entity

    <Key>
    Public Overrides Property ID As Guid?
    Public Property TagID As Guid?
    Public Property PostID As Guid?

    Public ReadOnly Property Tag() As Tag
        Get
            If Me.TagID Is Nothing Then Return Nothing
            Return (From e As Tag In DB.Tags
                    Where e.ID = Me.TagID
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

    Public Overrides ReadOnly Property FileUploadAllowed As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "tagreferences"
        End Get
    End Property

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function
End Class
