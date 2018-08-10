Imports System.Reflection
Imports Hotel
Imports Newtonsoft.Json

Public Class Guest
    Inherits Item

    Private m_Gender As Integer
    Private m_FirstName As String
    Private m_LastName As String
    Private m_FullName As String
    Private m_Company As String
    Private m_Address As String
    Private m_ZIP As String
    Private m_City As String
    Private m_Country As String
    Private m_RoomID As Guid
    Private m_TempRoom As Room = New Room
    Private m_IsCheckedIn As Boolean

    <JsonConstructor>
    Public Sub New(iD As Guid, gender As Integer, firstName As String, lastName As String, fullName As String, company As String, address As String, zIP As String, city As String, country As String, room As Room, isCheckedIn As Boolean)
        Me.ID = iD
        Me.Gender = gender
        Me.FirstName = firstName
        Me.LastName = lastName
        Me.FullName = fullName
        Me.Company = company
        Me.Address = address
        Me.ZIP = zIP
        Me.City = city
        Me.Country = country
        Me.Room = room
        Me.IsCheckedIn = isCheckedIn
    End Sub

    Public Sub New(iD As Guid, gender As Integer, firstName As String, lastName As String, fullName As String, company As String, address As String, zIP As String, city As String, country As String, roomID As Guid, isCheckedIn As Boolean)
        Me.ID = iD
        Me.Gender = gender
        Me.FirstName = firstName
        Me.LastName = lastName
        Me.FullName = fullName
        Me.Company = company
        Me.Address = address
        Me.ZIP = zIP
        Me.City = city
        Me.Country = country
        Me.m_RoomID = roomID
        Me.IsCheckedIn = isCheckedIn
    End Sub

    Public Sub New()
        MyBase.New()
    End Sub

    Public Property Gender As Integer
        Get
            Return m_Gender
        End Get
        Set(value As Integer)
            m_Gender = value
        End Set
    End Property

    Public Property FirstName As String
        Get
            Return m_FirstName
        End Get
        Set(value As String)
            m_FirstName = value
        End Set
    End Property

    Public Property LastName As String
        Get
            Return m_LastName
        End Get
        Set(value As String)
            m_LastName = value
        End Set
    End Property

    Public Property FullName As String
        Get
            Return m_FullName
        End Get
        Set(value As String)
            m_FullName = value
        End Set
    End Property

    Public Property Company As String
        Get
            Return m_Company
        End Get
        Set(value As String)
            m_Company = value
        End Set
    End Property

    Public Property Address As String
        Get
            Return m_Address
        End Get
        Set(value As String)
            m_Address = value
        End Set
    End Property

    Public Property ZIP As String
        Get
            Return m_ZIP
        End Get
        Set(value As String)
            m_ZIP = value
        End Set
    End Property

    Public Property City As String
        Get
            Return m_City
        End Get
        Set(value As String)
            m_City = value
        End Set
    End Property

    Public Property Country As String
        Get
            Return m_Country
        End Get
        Set(value As String)
            m_Country = value
        End Set
    End Property

    Public Property Room As Room
        Get
            Dim r = Map("room").FindByID(m_RoomID.ToString)
            If Not r = Nothing Then Return r
            Return New Room()
        End Get
        Set(value As Room)
            If value IsNot Nothing Then
                m_RoomID = value.ID
                m_TempRoom = value
            End If
        End Set
    End Property

    Public Property IsCheckedIn As Boolean
        Get
            Return m_IsCheckedIn
        End Get
        Set(value As Boolean)
            m_IsCheckedIn = value
        End Set
    End Property

    Public Overrides Function AsSqlString(ByVal withID As Boolean) As String
        Return IIf(withID, "'" & ID.ToString & "',", "") &
            Gender & "," &
            "'" & FirstName & "'," &
            "'" & LastName & "'," &
            "'" & FullName & "'," &
            "'" & Company & "'," &
            "'" & Address & "'," &
            "'" & ZIP & "'," &
            "'" & City & "'," &
            "'" & Country & "'," &
            "'" & m_RoomID.ToString & "'," &
            IsCheckedIn
    End Function

End Class
