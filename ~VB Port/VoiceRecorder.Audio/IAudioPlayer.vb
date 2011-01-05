Imports System.Text

Namespace VoiceRecorder.Audio
    Public Interface IAudioPlayer
        Inherits IDisposable
        Sub LoadFile(ByVal path As String)
        Sub Play()
        Sub [Stop]()
        Property CurrentPosition As TimeSpan
        Property StartPosition As TimeSpan
        Property EndPosition As TimeSpan
    End Interface
End Namespace
