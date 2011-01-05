Imports NUnit.Framework
Imports NAudio.Wave
Imports VoiceRecorder.Audio

Namespace VoiceRecorder.Tests
    <TestFixture>
    Public Class PerformanceTests
        <Test>
        Public Sub TimingTest()
            Dim timer As New Stopwatch
            Dim iterations = 10
            Dim ms = timer.Time(Sub() ReadFromTestProvider(1024 * 1024, 4096), iterations)
            Console.WriteLine("{0} ms", ms)
        End Sub

        <Test>
        Public Sub ShortTest()
            Dim timer As New Stopwatch
            Dim ms = timer.Time(Sub() ReadFromTestProvider(16 * 1024, 4096))
            Console.WriteLine("{0} ms", ms)
        End Sub

        Private Shared Sub ReadFromTestProvider(ByVal bytesToRead As Integer, ByVal bufferSize As Integer)
            Dim source As New TestWaveProvider(44100, 1)
            Dim autoTune As New AutoTuneWaveProvider(source)
            Dim buffer(bufferSize - 1) As Byte
            Dim bytesRead = 0
            Do While bytesRead < bytesToRead
                bytesRead += autoTune.Read(buffer, 0, buffer.Length)
            Loop
        End Sub
    End Class

    ' http://stackoverflow.com/questions/232848/wrapping-stopwatch-timing-with-a-delegate-or-lambda
    Friend Module StopwatchExtensions
        <System.Runtime.CompilerServices.Extension()>
        Public Function Time(ByVal sw As Stopwatch, ByVal action As Action) As Long
            Return sw.Time(action, 1)
        End Function

        <System.Runtime.CompilerServices.Extension()>
        Public Function Time(ByVal sw As Stopwatch, ByVal action As Action, ByVal iterations As Integer) As Long
            sw.Reset()
            sw.Start()
            For i = 0 To iterations - 1
                action()
            Next i
            sw.Stop()

            Return sw.ElapsedMilliseconds
        End Function
    End Module

    Friend Class TestWaveProvider
        Inherits WaveProvider32
        Private testData() As Single
        Private testIndex As Integer

        Public Sub New(ByVal sampleRate As Integer, ByVal channels As Integer)
            MyBase.New(sampleRate, channels)
            testData = New Single(sampleRate * channels * 4 - 1){} ' four seconds of audio
            ' for now, our test data is a sine wave
            Dim Frequency As Single = 517
            Dim Amplitude = 0.25F
            For sample = 0 To testData.Length - 1
                testData(sample) = CSng(Amplitude * Math.Sin((2 * Math.PI * sample * Frequency) / sampleRate))
            Next sample
        End Sub

        Public Overrides Function Read(ByVal buffer() As Single, ByVal offset As Integer, ByVal sampleCount As Integer) As Integer

            For n = 0 To sampleCount - 1
                buffer(offset + n) = testData(testIndex)
                testIndex += 1
                If testIndex >= testData.Length Then
                    testIndex = 0
                End If
            Next n
            Return sampleCount
        End Function
    End Class
End Namespace
