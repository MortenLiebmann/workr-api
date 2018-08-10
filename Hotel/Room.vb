Imports System.Reflection
Imports Newtonsoft.Json

Public Class Room
    Inherits Item

    Private m_Number As String
    Private m_View As Integer
    Private m_BedType As Integer
    Private m_Pricing As Integer
    Private m_State As Integer
    Private m_Extras As Integer

    <JsonConstructor>
    Public Sub New(iD As Guid, number As String, view As Integer, bedtype As Integer, pricing As Integer, state As Integer, extras As Integer)
        Me.ID = iD
        Me.Number = number
        Me.View = view
        Me.BedType = bedtype
        Me.Pricing = pricing
        Me.State = state
        Me.Extras = extras
    End Sub

    Public Sub New()
        MyBase.New()
    End Sub

    Public Property Number As String
        Get
            Return m_Number
        End Get
        Set(value As String)
            m_Number = value
        End Set
    End Property

    Public Property View As Integer
        Get
            Return m_View
        End Get
        Set(value As Integer)
            m_View = value
        End Set
    End Property

    Public Property BedType As Integer
        Get
            Return m_BedType
        End Get
        Set(value As Integer)
            m_BedType = value
        End Set
    End Property

    Public Property Pricing As Integer
        Get
            Return m_Pricing
        End Get
        Set(value As Integer)
            m_Pricing = value
        End Set
    End Property

    Public Property State As Integer
        Get
            Return m_State
        End Get
        Set(value As Integer)
            m_State = value
        End Set
    End Property

    Public Property Extras As Integer
        Get
            Return m_Extras
        End Get
        Set(value As Integer)
            m_Extras = value
        End Set
    End Property

    Public Overrides Function AsSqlString(ByVal withID As Boolean) As String
        Return IIf(withID, "'" & ID.ToString & "',", "") &
            "'" & Number & "'," &
            View & "," &
            BedType & "," &
            Pricing & "," &
            State & "," &
            Extras
    End Function

End Class
