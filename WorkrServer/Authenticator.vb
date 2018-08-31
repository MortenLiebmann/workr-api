Imports System.Security.Cryptography
Imports System.Text

Public Module Authenticator
    Private hasher As SHA256 = SHA256.Create

    Public Function GetHashAsHex(ByVal data As Byte()) As String
        Return BitConverter.ToString(hasher.ComputeHash(data)).Replace("-", "").ToLower
    End Function

    Public Function Authenticate(authHeaderBase64 As String) As Boolean
        Try
            authHeaderBase64 = authHeaderBase64.Replace("Basic ", "")
            Dim authHeaderString As String = Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderBase64))
            Dim authEmail As String = authHeaderString.Split(":")(0)
            Dim authPassword As String = authHeaderString.Split(":")(1)

            Dim authUser As User = (From e As User In DB.Users
                                    Where e.Email.ToLower = authEmail.ToLower
                                    Select e).Single

            If GetHashAsHex(Encoding.UTF8.GetBytes(authUser.Salt & authPassword)) = authUser.PasswordHash.ToLower Then
                Return True
            End If
        Catch ex As Exception
        End Try
        Return False
    End Function

    'Function AuthLogin(ByVal collection As FormCollection) As ActionResult
    '    Dim U As User = Nothing
    '    Dim username As String = Nothing
    '    Dim password As String = Nothing
    '    Try
    '        username = Request("username").ToLower
    '        password = Request("password")

    '        If Not String.IsNullOrEmpty(username) Or Not String.IsNullOrEmpty(password) Then
    '            U = (From e As User In C.Users
    '                 Where e.NetLogin.ToLower = username And e.LocalPhone = password
    '                 Select e).AsNoTracking.FirstOrDefault
    '        End If

    '        If U Is Nothing Then
    '            Return RedirectToAction("Login")
    '        End If

    '        Dim rand As New RNGCryptoServiceProvider()
    '        Dim tokenBytes As Byte() = New Byte(23) {} '32 base64 chars
    '        Dim selectorBytes As Byte() = New Byte(8) {} '12 base64 chars
    '        Dim base64Token As String
    '        Dim base64Selector As String
    '        Dim token As New Token
    '        Dim cookie As New HttpCookie("SmartDirAuthToken")

    '        rand.GetBytes(tokenBytes)
    '        rand.GetBytes(selectorBytes)
    '        base64Token = Convert.ToBase64String(tokenBytes)
    '        base64Selector = Convert.ToBase64String(selectorBytes)

    '        token.TokenHash = GetHashAsHex(tokenBytes)
    '        token.Selector = base64Selector
    '        token.UserID = U.UniqueID
    '        token.Expires = DateTime.UtcNow().AddDays(30)

    '        C.Tokens.Add(token)
    '        C.SaveChanges()

    '        Session("user") = U

    '        cookie.Value = base64Selector & "." & base64Token
    '        cookie.Expires = DateTime.UtcNow.AddDays(30)
    '        cookie.HttpOnly = True

    '        Response.SetCookie(cookie)

    '        If Request.Cookies("SmartDirFirstLogin") Is Nothing Then
    '            Dim firstlogin As New HttpCookie("SmartDirFirstLogin")
    '            firstlogin.Expires = DateTime.UtcNow.AddYears(10)
    '            Response.SetCookie(firstlogin)
    '            Return RedirectToAction("Help")
    '        End If

    '        Return RedirectToAction("Index")
    '    Catch ex As Exception
    '        LogMessage("Home", vbCrLf &
    '            "***************************" & vbCrLf &
    '            "AuthLogin *** ERROR ***" & vbCrLf &
    '            "MSG: " & ex.Message & vbCrLf &
    '            "USER: " & JsonConvert.SerializeObject(U) & vbCrLf &
    '            "USERNAME INPUT: " & username & vbCrLf &
    '            "IP ADDRESS: " & Request.UserHostAddress & vbCrLf &
    '            "***************************"
    '        )
    '    End Try

    '    Return RedirectToAction("Login")
    'End Function

End Module
