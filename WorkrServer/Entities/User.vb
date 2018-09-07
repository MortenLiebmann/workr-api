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
    Public Property PasswordHash As String
    <JsonIgnore>
    Public Property Salt As String
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

    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
    End Sub

    Public Shared Shadows Operator =(user1 As User, user2 As User)
        If user1.ID = user2.ID Then Return True
        If CStr(user1.Email).ToLower = CStr(user2.Email).ToLower Then Return True
        If CStr(user1.Name).ToLower = CStr(user2.Name).ToLower Then Return True
        Return False
    End Operator

    Public Shared Shadows Operator <>(user1 As User, user2 As User)
        Return Not (user1 = user2)
    End Operator

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function OnPatch(Optional params As Object = Nothing) As Boolean
        Return True
    End Function
End Class