Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Reflection
Imports WorkrServer

Public MustInherit Class Entity

    Public Sub New()
        'Me.ID = Guid.NewGuid
    End Sub
    Public MustOverride Property ID As Guid?
    <NotMapped>
    Public MustOverride ReadOnly Property TableName As String
    <NotMapped>
    Public MustOverride ReadOnly Property FileUploadAllowed As Boolean
    Public MustOverride Function OnFileUpload(Optional params As Object = Nothing) As Object
    Public MustOverride Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object



End Class
