Imports System.Data.Entity
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports WorkrServer
Imports WorkrServer.Entity

Public Class Resource(Of T As Entity)
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

    Public Overloads Function Put(json As String) As T
        CheckAuthentication()
        AddJsonProperty(json, "HttpMethod", "PUT")
        Dim jsonEntity As T = Nothing
        Dim dbEntity As T = Nothing
        Try
            jsonEntity = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
            jsonEntity.OnPut(jsonEntity)
            dbEntity = DbSet.Add(jsonEntity)
            DB.SaveChanges()
            Return (From e As T In DbSet.AsNoTracking
                    Where e.ID = dbEntity.ID
                    Select e).Single
        Catch ex As Infrastructure.DbUpdateException
            DB.DiscardTrackedEntity(dbEntity)
            Dim exceptionDetail As Core.UpdateException = TryCast(ex.InnerException, Core.UpdateException)
            If exceptionDetail IsNot Nothing Then
                Throw New ForeignKeyFialationException(DirectCast(exceptionDetail.InnerException, Npgsql.NpgsqlException).Detail)
            End If
            Throw ex
        Catch ex As JsonReaderException
            Throw New MalformedJsonException(ex.Message)
        Catch ex As Exception
            DB.DiscardTrackedEntity(dbEntity)
            Throw ex
        End Try
    End Function

    Public Function Delete(id As String) As Boolean
        CheckAuthentication()
        Try
            Dim userID As Guid = Guid.Parse(id)
            DbSet.Remove((From e As T In DbSet
                          Where e.ID = userID
                          Select e).First)
            DB.SaveChanges()
            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function Search(json As String) As T()
        AddJsonProperty(json, "HttpMethod", "POST")
        Dim jsonEntity As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
        Dim selector As Func(Of T, Boolean) = Function(e)
                                                  For Each prop As PropertyInfo In Properties
                                                      If Not CompareEntityProperty(jsonEntity, e, prop) Then Return False
                                                  Next
                                                  Return True
                                              End Function
        Return DbSet.ToArray().Where(selector).ToArray
        Return DbSet.Where(selector).ToArray
    End Function

    Public Function Patch(id As String, json As String) As T
        CheckAuthentication()
        Dim dbEntity As T = Nothing
        Try
            AddJsonProperty(json, "ID", id)
            AddJsonProperty(json, "HttpMethod", "PATCH")
            Dim jsonEntity As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
            Dim entityID As Guid = Guid.Parse(id)
            dbEntity = (From e As T In DbSet
                        Where e.ID = entityID
                        Select e).First

            If dbEntity.OnPatch(jsonEntity) Then
                For Each prop As PropertyInfo In Properties
                    If prop.GetValue(jsonEntity) Is Nothing Then Continue For
                    If prop.Name = "ID" Then Continue For
                    If Not prop.CanWrite Then Continue For
                    prop.SetValue(dbEntity, prop.GetValue(jsonEntity))
                Next
            End If

            DB.SaveChanges()
            Return dbEntity
        Catch ex As Exception
            DB.DiscardTrackedEntity(dbEntity)
            Throw ex
        End Try
    End Function

    Public Function PutFile(file As MemoryStream, associatedEntityID As String) As T
        CheckAuthentication()

        If Not TableEntity.FileUploadAllowed Then Throw New FileUploadNotAllowedException
        Dim dbEntity As T
        Dim fileEntity As T = TableEntity
        fileEntity = fileEntity.CreateFileAssociatedEntity(New With {.associatedEntityID = Guid.Parse(associatedEntityID)})
        dbEntity = fileEntity.OnFileUpload(fileEntity)

        Dim path As String() = {fileEntity.TableName, associatedEntityID}
        Dim fileName As String = fileEntity.ID.ToString & ".png"
        FTPUpload(path, fileName, file)

        Return dbEntity
    End Function

    Public Function GetFile(id As String) As MemoryStream
        Try
            Dim path As String = String.Join("/", {TableEntity.TableName, id})
            Return FTPDownloadFirstFile(path)
        Catch ex As Net.WebException
            Throw New IdNotFoundException(id, TableEntity.TableName)
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function GetFile(associatedEntityID As String, id As String) As MemoryStream
        Try
            Dim fileStream As New MemoryStream
            Dim path As String = String.Join("/", {TableEntity.TableName, associatedEntityID, id & ".png"})
            Return FTPDownload(path)
        Catch ex As Net.WebException
            Throw New Entity.FileNotFoundException(String.Format("File not found : '{0}/{1}/{2}'", TableEntity.TableName, associatedEntityID, id))
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub CheckAuthentication()
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
    End Sub

    Private Sub MakeUploadFolder(folderName As String)
        If Not Directory.Exists(Environment.CurrentDirectory & "\" & folderName) Then MkDir(Environment.CurrentDirectory & "\" & folderName)
    End Sub

    Private Function GetFirstFileInFolder(folderPath As String) As String
        Dim folder As New DirectoryInfo(folderPath)
        Return folder.GetFiles()(0).Name
    End Function

    Private Sub AddJsonProperty(ByRef json As String, propertyName As String, propertyValue As Object)
        Dim jo As JObject = JObject.Parse(json)
        jo.AddFirst(New JProperty(propertyName, propertyValue))
        json = jo.ToString()
    End Sub
End Class