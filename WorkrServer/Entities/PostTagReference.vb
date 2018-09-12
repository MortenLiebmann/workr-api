Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

''' <summary>
''' The PostTagReference entity class
''' This class is mapped to the "posttagreferences" table in the database
''' Contains the database table "posttagreferences" fields as propterties, marked with the attribute "Key"
''' Properties marked with the attribute "NotMapped" are mapped to a field in this entitys assosiated database table
''' Properties marked with the attribute "JsonIgnore" are not serialized or deserialized
''' </summary>
<Table("posttagreferences")>
Public Class PostTagReference
    Inherits Entity

    'These are the table columns
    <Key>
    Public Overrides Property ID As Guid?
    Public Property PostTagID As Guid?
    Public Property PostID As Guid?

    ''' <summary>
    ''' Fetches the full PostTag entity using the "PostTagID" property
    ''' </summary>
    ''' <returns>A PostTag Entity</returns>
    <NotMapped>
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

    ''' <summary>
    ''' PostTagReference specific code for Put operations
    ''' </summary>
    ''' <param name="params"></param>
    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
    End Sub

    Public Overrides Function OnPatch(Optional params As Object = Nothing) As Boolean
        Return True
    End Function

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function
End Class
