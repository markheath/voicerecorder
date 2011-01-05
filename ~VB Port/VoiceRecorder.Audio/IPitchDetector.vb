Imports System.Text

Namespace VoiceRecorder.Audio
    Public Interface IPitchDetector
        Function DetectPitch(ByVal buffer() As Single, ByVal frames As Integer) As Single
    End Interface
End Namespace
