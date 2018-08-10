Imports System.Reflection
Imports Hotel
Imports Newtonsoft.Json

Public Class Charge
    Inherits Item

    Private m_Type As Integer
    Private m_RoomID As Guid
    Private m_TempRoom As Room
    Private m_ChargeTime As DateTime
    Private m_Units As Integer
    Private m_PriceNet As Double
    Private m_PriceVat As Double
    Private m_PriceTotal As Double
    Private m_Description As String

    <JsonConstructor>
    Public Sub New(iD As Guid, type As Integer, room As Room, chargeTime As Date, units As Integer, priceNet As Double, priceVat As Double, priceTotal As Double, description As String)
        Me.ID = iD
        Me.Type = type
        Me.Room = room
        Me.ChargeTime = chargeTime
        Me.Units = units
        Me.PriceNet = priceNet
        Me.PriceVat = priceVat
        Me.PriceTotal = priceTotal
        Me.Description = description
    End Sub

    Public Sub New(iD As Guid, type As Integer, roomID As Guid, chargeTime As Date, units As Integer, priceNet As Double, priceVat As Double, priceTotal As Double, description As String)
        Me.ID = iD
        Me.Type = type
        Me.m_RoomID = roomID
        Me.ChargeTime = chargeTime
        Me.Units = units
        Me.PriceNet = priceNet
        Me.PriceVat = priceVat
        Me.PriceTotal = priceTotal
        Me.Description = description
    End Sub

    Public Sub New()
        MyBase.New()
    End Sub

    Public Property Type As Integer
        Get
            Return m_Type
        End Get
        Set(value As Integer)
            m_Type = value
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

    Public Property ChargeTime As Date
        Get
            Return m_ChargeTime
        End Get
        Set(value As Date)
            m_ChargeTime = value
        End Set
    End Property

    Public Property Units As Integer
        Get
            Return m_Units
        End Get
        Set(value As Integer)
            m_Units = value
        End Set
    End Property

    Public Property PriceNet As Double
        Get
            Return m_PriceNet
        End Get
        Set(value As Double)
            m_PriceNet = value
        End Set
    End Property

    Public Property PriceVat As Double
        Get
            Return m_PriceVat
        End Get
        Set(value As Double)
            m_PriceVat = value
        End Set
    End Property

    Public Property PriceTotal As Double
        Get
            Return m_PriceTotal
        End Get
        Set(value As Double)
            m_PriceTotal = value
        End Set
    End Property

    Public Property Description As String
        Get
            Return m_Description
        End Get
        Set(value As String)
            m_Description = value
        End Set
    End Property

    Public Overrides Function AsSqlString(ByVal withID As Boolean) As String
        Return IIf(withID, "'" & ID.ToString & "',", "") &
            Type & "," &
            "'" & m_RoomID.ToString & "'," &
            "'" & ChargeTime.ToString & "'," &
            Units & "," &
            PriceNet.ToString.Replace(",", ".") & "," &
            PriceVat.ToString.Replace(",", ".") & "," &
            PriceTotal.ToString.Replace(",", ".") & "," &
            "'" & Description & "'"
    End Function

End Class
