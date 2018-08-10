Imports Hotel
Imports Newtonsoft.Json

Public Class Booking
    Inherits Item

    Private m_ReservationIDs As Guid()
    Private m_TempReservations As Reservation()
    Private m_GuestIDs As Guid()
    Private m_TempGuests As Guest()
    Private m_CreditCardName As String
    Private m_CreditCardType As String
    Private m_CreditCardNumber As String
    Private m_State As Integer

    <JsonConstructor>
    Public Sub New(iD As Guid, reservations As Reservation(), guests As Guest(), creditCardName As String, creditCardType As String, creditCardNumber As String, state As Integer)
        Me.ID = iD
        Me.Reservations = reservations
        Me.Guests = guests
        Me.CreditCardName = creditCardName
        Me.CreditCardType = creditCardType
        Me.CreditCardNumber = creditCardNumber
        Me.State = state
    End Sub

    Public Sub New(iD As Guid, reservationIDs As Guid(), guestIDs As Guid(), creditCardName As String, creditCardType As String, creditCardNumber As String, state As Integer)
        Me.ID = iD
        Me.m_ReservationIDs = reservationIDs
        Me.m_GuestIDs = guestIDs
        'Dim glist As New List(Of Guest)
        'For Each g As Guid In guestIDs
        '    glist.Add(Map("guest").FindByID(g.ToString))
        'Next
        'Dim rlist As New List(Of Reservation)
        'For Each g As Guid In reservationIDs
        '    rlist.Add(Map("reservation").FindByID(g.ToString))
        'Next

        'Me.Guests = glist.ToArray
        'Me.Reservations = rlist.ToArray

        Me.CreditCardName = creditCardName
        Me.CreditCardType = creditCardType
        Me.CreditCardNumber = creditCardNumber
        Me.State = state
    End Sub

    Public Sub New()
        MyBase.New()
    End Sub

    Public Property Reservations As Reservation()
        Get
            Dim list As New List(Of Reservation)
            If m_ReservationIDs IsNot Nothing Then
                For i As Integer = 0 To m_ReservationIDs.Length - 1
                    Dim res = Map("reservation").FindByID(m_ReservationIDs(i).ToString)
                    If Not res = Nothing Then
                        list.Add(res)
                    Else
                        If Not m_TempReservations Is Nothing Then
                            list.Add(m_TempReservations(i))
                        End If
                    End If
                Next
            End If

            Return list.ToArray
        End Get
        Set(value As Reservation())
            m_TempReservations = value
            If value IsNot Nothing Then
                ReDim m_ReservationIDs(value.Length - 1)
                For i As Integer = 0 To value.Length - 1
                    m_ReservationIDs(i) = value(i).ID
                Next
            End If
        End Set
    End Property

    Public Property Guests As Guest()
        Get
            Dim list As New List(Of Guest)
            If m_GuestIDs IsNot Nothing Then
                For i As Integer = 0 To m_GuestIDs.Length - 1
                    Dim g = Map("guest").FindByID(m_GuestIDs(i).ToString)
                    If Not g = Nothing Then
                        list.Add(g)
                    Else
                        If Not m_TempGuests Is Nothing Then
                            list.Add(m_TempGuests(i))
                        End If
                    End If
                Next
            End If

            Return list.ToArray
        End Get
        Set(value As Guest())
            m_TempGuests = value
            If value IsNot Nothing Then
                ReDim m_GuestIDs(value.Length - 1)
                For i As Integer = 0 To value.Length - 1
                    m_GuestIDs(i) = value(i).ID
                Next
            End If
        End Set
    End Property

    Public Property CreditCardName As String
        Get
            Return m_CreditCardName
        End Get
        Set(value As String)
            m_CreditCardName = value
        End Set
    End Property

    Public Property CreditCardType As String
        Get
            Return m_CreditCardType
        End Get
        Set(value As String)
            m_CreditCardType = value
        End Set
    End Property

    Public Property CreditCardNumber As String
        Get
            Return m_CreditCardNumber
        End Get
        Set(value As String)
            m_CreditCardNumber = value
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

    Public Overrides Function AsSqlString(ByVal withID As Boolean) As String
        Return IIf(withID, "'" & ID.ToString & "',", "") &
            "'{""" & String.Join(""",""", m_ReservationIDs) & """}'," &
            "'{""" & String.Join(""",""", m_GuestIDs) & """}'," &
            "'" & CreditCardName & "'," &
            "'" & CreditCardType & "'," &
            "'" & CreditCardNumber & "'," &
            State
    End Function

End Class
