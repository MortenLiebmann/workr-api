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

    <NotMapped>
    Public Property HttpMethod As String = ""

    Public MustOverride Sub OnPut(Optional params As Object = Nothing)
    Public MustOverride Function OnPatch(Optional params As Object = Nothing) As Boolean
    Public MustOverride Function OnFileUpload(Optional params As Object = Nothing) As Object
    Public MustOverride Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object

    Public Function ShouldSerializeHttpMethod() As Boolean
        Return False
    End Function

    Public Shared Operator =(e1 As Entity, e2 As Entity) As Boolean
        Return e1.ID = e2.ID
    End Operator

    Public Shared Operator <>(e1 As Entity, e2 As Entity) As Boolean
        Return Not e1.ID = e2.ID
    End Operator

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

        Private m_ID As String = ""
        Private m_TableName

        Public Sub New(id As String, tableName As String)
            m_ID = id
            m_TableName = tableName
        End Sub

        Public Overrides ReadOnly Property Message As String
            Get
                Return String.Format("ID '{0}' was not found in resource '{1}'.", m_ID, m_TableName)
                Return "ID was not found.2"
            End Get
        End Property
    End Class

    Public Class FileUploadNotAllowedException
        Inherits Exception

        Public Overrides ReadOnly Property Message As String
            Get
                Return "File uploading is not allowed for this resource."
            End Get
        End Property
    End Class

    Public Class FileNotFoundException
        Inherits Exception

        Dim m_Message As String = "Requested file was not found."

        Public Sub New()
        End Sub

        Public Sub New(message As String)
            m_Message = message.Replace("""", "'")
        End Sub

        Public Overrides ReadOnly Property Message As String
            Get
                Return m_Message
            End Get
        End Property
    End Class

    Public Class ForeignKeyFialationException
        Inherits Exception

        Dim m_Message As String = ""
        Public Sub New(message As String)
            m_Message = message.Replace("""", "'")
        End Sub

        Public Overrides ReadOnly Property Message As String
            Get
                Return m_Message
            End Get
        End Property
    End Class

    Public Class MalformedJsonException
        Inherits Exception

        Dim m_Message As String = ""
        Public Sub New(message As String)
            m_Message = message.Replace("""", "'")
        End Sub

        Public Overrides ReadOnly Property Message As String
            Get
                Return m_Message
            End Get
        End Property
    End Class
End Class
