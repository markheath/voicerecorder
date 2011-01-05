Imports System.Text
Imports VoiceRecorder.Audio
Imports GalaSoft.MvvmLight

Namespace VoiceRecorder
    Public Class NoteViewModel
        Inherits ViewModelBase
        Public Sub New(ByVal note As Note, ByVal displayName As String)
            Me.Note = note
            Me.DisplayName = displayName
        End Sub

        Public Property Note As Note

        Private _selected As Boolean
        Public Property Selected As Boolean
            Get
                Return _selected
            End Get
            Set(ByVal value As Boolean)
                If _selected <> value Then
                    _selected = value
                    RaisePropertyChanged("Selected")
                End If
            End Set
        End Property

        Public Property DisplayName As String
    End Class
End Namespace
