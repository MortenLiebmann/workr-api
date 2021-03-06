﻿Imports System.Security.Cryptography
Imports System.Text
Imports Newtonsoft.Json

''' <summary>
''' This static class handles user authentication, registration, and password hashing.
''' </summary>
Public Module Authenticator
    Private m_Hasher As SHA256 = SHA256.Create
    Private m_AuthUser As User = Nothing

    ''' <summary>
    ''' Holds the User entity of an authenticated request sender
    ''' Will be null if the sender of the request is not authenticated
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property AuthUser As User
        Get
            Return m_AuthUser
        End Get
    End Property

    ''' <summary>
    ''' Authenticates a user using the HTTP "Authorization" header
    ''' </summary>
    ''' <param name="authHeaderBase64">the HTTP "Authorization" header value in base64</param>
    ''' <returns>True if authentication was successful</returns>
    Public Function Authenticate(authHeaderBase64 As String) As Boolean
        Try
            m_AuthUser = Nothing
            authHeaderBase64 = authHeaderBase64.Replace("Basic ", "")
            Dim authHeaderString As String = Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderBase64))
            Dim authEmail As String = authHeaderString.Split(":")(0)
            Dim authPassword As String = authHeaderString.Split(":")(1)

            Dim authUser As User = (From e As User In DB.Users
                                    Where e.Email.ToLower = authEmail.ToLower
                                    Select e).Single

            If GetHashAsHex(authUser.Salt & authPassword) = authUser.PasswordHash.ToLower Then
                m_AuthUser = authUser
                Return True
            End If
        Catch ex As Exception
        End Try
        m_AuthUser = Nothing
        Return False
    End Function

    ''' <summary>
    ''' Registers a new user
    ''' </summary>
    ''' <param name="jsonUser">the JSON formatted user to register</param>
    ''' <param name="password">the plain/text password of the user to register</param>
    ''' <returns>The newly registerd User entity</returns>
    Public Function Register(jsonUser As String, password As String) As User
        If password.Length < 8 And Not password.Contains(":") Then Throw New PasswordRequirementException
        Dim newUser As New User
        Try
            newUser = JsonConvert.DeserializeObject(Of User)(jsonUser, JSONSettings)
            If String.IsNullOrEmpty(newUser.Email) Then Throw New UserRegistrationException
            If String.IsNullOrEmpty(newUser.Name) Then Throw New UserRegistrationException
            If DB.Users.Where(Function(e) e.Email.ToLower = newUser.Email.ToLower).Count > 0 Then Throw New UserRegistrationException
            newUser.ID = Guid.NewGuid
            newUser.Salt = GenerateSalt()
            newUser.PasswordHash = GetHashAsHex(newUser.Salt & password)
            DB.Users.Add(newUser)
            DB.SaveChanges()
            Return (From e As User In DB.Users
                    Where e.ID = newUser.ID
                    Select e).Single
        Catch ex As Exception
            DB.DiscardTrackedEntity(newUser)
            Throw ex
        End Try
    End Function

    ''' <summary>
    ''' Hashes the input data using the SHA256 hashing algorithm
    ''' </summary>
    ''' <param name="data">the string data to be hashed</param>
    ''' <returns>The output hash as hexidecimal</returns>
    Private Function GetHashAsHex(ByVal data As Byte()) As String
        Return BitConverter.ToString(m_Hasher.ComputeHash(data)).Replace("-", "").ToLower
    End Function

    ''' <summary>
    ''' Hashes the input data using the SHA256 hashing algorithm
    ''' </summary>
    ''' <param name="data">the byte array data to be hashed</param>
    ''' <returns>The output hash as hexidecimal</returns>
    Private Function GetHashAsHex(ByVal data As String) As String
        Dim dataBytes As Byte() = Encoding.UTF8.GetBytes(data)
        Return BitConverter.ToString(m_Hasher.ComputeHash(dataBytes)).Replace("-", "").ToLower
    End Function

    ''' <summary>
    ''' cryptographically securely generates a random 32 base64 character string
    ''' </summary>
    ''' <returns>The random salt</returns>
    Private Function GenerateSalt() As String
        Dim rand As New RNGCryptoServiceProvider()
        Dim saltBytes As Byte() = New Byte(23) {} '32 base64 chars
        rand.GetBytes(saltBytes)
        Return Convert.ToBase64String(saltBytes)
    End Function

    'Authenticator exceptions
    Public Class NotAuthorizedException
        Inherits Exception

        Private m_Message As String = "Not authorized."

        Public Sub New()
        End Sub

        Public Sub New(message As String)
            m_Message = message
        End Sub
        Public Overrides ReadOnly Property Message As String
            Get
                Return m_Message
            End Get
        End Property
    End Class

    Public Class UserRegistrationException
        Inherits Exception

        Public Overrides ReadOnly Property Message As String
            Get
                Return "Error registering user."
            End Get
        End Property
    End Class

    Public Class PasswordRequirementException
        Inherits Exception

        Public Overrides ReadOnly Property Message As String
            Get
                Return "Password must be no less than 8 characters."
            End Get
        End Property
    End Class
End Module
