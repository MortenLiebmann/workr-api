Imports System.Reflection
Imports Newtonsoft.Json
Imports Npgsql

Public MustInherit Class DbToItem(Of T As Item)
    Private m_dbConnection As NpgsqlConnection
    Private m_reader As NpgsqlDataReader
    Private m_cmd As NpgsqlCommand
    Private m_Items As New Dictionary(Of Guid, T)
    Private m_Path As String

    MustOverride Overloads Function Equals(x As T, y As T) As Boolean
    ''' <summary>
    ''' uses Reader to convert postgreSQL query results into items
    ''' </summary>
    ''' <returns></returns>
    MustOverride Function ReaderToItems() As Dictionary(Of Guid, T)
    ''' <summary>
    ''' Gets all rows in DB.
    ''' </summary>
    MustOverride Sub DBGet()
    ''' <summary>
    ''' Adds a row to DB.
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    MustOverride Function DBAdd(ByVal obj As T) As T
    ''' <summary>
    ''' Removes a row from DB
    ''' </summary>
    ''' <param name="obj"></param>
    MustOverride Sub DBRemove(ByVal obj As T)
    ''' <summary>
    ''' Modifies row in DB.
    ''' </summary>
    ''' <param name="obj"></param>
    MustOverride Sub DBModify(ByVal obj As T)

    Public Sub New()
    End Sub

    Public Sub New(ByRef dbCon As NpgsqlConnection, ByVal path As String)
        DbConnection = dbCon
        Me.Path = path
    End Sub

    Public ReadOnly Property ItemType() As Type
        Get
            Return GetType(T)
        End Get
    End Property

    ''' <summary>
    ''' Main items collection.
    ''' </summary>
    ''' <returns></returns>
    Friend Property Items As Dictionary(Of Guid, T)
        Get
            Return m_Items
        End Get
        Set(value As Dictionary(Of Guid, T))
            m_Items = value
        End Set
    End Property

    ''' <summary>
    ''' http URL path.
    ''' </summary>
    ''' <returns></returns>
    Friend Property Path As String
        Get
            Return m_Path
        End Get
        Set(value As String)
            m_Path = value
        End Set
    End Property

    Friend Property Reader As NpgsqlDataReader
        Get
            Return m_reader
        End Get
        Set(value As NpgsqlDataReader)
            m_reader = value
        End Set
    End Property

    Friend Property Cmd As NpgsqlCommand
        Get
            Return m_cmd
        End Get
        Set(value As NpgsqlCommand)
            m_cmd = value
        End Set
    End Property

    ''' <summary>
    ''' List of Properties in T
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly Property Properties As PropertyInfo()
        Get
            Return GetType(T).GetProperties()
        End Get
    End Property

    ''' <summary>
    ''' DB connection.
    ''' </summary>
    ''' <returns></returns>
    Public Property DbConnection As NpgsqlConnection
        Get
            Return m_dbConnection
        End Get
        Set(value As NpgsqlConnection)
            m_dbConnection = value
            If Not m_dbConnection.FullState = ConnectionState.Open Then
                m_dbConnection.Open()
            End If
            Cmd = New NpgsqlCommand("", m_dbConnection)
            DBGet()
        End Set
    End Property

    ''' <summary>
    ''' Finds item by ID.
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Public Overloads Function FindByID(id As String) As T
        Try
            Return Items(Guid.Parse(id))
        Catch ex As Exception
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Finds item by ID.
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Public Overloads Function FindByID(id As Guid) As T
        Try
            Return Items(id)
        Catch ex As Exception
        End Try
        Return Nothing
    End Function

    Public Function GetAll() As T()
        Return Items.Values.ToArray
    End Function

    Public Overloads Function Put(ByVal obj As T) As T
        Items.Add(obj.ID, obj)
        Return DBAdd(obj)
    End Function

    Public Overloads Function Put(ByVal json As String) As T
        Dim obj As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
        If obj.ID = Guid.Empty Then obj.ID = Guid.NewGuid
        Items.Add(obj.ID, obj)
        Return DBAdd(obj)
    End Function

    Public Sub Delete(ByVal obj As T)
        Items.Remove(obj.ID)
        DBRemove(obj)
    End Sub

    Public Sub Delete(ByVal id As String)
        Dim obj As T = FindByID(id)
        Items.Remove(obj.ID)
        DBRemove(obj)
    End Sub

    Public Overloads Function Search(ByVal json As String) As T()
        Dim objectToMatch As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
        Dim result As New List(Of T)
        result.AddRange(Items.Values.ToArray)

        For Each prop As PropertyInfo In Properties
            If prop.PropertyType.IsArray Then
                If prop.GetValue(objectToMatch).length > 0 Then
                    result.RemoveAll(Function(e) Not IsSubsetOf(prop.GetValue(objectToMatch), prop.GetValue(e)))
                End If
            ElseIf Not prop.GetValue(objectToMatch) = Nothing Then
                result.RemoveAll(Function(e) Not (prop.GetValue(e) = prop.GetValue(objectToMatch)))
            End If
        Next

        Return result.ToArray
    End Function

    Public Overloads Sub Modify(ByVal obj As T)
        Items(obj.ID) = obj
        DBModify(obj)
    End Sub

    Public Overloads Sub Modify(ByVal json As String)
        Dim obj As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
        Items(obj.ID) = obj
        DBModify(obj)
    End Sub

End Class
