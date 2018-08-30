Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity
Imports Newtonsoft.Json
Imports WorkrServer

<Table("users")>
Public Class User
    Inherits Entity

    <Key>
    Public Overrides Property ID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property Name As String
    Public Property Email As String
    <JsonIgnore>
    Public Property PasswordHash As String = "336E28244541A68A5209083537BCC18CE661CE4B4B9452BA8BA543B6AB9A2BA4" 'fix
    <JsonIgnore>
    Public Property Salt As String = "6661488" ' fix
    Public Property Address As String
    Public Property Business As String
    Public Property Phone As String
    Public Property Company As String
    Public Property Flags As Int64?

    Public Overrides ReadOnly Property FileUploadAllowed As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "users"
        End Get
    End Property

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function
End Class