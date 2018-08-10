Imports System.Reflection

Public MustInherit Class Item
    Implements IEquatable(Of Item)

    Protected Friend m_ID As Guid

    Public Property ID As Guid
        Get
            Return m_ID
        End Get
        Set(value As Guid)
            m_ID = value
        End Set
    End Property

    Public Sub New()
    End Sub

    ''' <summary>
    ''' returns object as PostgreSQL string
    ''' </summary>
    ''' <param name="withID"></param>
    ''' <returns></returns>
    Public MustOverride Function AsSqlString(ByVal withID As Boolean) As String

    Public Shared Operator =(ByVal x As Item, ByVal y As Item)
        If x Is Nothing And y Is Nothing Then Return True
        If x Is Nothing Or y Is Nothing Then Return False
        Return x.PropsEquals(y)
    End Operator

    Public Shared Operator <>(ByVal x As Item, ByVal y As Item)
        If x Is Nothing Or y Is Nothing Then Return True
        Return Not x.PropsEquals(y)
    End Operator

    ''' <summary>
    ''' ID check
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    Public Overloads Function Equals(ByVal obj As Item) As Boolean Implements IEquatable(Of Item).Equals
        If Me.ID = obj.ID Then Return True
        Return False
    End Function

    ''' <summary>
    ''' compares all properties that are not null
    ''' </summary>
    ''' <param name="objectToMatch"></param>
    ''' <returns></returns>
    Public Function PropsEquals(ByVal objectToMatch As Item) As Boolean
        For Each prop As PropertyInfo In objectToMatch.GetType.GetProperties()
            If prop.PropertyType.IsArray Then
                If prop.GetValue(objectToMatch).Length > 0 Then
                    If Not IsSubsetOf(prop.GetValue(objectToMatch), prop.GetValue(Me)) Then Return False
                End If
            ElseIf Not prop.GetValue(objectToMatch) = Nothing AndAlso
            Not prop.GetValue(Me) = Nothing AndAlso
            Not prop.GetValue(objectToMatch) = prop.GetValue(Me) Then
                Return False
            End If
        Next

        Return True
    End Function

    ''' <summary>
    ''' returns object as JSON formatted string
    ''' </summary>
    ''' <returns></returns>
    Public Function AsJsonString() As String
        Return Newtonsoft.Json.JsonConvert.SerializeObject(Me)
    End Function

End Class
