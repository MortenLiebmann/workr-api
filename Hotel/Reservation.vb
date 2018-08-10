Imports System.Reflection
Imports Hotel
Imports Newtonsoft.Json

Public Class Reservation
    Inherits Item

    Private m_StartTime As DateTime
    Private m_EndTime As DateTime
    Private m_RoomID As Guid
    Private m_TempRoom As Room


    Public Sub New(iD As Guid, startTime As Date, endTime As Date, roomID As Guid)
        Me.ID = iD
        Me.StartTime = startTime
        Me.EndTime = endTime
        Me.m_RoomID = roomID
    End Sub

    <JsonConstructor>
    Public Sub New(iD As Guid, startTime As Date, endTime As Date, room As Room)
        Me.ID = iD
        Me.StartTime = startTime
        Me.EndTime = endTime
        Me.Room = room
    End Sub

    Public Sub New()
        MyBase.New()
    End Sub

    Public Property StartTime As Date
        Get
            Return m_StartTime
        End Get
        Set(value As Date)
            m_StartTime = value
        End Set
    End Property

    Public Property EndTime As Date
        Get
            Return m_EndTime
        End Get
        Set(value As Date)
            m_EndTime = value
        End Set
    End Property

    Public Property Room As Room
        Get
            Dim r = Map("room").FindByID(m_RoomID.ToString)
            If Not r = Nothing Then Return r
            Return m_TempRoom
        End Get
        Set(value As Room)
            If Not value = Nothing Then
                m_RoomID = value.ID
                m_TempRoom = value
            Else
                m_TempRoom = New Room
            End If
        End Set
    End Property

    Public Overrides Function AsSqlString(ByVal withID As Boolean) As String
        Return IIf(withID, "'" & ID.ToString & "',", "") &
            "'" & StartTime & "'," &
            "'" & EndTime & "'," &
            "'" & m_RoomID.ToString & "'"
    End Function

End Class
