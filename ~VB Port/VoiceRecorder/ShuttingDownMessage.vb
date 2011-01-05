Imports System.Text

Namespace VoiceRecorder
    Friend Class ShuttingDownMessage
        Private _currentViewName As String
        Public Property CurrentViewName As String
            Get
                Return _currentViewName
            End Get
            Private Set(ByVal value As String)
                _currentViewName = value
            End Set
        End Property

        Public Sub New(ByVal currentViewName As String)
            Me.CurrentViewName = currentViewName
        End Sub
    End Class
End Namespace
