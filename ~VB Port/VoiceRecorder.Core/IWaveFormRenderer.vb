Imports System.Text

Namespace VoiceRecorder.Core
    Public Interface IWaveFormRenderer
        Sub AddValue(ByVal maxValue As Single, ByVal minValue As Single)
    End Interface
End Namespace
