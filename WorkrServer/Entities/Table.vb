Imports System.Data.Entity
Imports System.IO
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

    Private ReadOnly Property TableEntity As T
        Get
            Return GetType(T).GetConstructor(New Type() {}).Invoke(New Object() {})
        End Get
    End Property

    Public Overloads Function GetAll() As T()
        Return (From e As T In DbSet.AsNoTracking
                Select e).ToArray
    End Function

    Public Overloads Function GetByID(id As String) As T
        Try
            Dim userID As Guid = Guid.Parse(id)
            Return (From e As T In DbSet.AsNoTracking
                    Where e.ID = userID
                    Select e).First
        Catch ex As InvalidOperationException
            Throw New Entity.IdNotFoundException(id, TableEntity.TableName)
        End Try
    End Function
    'fix db id null
    Public Overloads Function Put(json As String) As T
        Dim jsonEntity As T = Nothing
        Try
            jsonEntity = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
            'jsonEntity.ID = Guid.NewGuid
            Dim result As T = DbSet.Add(jsonEntity)
            DB.SaveChanges()
            Return result
        Catch ex As Exception
            DB.DiscardTrackedEntityByID(jsonEntity.ID)
            Throw ex
        End Try
    End Function

    Public Function Delete(id As String) As Boolean
        Try
            Dim userID As Guid = Guid.Parse(id)
            DbSet.Remove((From e As T In DbSet
                          Where e.ID = userID
                          Select e).First)
            DB.SaveChanges()
            Return True
        Catch ex As Exception
            DB.DiscardTrackedEntityByID(Guid.Parse(id))
            Throw ex
        End Try
    End Function

    Public Overloads Function Search(jsonEntity As T) As T()
        Dim selector As Func(Of T, Boolean) = Function(e)
                                                  For Each prop As PropertyInfo In Properties
                                                      If Not CompareEntityProperty(jsonEntity, e, prop) Then Return False
                                                  Next
                                                  Return True
                                              End Function
        Return DbSet.Where(selector).ToArray
    End Function

    Public Overloads Function Search(jsonEntity As String) As T()
        Return Search(JsonConvert.DeserializeObject(Of T)(jsonEntity, JSONSettings))
    End Function

    Public Function Patch(id As String, json As String) As T
        Try
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
        Catch ex As Exception
            DB.DiscardTrackedEntityByID(Guid.Parse(id))
        Throw ex
        End Try
    End Function

    Public Function PutFile(file As MemoryStream, associatedEntity As String) As T
        Dim dbEntity As T
        Dim fileEntity As T = TableEntity
        fileEntity = fileEntity.CreateFileAssociatedEntity(New With {.associatedEntityID = Guid.Parse(associatedEntity)})
        dbEntity = fileEntity.OnFileUpload(fileEntity)

        Dim path As String = String.Format(
                "{0}\{1}\{2}\{3}.png",
                Environment.CurrentDirectory,
                fileEntity.TableName,
                associatedEntity,
                fileEntity.ID.ToString)

        MakeUploadFolder(fileEntity.TableName)
        MakeUploadFolder(fileEntity.TableName & "\" & associatedEntity)

        Dim fileSaver As New FileStream(
            path,
            FileMode.Create,
            FileAccess.Write)

        file.Position = 0
        file.CopyTo(fileSaver)
        file.Close()
        fileSaver.Close()
        Return dbEntity
    End Function

    Public Function View(associatedEntityID As String, id As String) As MemoryStream
        Dim fileStream As New MemoryStream

        Dim path As String = String.Format(
                "{0}\{1}\{2}\{3}.png",
                Environment.CurrentDirectory,
                TableEntity.TableName,
                associatedEntityID,
                id)
        Try
            Dim fileReader As New FileStream(path,
                                 FileMode.Open,
                                 FileAccess.Read)
            fileReader.CopyTo(fileStream)
            Return fileStream
        Catch ex As IO.FileNotFoundException
            Throw New Entity.FileNotFoundException
        Catch ex As DirectoryNotFoundException
            Throw New Entity.FileNotFoundException
        End Try
    End Function

    Private Sub MakeUploadFolder(folderName As String)
        If Not Directory.Exists(Environment.CurrentDirectory & "\" & folderName) Then MkDir(Environment.CurrentDirectory & "\" & folderName)
    End Sub

    Private Function CompareEntityProperty(jsonEntity As T, dbEntity As T, prop As PropertyInfo) As Boolean
        If prop.GetValue(jsonEntity) Is Nothing Then Return True
        If prop.PropertyType Is GetType(DateTime) Then
            Dim t1 As UInt64 = Math.Round(CDate(prop.GetValue(jsonEntity)).Ticks / TimeSpan.TicksPerSecond, 0) * TimeSpan.TicksPerSecond
            Dim t2 As UInt64 = Math.Round(CDate(prop.GetValue(dbEntity)).Ticks / TimeSpan.TicksPerSecond, 0) * TimeSpan.TicksPerSecond
            Return t1 = t2
        End If
        Return prop.GetValue(jsonEntity) = prop.GetValue(dbEntity)
    End Function
End Class
