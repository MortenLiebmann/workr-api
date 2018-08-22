Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Reflection
Imports Newtonsoft.Json
Imports WorkrServer

Public MustInherit Class Entity
    Public MustOverride Property ID As Guid?

    <NotMapped>
    <JsonIgnore>
    Public MustOverride ReadOnly Property TableName As String

    <NotMapped>
    <JsonIgnore>
    Public MustOverride ReadOnly Property FileUploadAllowed As Boolean

    Public MustOverride Function OnFileUpload(Optional params As Object = Nothing) As Object
    Public MustOverride Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object

    Public Class OnFileUploadException
        Inherits Exception

        Public Overrides ReadOnly Property Message As String
            Get
                Return "An error occurred uploading a file."
            End Get
        End Property
    End Class

    Public Class IdNotFoundException
        Inherits Exception

        Public Overrides ReadOnly Property Message As String
            Get
                Return "ID was not found."
            End Get
        End Property
    End Class

    Public Class FileUploadNotAllowedException
        Inherits Exception

        Public Overrides ReadOnly Property Message As String
            Get
                Return "File uploading is not allowed for this entity."
            End Get
        End Property
    End Class
End Class
