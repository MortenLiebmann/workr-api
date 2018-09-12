''' <summary>
''' WorkrServer hoved classe. Dette er starten på serveren.
''' I denne start klasse bliver resoursse korten defineret, et HttpController bliver instansieret og startet.
''' </summary>
Public Class WorkrServer
    Private WithEvents Controller As HttpController

    ''' <summary>
    ''' Starter serveren.
    ''' </summary>
    Public Sub Start()
        'Resoursse kort defination
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

        'HttpControlleren bliver startet
        Controller = New HttpController({"http://127.0.0.1:9877/", "http://192.168.1.88:9877/"}, map)
        Controller.StartListening()
    End Sub

    ''' <summary>
    ''' Lytter på et OnRequest event og skriver API kald data ud i konsolen.
    ''' </summary>
    ''' <param name="data"></param>
    Private Sub OnRequest(ByVal data As String) Handles Controller.OnRequest
        Console.WriteLine(data)
        Console.WriteLine("--------------------------------------------------------------------------")
    End Sub
End Class


