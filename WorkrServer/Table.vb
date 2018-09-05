﻿Imports System.Data.Entity
Imports System.IO
Imports System.Reflection
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports WorkrServer
Imports WorkrServer.Entity

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

    Public Overloads Function Put(json As String) As T
        CheckPermissions()
        AddJsonProperty(json, "HttpMethod", "PUT")
        Dim jsonEntity As T = Nothing
        Dim dbEntity As T = Nothing
        Try
            jsonEntity = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
            jsonEntity.OnPut()
            dbEntity = DbSet.Add(jsonEntity)
            DB.SaveChanges()
            Return (From e As T In DbSet.AsNoTracking
                    Where e.ID = dbEntity.ID
                    Select e).Single
        Catch ex As Infrastructure.DbUpdateException
            DB.DiscardTrackedEntityByID(dbEntity)
            Dim exceptionDetail As Core.UpdateException = TryCast(ex.InnerException, Core.UpdateException)
            If exceptionDetail IsNot Nothing Then
                Throw New ForeignKeyFialationException(DirectCast(exceptionDetail.InnerException, Npgsql.NpgsqlException).Detail)
            End If
            Throw ex
        Catch ex As JsonReaderException
            Throw New MalformedJsonException(ex.Message)
        Catch ex As Exception
            DB.DiscardTrackedEntityByID(dbEntity)
            Throw ex
        End Try
    End Function

    Public Function Delete(id As String) As Boolean
        CheckPermissions()
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
        CheckPermissions()
        Dim dbEntity As T = Nothing
        Try
            AddJsonProperty(json, "ID", id)
            AddJsonProperty(json, "HttpMethod", "PATCH")
            Dim jsonEntity As T = JsonConvert.DeserializeObject(Of T)(json, JSONSettings)
            Dim entityID As Guid = Guid.Parse(id)
            dbEntity = (From e As T In DbSet
                        Where e.ID = entityID
                        Select e).First

            For Each prop As PropertyInfo In Properties
                If prop.GetValue(jsonEntity) Is Nothing Then Continue For
                If prop.Name = "ID" Then Continue For
                If Not prop.CanWrite Then Continue For
                prop.SetValue(dbEntity, prop.GetValue(jsonEntity))
            Next

            DB.SaveChanges()
            Return dbEntity
        Catch ex As Exception
            DB.DiscardTrackedEntityByID(dbEntity)
            Throw ex
        End Try
    End Function

    Public Function PutFile(file As MemoryStream, associatedEntity As String) As T
        CheckPermissions()

        If Not TableEntity.FileUploadAllowed Then Throw New FileUploadNotAllowedException
        Dim dbEntity As T
        Dim fileEntity As T = TableEntity
        fileEntity = fileEntity.CreateFileAssociatedEntity(New With {.associatedEntityID = Guid.Parse(associatedEntity)})
        dbEntity = fileEntity.OnFileUpload(fileEntity)

        Dim path As String() = {fileEntity.TableName, associatedEntity}
        Dim fileName As String = fileEntity.ID.ToString & ".png"
        FTPUpload(path, fileName, file)
        'Dim path As String = String.Format(
        '        "{0}\{1}\{2}\{3}.png",
        '        Environment.CurrentDirectory,
        '        fileEntity.TableName,
        '        associatedEntity,
        '        fileEntity.ID.ToString)

        'MakeUploadFolder(fileEntity.TableName)
        'MakeUploadFolder(fileEntity.TableName & "\" & associatedEntity)

        'Dim fileSaver As New FileStream(
        '    path,
        '    FileMode.Create,
        '    FileAccess.Write)

        'file.Position = 0
        'file.CopyTo(fileSaver)
        'file.Close()
        'fileSaver.Close()
        Return dbEntity
    End Function

    Public Function GetFile(id As String) As MemoryStream
        Dim path As String = String.Join("/", {TableEntity.TableName, id})
        Return FTPDownloadFirstFile(path)


        'Dim fileStream As New MemoryStream


        'Dim path As String = String.Format(
        '        "{0}\{1}\{2}\",
        '        Environment.CurrentDirectory,
        '        TableEntity.TableName,
        '        id)
        'path += GetFirstFileInFolder(path)
        'Try
        '    Dim fileReader As New FileStream(path,
        '                         FileMode.Open,
        '                         FileAccess.Read)
        '    fileReader.CopyTo(fileStream)
        '    Return fileStream
        'Catch ex As IO.FileNotFoundException
        '    Throw New Entity.FileNotFoundException
        'Catch ex As DirectoryNotFoundException
        '    Throw New Entity.FileNotFoundException
        'End Try
    End Function

    Public Function GetFile(associatedEntityID As String, id As String) As MemoryStream
        Dim fileStream As New MemoryStream
        Dim path As String = String.Join("/", {TableEntity.TableName, associatedEntityID, id & ".png"})
        'Dim path As String() = {TableEntity.TableName, associatedEntityID}
        'Dim filename As String = id & ".png"
        'Dim path As String = String.Format(
        '        "{0}\{1}\{2}\{3}.png",
        '        Environment.CurrentDirectory,
        '        TableEntity.TableName,
        '        associatedEntityID,
        '        id)
        'Try
        '    Dim fileReader As New FileStream(path,
        '                         FileMode.Open,
        '                         FileAccess.Read)
        '    fileReader.CopyTo(fileStream)
        '    Return fileStream
        'Catch ex As IO.FileNotFoundException
        '    Throw New Entity.FileNotFoundException
        'Catch ex As DirectoryNotFoundException
        '    Throw New Entity.FileNotFoundException
        'End Try
        Return FTPDownload(path)
    End Function

    Private Sub CheckPermissions()
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
