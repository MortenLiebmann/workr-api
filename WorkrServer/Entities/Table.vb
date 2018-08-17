Imports System.Data.Entity
Imports System.Reflection
Imports Newtonsoft.Json
Imports WorkrServer

Public Class Table(Of T As Entity)
    Public ReadOnly Property DbSet As DbSet(Of T)

    Public Sub New(ByRef dbset As DbSet(Of T))
        Me.DbSet = dbset
    End Sub

    Public ReadOnly Property Properties As PropertyInfo()
        Get
            Return GetType(T).GetProperties()
        End Get
    End Property

    Public Overridable Function GetAll() As T()
        Return (From e As T In DbSet
                Select e).ToArray
    End Function

    Public Overridable Function Put(e As T) As T
        e.ID = Guid.NewGuid
        Dim result As T = DbSet.Add(e)
        DB.SaveChanges()
        Return result
    End Function

    Public Overridable Function Put(json As String) As T
        Dim jsonEntity As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
        jsonEntity.ID = Guid.NewGuid
        Dim result As T = DbSet.Add(jsonEntity)
        DB.SaveChanges()
        Return result
    End Function

    Public Overridable Sub Delete(e As T)
        DbSet.Remove(e)
        DB.SaveChanges()
    End Sub

    Public Overridable Sub Delete(id As String)
        DbSet.Remove((From e As T In DbSet
                      Where e.ID.ToString = id
                      Select e).First)
        DB.SaveChanges()
    End Sub

    Public Overridable Sub Delete(id As Guid)
        DbSet.Remove(From e As T In DbSet
                     Where e.ID = id
                     Select e)
        DB.SaveChanges()
    End Sub

    Public Overridable Function Search(json As String) As T()
        Dim jsonEntity As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)

        Dim selector As Func(Of T, Boolean) = Function(e)
                                                  For Each prop As PropertyInfo In Properties
                                                      If prop.GetValue(jsonEntity) IsNot Nothing Then
                                                          If Not CompareEntitys(jsonEntity, e, prop) Then Return False
                                                      End If
                                                  Next
                                                  Return True
                                              End Function
        Dim result As T() = DbSet.Where(selector).ToArray
        Return result
    End Function

    Public Overridable Function Patch(json As String) As T
        Dim jsonEntity As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)

    End Function

    Private Function CompareEntitys(jsonEntity As T, dbEntity As T, prop As PropertyInfo) As Boolean
        If prop.Name.EndsWith("ID") AndAlso (jsonEntity.ID = Guid.Empty) Then Return True
        If prop.GetValue(jsonEntity) Is Nothing Then Return True
        Return prop.GetValue(jsonEntity) = prop.GetValue(dbEntity)
    End Function
End Class
