Imports System.Data.Entity
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Newtonsoft.Json
Imports WorkrServer

''' <summary>
''' Static class helper
''' </summary>
Module Helper
    Private m_DB As New WorkrDB
    Private m_Map As New Dictionary(Of String, UserTable)
    Private m_JSONSettings As New JsonSerializerSettings() With {.MissingMemberHandling = MissingMemberHandling.Ignore,
        .DateFormatString = "yyyy-MM-ddTHH:mm:ssZ"}

    ''' <summary>
    ''' Dictionary of DbToItem objects and a string key. The key is used in the URL of HTTP requests.
    ''' </summary>
    ''' <returns></returns>
    Public Property Map As Dictionary(Of String, UserTable)
        Get
            Return m_Map
        End Get
        Set(value As Dictionary(Of String, UserTable))
            m_Map = value
        End Set
    End Property

    Public Property JSONSettings As JsonSerializerSettings
        Get
            Return m_JSONSettings
        End Get
        Set(value As JsonSerializerSettings)
            m_JSONSettings = value
        End Set
    End Property

    Public Property DB As WorkrDB
        Get
            Return m_DB
        End Get
        Set(value As WorkrDB)
            m_DB = value
        End Set
    End Property

    ''' <summary>
    ''' Returns true if the superset array contains all items in subset array.
    ''' </summary>
    ''' <param name="subset"></param>
    ''' <param name="superset"></param>
    ''' <returns></returns>
    Public Function IsSubsetOf(ByVal subset As Object(), ByVal superset As Object()) As Boolean
        Dim supersetList As List(Of Object) = superset.ToList
        Dim subsetList As List(Of Object) = subset.ToList
        For Each subItem In subsetList
            Dim found As Boolean = False
            For Each supItem In supersetList
                If IsArray(supItem) Then
                    If IsSubsetOf(subItem, supItem) Then found = True : supersetList.Remove(supItem) : Exit For
                Else
                    If subItem = supItem Then found = True : supersetList.Remove(supItem) : Exit For
                End If
            Next
            If Not found Then Return False
        Next

        Return True
    End Function
End Module
