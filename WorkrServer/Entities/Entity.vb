Imports System.Reflection
Imports WorkrServer

Public MustInherit Class Entity
    Public MustOverride Property ID As Guid?

    Public MustOverride Function Expand() As Object
End Class
