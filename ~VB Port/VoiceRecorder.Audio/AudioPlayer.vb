Imports System.Text
Imports NAudio.Wave

Namespace VoiceRecorder.Audio
    Public Class AudioPlayer
        Implements IAudioPlayer
        Private waveOut As WaveOut
        Private inStream As TrimWaveStream

        Public Sub New()
        End Sub

        Public Sub LoadFile(ByVal path As String) Implements IAudioPlayer.LoadFile
            CloseWaveOut()
            CloseInStream()
            Me.inStream = New TrimWaveStream(New WaveFileReader(path))
        End Sub

        Public Sub Play() Implements IAudioPlayer.Play
            CreateWaveOut()
            If waveOut.PlaybackState = PlaybackState.Stopped Then
                inStream.Position = 0
                waveOut.Play()
            End If
        End Sub

        Private Sub CreateWaveOut()
            If waveOut Is Nothing Then
                waveOut = New WaveOut
                waveOut.Init(inStream)
                AddHandler waveOut.PlaybackStopped, AddressOf waveOut_PlaybackStopped
            End If
        End Sub

        Private Sub waveOut_PlaybackStopped(ByVal sender As Object, ByVal e As EventArgs)
            Me.PlaybackState = PlaybackState.Stopped
        End Sub

        Public Sub [Stop]() Implements IAudioPlayer.Stop
            waveOut.Stop()
            inStream.Position = 0
        End Sub

        Public Property StartPosition As TimeSpan Implements IAudioPlayer.StartPosition
            Get
                Return inStream.StartPosition
            End Get
            Set(ByVal value As TimeSpan)
                inStream.StartPosition = value
            End Set
        End Property

        Public Property EndPosition As TimeSpan Implements IAudioPlayer.EndPosition
            Get
                Return inStream.EndPosition
            End Get
            Set(ByVal value As TimeSpan)
                inStream.EndPosition = value
            End Set
        End Property

        Public Property CurrentPosition As TimeSpan Implements IAudioPlayer.CurrentPosition

        Private _playbackState As PlaybackState
        Public Property PlaybackState As PlaybackState
            Get
                Return _playbackState
            End Get
            Private Set(ByVal value As PlaybackState)
                _playbackState = value
            End Set
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            CloseWaveOut()
            CloseInStream()
        End Sub

        Private Sub CloseInStream()
            If inStream IsNot Nothing Then
                inStream.Dispose()
                inStream = Nothing
            End If
        End Sub

        Private Sub CloseWaveOut()
            If waveOut IsNot Nothing Then
                waveOut.Dispose()
                waveOut = Nothing
            End If
        End Sub
    End Class
End Namespace
