Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Reflection
Imports Newtonsoft.Json
Imports WorkrServer

''' <summary>
''' The base class that all entitys inherit from.
''' </summary>
Public MustInherit Class Entity
    Public MustOverride Property ID As Guid?

    ''' <summary>
    ''' The tablename that the entity is assosiated with.
    ''' </summary>
    ''' <returns></returns>
    <NotMapped>
    <JsonIgnore>
    Public MustOverride ReadOnly Property TableName As String

    <NotMapped>
    <JsonIgnore>
    Public MustOverride ReadOnly Property FileUploadAllowed As Boolean

    ''' <summary>
    ''' The HTPP method that an entity operation might use.
    ''' </summary>
    ''' <returns></returns>
    <NotMapped>
    Public Property HttpMethod As String = ""

    ''' <summary>
    ''' A subroutine that entitys must override and can be used for entity specific code that should be executed during a Put operation
    ''' </summary>
    ''' <param name="params">Can be used for entity specific parameters</param>
    Public MustOverride Sub OnPut(Optional params As Object = Nothing)
    ''' <summary>
    ''' A subroutine that entitys must override and can be used for entity specific code that should be executed during a Patch operation
    ''' </summary>
    ''' <param name="params">Can be used for entity specific parameters</param>
    Public MustOverride Function OnPatch(Optional params As Object = Nothing) As Boolean
    ''' <summary>
    ''' A subroutine that entitys must override and can be used for entity specific code that should be executed during a file upload
    ''' </summary>
    ''' <param name="params">Can be used for entity specific parameters</param>
    Public MustOverride Function OnFileUpload(Optional params As Object = Nothing) As Object
    ''' <summary>
    ''' A subroutine that entitys must override and is used in to create a file associated with a different entity
    ''' </summary>
    ''' <param name="params">Can be used for entity specific parameters</param>
    Public MustOverride Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object

    ''' <summary>
    ''' Tells Newtonsoft.JSON to not serialise the HttpMethod property
    ''' </summary>
    ''' <returns></returns>
    Public Function ShouldSerializeHttpMethod() As Boolean
        Return False
    End Function

    Public Shared Operator =(e1 As Entity, e2 As Entity) As Boolean
        Return e1.ID = e2.ID
    End Operator

    Public Shared Operator <>(e1 As Entity, e2 As Entity) As Boolean
        Return Not e1.ID = e2.ID
    End Operator

    'Enity exceptions
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
