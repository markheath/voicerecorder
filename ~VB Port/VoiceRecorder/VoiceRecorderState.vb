Imports System.Text
Imports System.IO
Imports VoiceRecorder.Audio
Imports GalaSoft.MvvmLight.Messaging

Namespace VoiceRecorder
    Friend Class NavigateMessage
        Private _targetView As String
        Public Property TargetView As String
            Get
                Return _targetView
            End Get
            Private Set(ByVal value As String)
                _targetView = value
            End Set
        End Property

        Private _state As Object
        Public Property State As Object
            Get
                Return _state
            End Get
            Private Set(ByVal value As Object)
                _state = value
            End Set
        End Property

        Public Sub New(ByVal targetView As String, ByVal state As Object)
            Me.TargetView = targetView
            Me.State = state
        End Sub
    End Class


    Friend Class VoiceRecorderState
        Private _recordingFileName As String
        Private _effectedFileName As String
        Private _autoTuneSettings As AutoTuneSettings

        Public Sub New(ByVal recordingFileName As String, ByVal effectedFileName As String)
            Me.RecordingFileName = recordingFileName
            Me.EffectedFileName = effectedFileName
            Me._autoTuneSettings = New AutoTuneSettings
        End Sub

        Public Property RecordingFileName As String
            Get
                Return _recordingFileName
            End Get
            Set(ByVal value As String)
                If (_recordingFileName IsNot Nothing) AndAlso (_recordingFileName <> value) Then
                    DeleteFile(_recordingFileName)
                End If
                Me._recordingFileName = value
            End Set
        End Property

        Public Property EffectedFileName As String
            Get
                Return _effectedFileName
            End Get
            Set(ByVal value As String)
                If (_effectedFileName IsNot Nothing) AndAlso (_effectedFileName <> value) Then
                    DeleteFile(_effectedFileName)
                End If
                Me._effectedFileName = value
            End Set
        End Property

        Public ReadOnly Property ActiveFile As String
            Get
                If _autoTuneSettings.Enabled AndAlso (Not String.IsNullOrEmpty(EffectedFileName)) Then
                    Return EffectedFileName
                End If
                Return RecordingFileName
            End Get
        End Property

        Public ReadOnly Property AutoTuneSettings As AutoTuneSettings
            Get
                Return _autoTuneSettings
            End Get
        End Property

        Public Sub DeleteFiles()
            Me.RecordingFileName = Nothing
            Me.EffectedFileName = Nothing
        End Sub

        Private Sub DeleteFile(ByVal fileName As String)
            If (Not String.IsNullOrEmpty(fileName)) AndAlso File.Exists(fileName) Then
                File.Delete(fileName)
            End If
        End Sub
    End Class
End Namespace
