Imports System.Data.Entity
Imports WorkrServer

Public MustInherit Class Table(Of T As Entity)
    Public MustOverride ReadOnly Property DbSet As DbSet(Of T)
End Class
