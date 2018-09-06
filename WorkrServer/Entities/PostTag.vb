Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Newtonsoft.Json
Imports WorkrServer

<Table("posttags")>
Public Class PostTag
    Inherits Entity

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

    Public Shared Operator =(tag1 As PostTag, tag2 As PostTag) As Boolean
        Return tag1.Name = tag2.Name
    End Operator

    Public Shared Operator <>(tag1 As PostTag, tag2 As PostTag) As Boolean
        Return tag1.Name <> tag2.Name
    End Operator
End Class
