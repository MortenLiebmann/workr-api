Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("postbids")>
Public Class PostBid
    Inherits Entity
    <Key>
    Public Overrides Property ID As Guid?
    Public Property PostID As Guid?
    Public Property CreatedByUserID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property Text As String
    Public Property Price As Decimal
    Public Property Flags As Int64?

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "postbids"
        End Get
    End Property

    Public Overrides ReadOnly Property FileUploadAllowed As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function
End Class
