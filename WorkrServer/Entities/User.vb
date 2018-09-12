Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Newtonsoft.Json

''' <summary>
''' The User entity class
''' This class is mapped to the "users" table in the database
''' Contains the database table "users" fields as propterties, marked with the attribute "Key"
''' Properties marked with the attribute "NotMapped" are mapped to a field in this entitys assosiated database table
''' Properties marked with the attribute "JsonIgnore" are not serialized or deserialized
''' </summary>
<Table("users")>
Public Class User
    Inherits Entity

    'These are the table columns
    <Key>
    Public Overrides Property ID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property Name As String
    Public Property Email As String
    <JsonIgnore>
    Public Property PasswordHash As String 'Marked JsonIgnore to not expose users PasswordHash when User entities are seriallised for API call returning.
    <JsonIgnore>
    Public Property Salt As String 'Marked JsonIgnore to not expose users Salt when User entities are seriallised for API call returning.
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

    ''' <summary>
    ''' Rating specific code for Put operations
    ''' </summary>
    ''' <param name="params"></param>
    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
    End Sub

    ''' <summary>
    ''' Shadows the default = operator and changes it look at more than just the ID
    ''' </summary>
    ''' <param name="user1"></param>
    ''' <param name="user2"></param>
    ''' <returns></returns>
    Public Shared Shadows Operator =(user1 As User, user2 As User)
        If user1.ID = user2.ID Then Return True
        If CStr(user1.Email).ToLower = CStr(user2.Email).ToLower Then Return True
        If CStr(user1.Name).ToLower = CStr(user2.Name).ToLower Then Return True
        Return False
    End Operator

    ''' <summary>
    ''' Shadows the default <> operator and changes it look at more than just the ID
    ''' </summary>
    ''' <param name="user1"></param>
    ''' <param name="user2"></param>
    ''' <returns></returns>
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