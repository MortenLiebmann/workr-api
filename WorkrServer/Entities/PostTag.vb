Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

''' <summary>
''' The PostTag entity class
''' This class is mapped to the "posttags" table in the database
''' Contains the database table "posttags" fields as propterties, marked with the attribute "Key"
''' Properties marked with the attribute "NotMapped" are mapped to a field in this entitys assosiated database table
''' Properties marked with the attribute "JsonIgnore" are not serialized or deserialized
''' </summary>
<Table("posttags")>
Public Class PostTag
    Inherits Entity

    'These are the table columns
    <Key>
    Public Overrides Property ID As Guid?
    Public Property Name As String
    Public Property CreatedDate As DateTime?
    Public Property Flags As Int64?

    Public Overrides ReadOnly Property FileUploadAllowed As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "posttags"
        End Get
    End Property

    ''' <summary>
    ''' PostTag specific code for Put operations
    ''' </summary>
    ''' <param name="params"></param>
    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
    End Sub

    ''' <summary>
    ''' PostTag specific code for Patch operations
    ''' </summary>
    ''' <param name="params"></param>
    Public Overrides Function OnPatch(Optional params As Object = Nothing) As Boolean
        Return True
    End Function

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    ''' <summary>
    ''' Shadows the default = operator and changes it to use "Name" instead of "ID"
    ''' </summary>
    ''' <param name="tag1"></param>
    ''' <param name="tag2"></param>
    ''' <returns></returns>
    Public Shared Shadows Operator =(tag1 As PostTag, tag2 As PostTag) As Boolean
        Return tag1.Name = tag2.Name
    End Operator

    ''' <summary>
    ''' Shadows the default <> operator and changes it to use "Name" instead of "ID"
    ''' </summary>
    ''' <param name="tag1"></param>
    ''' <param name="tag2"></param>
    ''' <returns></returns>
    Public Shared Shadows Operator <>(tag1 As PostTag, tag2 As PostTag) As Boolean
        Return tag1.Name <> tag2.Name
    End Operator
End Class
