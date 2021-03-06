﻿Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Newtonsoft.Json
Imports WorkrServer

<Table("userimages")>
Public Class UserImage
    Inherits Entity

    <Key>
    Public Overrides Property ID As Guid?
    Public Property UserID As Guid?

    <JsonIgnore>
    Public ReadOnly Property User() As User
        Get
            Try
                If Me.UserID Is Nothing Then Return Nothing
                Return (From e As User In DB.Users.AsNoTracking
                        Where e.ID = Me.UserID
                        Select e).First
            Catch ex As InvalidOperationException
                Throw New IdNotFoundException(Me.UserID.ToString, "users")
            End Try
        End Get
    End Property

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "userimages"
        End Get
    End Property

    Public Overrides ReadOnly Property FileUploadAllowed As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If Not AuthUser.ID = UserID Then Throw New NotAuthorizedException
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
        UserID = AuthUser.ID
    End Sub

    Public Overrides Function OnPatch(Optional params As Object = Nothing) As Boolean
        Return True
    End Function

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If Not AuthUser.ID = UserID Then Throw New NotAuthorizedException("Can not upload userimage for a user you are not logged in as.")
        Dim userImage As UserImage = Nothing
        Try
            If Not FileUploadAllowed Then Throw New FileUploadNotAllowedException
            userImage = DirectCast(params, UserImage)
            Dim dbPostImage As UserImage = DB.UserImages.Add(userImage)
            DB.SaveChanges()
            Return dbPostImage
        Catch ex As Data.Entity.Infrastructure.DbUpdateException
            DB.DiscardTrackedEntity(params)
            Throw New IdNotFoundException(userImage.UserID.ToString, "users")
        Catch ex As Exception
            DB.DiscardTrackedEntity(params)
            Throw New OnFileUploadException
        End Try
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If params Is Nothing Then Throw New Exception("params is nothing")
        If Me.UserID Is Nothing Then Me.UserID = params.associatedEntityID
        If Not AuthUser.ID = UserID Then Throw New NotAuthorizedException("Can not upload userimage for a user you are not logged in as.")
        Return CreateFileAssociatedUserImage(params.associatedEntityID)
    End Function

    Private Function CreateFileAssociatedUserImage(userID As Guid) As UserImage
        Return New UserImage With {
            .ID = Guid.NewGuid,
            .UserID = userID}
    End Function
End Class
