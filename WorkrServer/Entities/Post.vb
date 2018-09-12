Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

''' <summary>
''' The Post entity class
''' This class is mapped to the "posts" table in the database
''' Contains the database table "posts" fields as propterties, marked with the attribute "Key"
''' Properties marked with the attribute "NotMapped" are mapped to a field in this entitys assosiated database table
''' Properties marked with the attribute "JsonIgnore" are not serialized or deserialized
''' </summary>
<Table("posts")>
Public Class Post
    Inherits Entity

    'These are the table columns
    <Key>
    Public Overrides Property ID As Guid?
    Public Property CreatedByUserID As Guid?
    Public Property CreatedDate As DateTime?
    Public Property Title As String
    Public Property Description As String
    Public Property Address As String
    Public Property JobEndDate As DateTime?
    Public Property Flags As Int64?

    'Enumaration defining the meaning of the bit flags in the "Flags" Int64 property
    <Flags>
    Public Enum PostFlags As Int64
        OPEN = 0
        DELETED = 1
        COMPLETED = 2
    End Enum

    Private m_CreatedByUser As User
    Private m_Tags As PostTag()

    ''' <summary>
    ''' Fetches the full User entity using the "CreatedByUserID" property
    ''' </summary>
    ''' <returns>A User entity</returns>
    <NotMapped>
    Public Property CreatedByUser As User
        Get
            Try
                If m_CreatedByUser IsNot Nothing Then Return m_CreatedByUser
                If Me.CreatedByUserID Is Nothing Then Return Nothing
                Return (From e As User In DB.Users.AsNoTracking
                        Where e.ID = Me.CreatedByUserID
                        Select e).First
            Catch ex As InvalidOperationException
                Throw New IdNotFoundException(Me.CreatedByUserID.ToString, "users")
            End Try
        End Get
        Set(value As User)
            If HttpMethod = "POST" Then m_CreatedByUser = value
        End Set
    End Property

    ''' <summary>
    ''' Fetches an array of PostImageIDs that belong to this posts using its "ID" property
    ''' </summary>
    ''' <returns>An array of GUID</returns>
    <NotMapped>
    Public ReadOnly Property PostImageIDs As Guid?()
        Get
            Return (From e As PostImage In DB.PostImages.AsNoTracking
                    Where e.PostID = Me.ID
                    Select e.ID).ToArray
        End Get
    End Property

    ''' <summary>
    ''' Fetches an array of PostTag entitys that belong to this posts using its "ID" property
    ''' </summary>
    ''' <returns>An array </returns>
    <NotMapped>
    Public Property PostTags As PostTag()
        Get
            If m_Tags IsNot Nothing AndAlso m_Tags.Count > 0 Then Return m_Tags
            Return (From t As PostTag In DB.PostTags.AsNoTracking
                    Join tr As PostTagReference In DB.PostTagReferences.AsNoTracking
                    On tr.PostTagID Equals t.ID
                    Where tr.PostID = Me.ID
                    Select t).ToArray
        End Get
        Set(value As PostTag())
            Select Case HttpMethod
                Case "PUT", "PATCH"
                    'Automatically creates PostTag entitys on "PUT" and "PATCH" operations
                    CreatePostTagEntitys(value)
                    Exit Property
                Case Else
                    m_Tags = value
                    Exit Property
            End Select
        End Set
    End Property

    ''' <summary>
    ''' Fetches an array of PostBid entitys that belong to this posts using its "ID" property
    ''' </summary>
    ''' <returns></returns>
    <NotMapped>
    Public ReadOnly Property PostBids As PostBid()
        Get
            Try
                If Me.ID Is Nothing Then Return Nothing
                Return (From e As PostBid In DB.PostBids.AsNoTracking
                        Where e.PostID = Me.ID
                        Select e).ToArray
            Catch ex As InvalidOperationException
                Throw New IdNotFoundException(Me.CreatedByUserID.ToString, "users")
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Creates new PostTags that are given when a new Post is created with Tags attached
    ''' </summary>
    ''' <param name="postTags">The PostTags to create</param>
    Private Sub CreatePostTagEntitys(postTags As PostTag())
        If Me.ID Is Nothing OrElse Me.ID = Guid.Empty Then Me.ID = Guid.NewGuid
        Dim myPostTags As New List(Of PostTag)
        For Each tag As PostTag In postTags
            Dim existingTag As PostTag = (From e As PostTag In DB.PostTags
                                          Where e.Name = tag.Name
                                          Select e).FirstOrDefault
            If existingTag IsNot Nothing Then
                myPostTags.Add(existingTag)
                Continue For
            End If
            tag.ID = Guid.NewGuid
            Dim dbEntity As PostTag = Nothing
            Try
                dbEntity = DB.PostTags.Add(tag)
                myPostTags.Add(dbEntity)
            Catch ex As Exception
                DB.DiscardTrackedEntity(dbEntity)
            End Try
        Next
        CreatePostTagReferenceEntitys(myPostTags.ToArray)
    End Sub

    ''' <summary>
    ''' Creates PostTagReferences connection this post to its Tags
    ''' </summary>
    ''' <param name="postTags">The PostTags to connect to this Post</param>
    Private Sub CreatePostTagReferenceEntitys(postTags As PostTag())
        For Each tag As PostTag In postTags
            Dim dbEntity As PostTagReference = Nothing
            Try
                If (From e As PostTagReference In DB.PostTagReferences
                    Where e.PostID = Me.ID And
                    e.PostTagID = tag.ID
                    Select e).FirstOrDefault IsNot Nothing Then Continue For

                dbEntity = DB.PostTagReferences.Add(New PostTagReference With {
                                                    .ID = Guid.NewGuid,
                                                    .PostID = Me.ID,
                                                    .PostTagID = tag.ID})
            Catch ex As Exception
                DB.DiscardTrackedEntity(dbEntity)
            End Try
        Next
    End Sub

    Public Overrides ReadOnly Property FileUploadAllowed As Boolean = False
    Public Overrides ReadOnly Property TableName As String = "posts"

    ''' <summary>
    ''' Post specific code for Put operations
    ''' </summary>
    ''' <param name="params"></param>
    Public Overrides Sub OnPut(Optional params As Object = Nothing)
        If AuthUser Is Nothing Then Throw New NotAuthorizedException
        If ID Is Nothing OrElse ID = Guid.Empty Then ID = Guid.NewGuid
        CreatedByUserID = AuthUser.ID
    End Sub

    Public Overrides Function OnFileUpload(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateFileAssociatedEntity(Optional params As Object = Nothing) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function OnPatch(Optional params As Object = Nothing) As Boolean
        Return True
    End Function
End Class