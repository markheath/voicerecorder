Imports System.Text
Imports NUnit.Framework
Imports VoiceRecorder.Audio

Namespace VoiceRecorder.Tests
    <TestFixture>
    Public Class AutoCorrelatorTests
        Private sampleRate As Integer = 44100

        <Test>
        Public Sub TestEmptyBufferDoesntDetectAPitch()
            Dim autoCorrelator As New AutoCorrelator(sampleRate)
            Dim pitch As Single = autoCorrelator.DetectPitch(New Single(1023){}, 1024)
            Assert.AreEqual(0f,pitch)
        End Sub

        <Test>
        Public Sub TestSineWaveDetectionFft()
            Dim buffer(4095) As Single ' FFT needs at least 4096 to get the granularity
            Dim pitchDetector As IPitchDetector = New FftPitchDetector(sampleRate)
            TestPitchDetection(buffer, pitchDetector)
        End Sub

        <Test>
        Public Sub TestSineWaveDetectionAutocorrelator()
            Dim buffer(4095) As Single
            Dim pitchDetector As IPitchDetector = New AutoCorrelator(sampleRate)
            TestPitchDetection(buffer, pitchDetector)
        End Sub

        Private Sub TestPitchDetection(ByVal buffer() As Single, ByVal pitchDetector As IPitchDetector)
            For midiNoteNumber = 45 To 62
                Dim freq = CSng(8.175 * Math.Pow(1.05946309, midiNoteNumber))
                SetFrequency(buffer, freq)
                Dim detectedPitch = pitchDetector.DetectPitch(buffer, buffer.Length)
                ' since the autocorrelator works with a lag, give it two shots at the same buffer
                detectedPitch = pitchDetector.DetectPitch(buffer, buffer.Length)
                Console.WriteLine("Testing for {0:F2}Hz, got {1:F2}Hz", freq, detectedPitch)
                'Assert.AreEqual(detectedPitch, freq, 0.5);
            Next midiNoteNumber
        End Sub

        Private Sub SetFrequency(ByVal buffer() As Single, ByVal frequency As Single)
            Dim amplitude = 0.25F
            For n = 0 To buffer.Length - 1
                buffer(n) = CSng(amplitude * Math.Sin((2 * Math.PI * n * frequency) / sampleRate))
            Next n
        End Sub
    End Class
End Namespace
