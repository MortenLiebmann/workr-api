''' <summary>
''' WorkrServer main class. This is the where the server starts
''' The resource map is defined in this class, and a HttpController instance is created and started.
''' </summary>
Public Class WorkrServer
    Private WithEvents Controller As HttpController

    ''' <summary>
    ''' Starts the server
    ''' </summary>
    Public Sub Start()
        'Resource map definition
        Dim map As New Dictionary(Of String, Object) From {
            {"users", New Resource(Of User)(DB.Users)},
            {"userimages", New Resource(Of UserImage)(DB.UserImages)},
            {"posts", New Resource(Of Post)(DB.Posts)},
            {"postimages", New Resource(Of PostImage)(DB.PostImages)},
            {"chats", New Resource(Of Chat)(DB.Chats)},
            {"messages", New Resource(Of Message)(DB.Messages)},
            {"ratings", New Resource(Of Rating)(DB.Ratings)},
            {"posttags", New Resource(Of PostTag)(DB.PostTags)},
            {"posttagreferences", New Resource(Of PostTagReference)(DB.PostTagReferences)},
            {"postbids", New Resource(Of PostBid)(DB.PostBids)}
        }

        'HttpControlleren is startet
        Controller = New HttpController({"http://127.0.0.1:9877/", "http://10.0.0.37:9877/", "http://skurk.info:9877/"}, map)
        Controller.StartListening()
    End Sub

    ''' <summary>
    ''' Lytter på et OnRequest event og skriver API kald data ud i konsolen.
    ''' Listens the HttpController.OnRequest event and prints the data in the console. 
    ''' </summary>
    ''' <param name="data">the request data</param>
    Private Sub OnRequest(ByVal data As String) Handles Controller.OnRequest
        Console.WriteLine(data)
        Console.WriteLine("--------------------------------------------------------------------------")
    End Sub
End Class


