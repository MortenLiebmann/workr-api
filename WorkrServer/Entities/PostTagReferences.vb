Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Newtonsoft.Json

<Table("posttagreferences")>
Public Class PostTagReferences
    Inherits Entity

    <Key>
    Public Overrides Property ID As Guid?
    Public Property PostTagID As Guid?
    Public Property PostID As Guid?

    Public ReadOnly Property PostTag() As PostTag
        Get
            If Me.PostTagID Is Nothing Then Return Nothing
            Return (From e As PostTag In DB.PostTags.AsNoTracking
                    Where e.ID = Me.PostTagID
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
