Imports System.Text
Imports NAudio.Wave
Imports NAudio.Mixer
Imports System.IO

Namespace VoiceRecorder.Audio
    Public Class AudioRecorder
        Implements IAudioRecorder
        Private waveIn As WaveIn
        Private _sampleAggregator As SampleAggregator
        Private volumeControl As UnsignedMixerControl
        Private desiredVolume As Double = 100
        Private _recordingState As RecordingState
        Private writer As WaveFileWriter
        Private _recordingFormat As WaveFormat

        Public Event Stopped As EventHandler Implements IAudioRecorder.Stopped

        Public Sub New()
            _sampleAggregator = New SampleAggregator
            RecordingFormat = New WaveFormat(44100, 1)
        End Sub

        Public Property RecordingFormat As WaveFormat Implements IAudioRecorder.RecordingFormat
            Get
                Return _recordingFormat
            End Get
            Set(ByVal value As WaveFormat)
                _recordingFormat = value
                _sampleAggregator.NotificationCount = value.SampleRate \ 10
            End Set
        End Property

        Public Sub BeginMonitoring(ByVal recordingDevice As Integer) Implements IAudioRecorder.BeginMonitoring
            If _recordingState <> RecordingState.Stopped Then
                Throw New InvalidOperationException("Can't begin monitoring while we are in this state: " & _recordingState.ToString())
            End If
            waveIn = New WaveIn
            waveIn.DeviceNumber = recordingDevice
            AddHandler waveIn.DataAvailable, AddressOf waveIn_DataAvailable
            AddHandler waveIn.RecordingStopped, AddressOf waveIn_RecordingStopped
            waveIn.WaveFormat = _recordingFormat
            waveIn.StartRecording()
            TryGetVolumeControl()
            _recordingState = RecordingState.Monitoring
        End Sub

        Private Sub waveIn_RecordingStopped(ByVal sender As Object, ByVal e As EventArgs)
            _recordingState = RecordingState.Stopped
            writer.Dispose()
            RaiseEvent Stopped(Me, EventArgs.Empty)
        End Sub

        Public Sub BeginRecording(ByVal waveFileName As String) Implements IAudioRecorder.BeginRecording
            If _recordingState <> RecordingState.Monitoring Then
                Throw New InvalidOperationException("Can't begin recording while we are in this state: " & _recordingState.ToString())
            End If
            writer = New WaveFileWriter(waveFileName, _recordingFormat)
            _recordingState = RecordingState.Recording
        End Sub

        Public Sub [Stop]() Implements IAudioRecorder.Stop
            If _recordingState = RecordingState.Recording Then
                _recordingState = RecordingState.RequestedStop
                waveIn.StopRecording()
            End If
        End Sub

        Private Sub TryGetVolumeControl()
            Dim waveInDeviceNumber = waveIn.DeviceNumber
            If Environment.OSVersion.Version.Major >= 6 Then ' Vista and over
                Dim mixerLine = waveIn.GetMixerLine()
                'New MixerLine(CType(waveInDeviceNumber, IntPtr), 0, MixerFlags.WaveIn)
                For Each control In mixerLine.Controls
                    If control.ControlType = MixerControlType.Volume Then
                        Me.volumeControl = TryCast(control, UnsignedMixerControl)
                        MicrophoneLevel = desiredVolume
                        Exit For
                    End If
                Next control
            Else
                Dim mixer = New Mixer(waveInDeviceNumber)
                For Each destination In mixer.Destinations
                    If destination.ComponentType = MixerLineComponentType.DestinationWaveIn Then
                        For Each source In destination.Sources
                            If source.ComponentType = MixerLineComponentType.SourceMicrophone Then
                                For Each control In source.Controls
                                    If control.ControlType = MixerControlType.Volume Then
                                        volumeControl = TryCast(control, UnsignedMixerControl)
                                        MicrophoneLevel = desiredVolume
                                        Exit For
                                    End If
                                Next control
                            End If
                        Next source
                    End If
                Next destination
            End If

        End Sub

        Public Property MicrophoneLevel As Double Implements IAudioRecorder.MicrophoneLevel
            Get
                Return desiredVolume
            End Get
            Set(ByVal value As Double)
                desiredVolume = value
                If volumeControl IsNot Nothing Then
                    volumeControl.Percent = value
                End If
            End Set
        End Property

        Public ReadOnly Property SampleAggregator As SampleAggregator Implements IAudioRecorder.SampleAggregator
            Get
                Return _sampleAggregator
            End Get
        End Property

        Public ReadOnly Property RecordingState As RecordingState Implements IAudioRecorder.RecordingState
            Get
                Return _recordingState
            End Get
        End Property

        Public ReadOnly Property RecordedTime As TimeSpan Implements IAudioRecorder.RecordedTime
            Get
                If writer Is Nothing Then
                    Return TimeSpan.Zero
                Else
                    Return TimeSpan.FromSeconds(CDbl(writer.Length) / writer.WaveFormat.AverageBytesPerSecond)
                End If
            End Get
        End Property

        Private Sub waveIn_DataAvailable(ByVal sender As Object, ByVal e As WaveInEventArgs)
            Dim buffer() = e.Buffer
            Dim bytesRecorded = e.BytesRecorded
            WriteToFile(buffer, bytesRecorded)

            For index = 0 To e.BytesRecorded - 1 Step 2
                Dim sample = CShort(buffer(index + 1)) << 8 Or CShort(buffer(index + 0))
                Dim sample32 = sample / 32768.0F
                _sampleAggregator.Add(sample32)
            Next index
        End Sub

        Private Sub WriteToFile(ByVal buffer() As Byte, ByVal bytesRecorded As Integer)
            Dim maxFileLength As Long = Me._recordingFormat.AverageBytesPerSecond * 60

            If _recordingState = RecordingState.Recording OrElse _recordingState = RecordingState.RequestedStop Then
                Dim toWrite = CInt(Fix(Math.Min(maxFileLength - writer.Length, bytesRecorded)))
                If toWrite > 0 Then
                    writer.WriteData(buffer, 0, bytesRecorded)
                Else
                    Me.Stop()
                End If
            End If
        End Sub
    End Class
End Namespace
