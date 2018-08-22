Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity
Imports WorkrServer

<Table("users")>
Public Class User
    Inherits Entity

    <Key>
    Public Overrides Property ID As Guid?
    Public Property Name As String
    Public Property Email As String
    Public Property PasswordHash As String
    Public Property Salt As String
    Public Property Address As String
    Public Property Business As String
    Public Property Phone As String
    Public Property Company As String
    Public Property AccountFlags As Int64?

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