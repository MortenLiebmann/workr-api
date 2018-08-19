Imports System.Data.Entity
Imports System.Reflection
Imports Newtonsoft.Json
Imports WorkrServer

Public Class Table(Of T As Entity)
    Public ReadOnly Property DbSet As DbSet(Of T)

    Public Sub New(ByRef dbset As DbSet(Of T))
        Me.DbSet = dbset
    End Sub

    Private ReadOnly Property Properties As PropertyInfo()
        Get
            Return GetType(T).GetProperties()
        End Get
    End Property

    Public Overloads Function GetAll() As T()
        Return (From e As T In DbSet.AsNoTracking
                Select e).ToArray
    End Function

    Public Overloads Function GetAll(expand As Boolean) As Object()
        If expand Then Return GetAllExpand()
        Return GetAll()
    End Function

    Public Overloads Function GetByID(id As String, expand As Boolean) As Object
        If expand Then Return GetByIDExpand(id)
        Return GetByID(id)
    End Function

    Public Overloads Function GetByID(id As String) As T
        Dim userID As Guid = Guid.Parse(id)
        Return (From e As T In DbSet.AsNoTracking
                Where e.ID = userID
                Select e).First
    End Function

    Public Overloads Function GetAllExpand() As Object()
        Dim result As T() = (From e As T In DbSet.AsNoTracking
                             Select e).ToArray

        Dim expandedResult As New List(Of Object)
        For Each u As T In result
            expandedResult.Add(u.Expand)
        Next

        Return expandedResult.ToArray
    End Function

    Public Overloads Function GetByIDExpand(id As String) As Object
        Dim userID As Guid = Guid.Parse(id)
        Dim result As T = (From e As T In DbSet.AsNoTracking
                           Where e.ID = userID
                           Select e).First
        Return result.Expand
    End Function

    Public Overloads Function Put(e As T) As T
        e.ID = Guid.NewGuid
        Dim result As T = DbSet.Add(e)
        DB.SaveChanges()
        Return result
    End Function

    Public Overloads Function Put(json As String) As T
        Dim jsonEntity As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
        jsonEntity.ID = Guid.NewGuid
        Dim result As T = DbSet.Add(jsonEntity)
        DB.SaveChanges()
        Return result
    End Function

    Public Function Delete(id As String) As Boolean
        Dim userID As Guid = Guid.Parse(id)
        DbSet.Remove((From e As T In DbSet
                      Where e.ID = userID
                      Select e).First)
        DB.SaveChanges()
        Return True
    End Function

    Public Function Search(json As String) As T()
        Dim jsonEntity As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)

        Dim selector As Func(Of T, Boolean) = Function(e)
                                                  For Each prop As PropertyInfo In Properties
                                                      If Not CompareEntitys(jsonEntity, e, prop) Then Return False
                                                  Next
                                                  Return True
                                              End Function
        Dim result As T() = DbSet.Where(selector).ToArray
        Return result
    End Function

    Public Function Patch(id As String, json As String) As T
        Dim jsonEntity As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
        Dim userID As Guid = Guid.Parse(id)
        Dim dbEntity As T = (From e As T In DbSet
                             Where e.ID = userID
                             Select e).First

        For Each prop As PropertyInfo In Properties
            If prop.GetValue(jsonEntity) Is Nothing Then Continue For
            prop.SetValue(dbEntity, prop.GetValue(jsonEntity))
        Next

        DB.SaveChanges()
        Return dbEntity
    End Function

    Private Function CompareEntitys(jsonEntity As T, dbEntity As T, prop As PropertyInfo) As Boolean
        If prop.GetValue(jsonEntity) Is Nothing Then Return True
        Return prop.GetValue(jsonEntity) = prop.GetValue(dbEntity)
    End Function
End Class