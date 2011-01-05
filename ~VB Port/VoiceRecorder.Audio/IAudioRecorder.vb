Imports System.Text
Imports NAudio.Wave

Namespace VoiceRecorder.Audio
    Public Interface IAudioRecorder
        Sub BeginMonitoring(ByVal recordingDevice As Integer)
        Sub BeginRecording(ByVal path As String)
        Sub [Stop]()
        Property MicrophoneLevel As Double
        ReadOnly Property RecordingState As RecordingState
        ReadOnly Property SampleAggregator As SampleAggregator
        Event Stopped As EventHandler
        Property RecordingFormat As WaveFormat
        ReadOnly Property RecordedTime As TimeSpan
    End Interface
End Namespace
