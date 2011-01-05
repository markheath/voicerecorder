Imports System.Text

Namespace VoiceRecorder.Audio
    ' FFT based pitch detector. seems to work best with block sizes of 4096
    Public Class FftPitchDetector
        Implements IPitchDetector
        Private sampleRate As Single

        Public Sub New(ByVal sampleRate As Single)
            Me.sampleRate = sampleRate
        End Sub

        ' http://en.wikipedia.org/wiki/Window_function
        Private Function HammingWindow(ByVal n As Integer, ByVal _N As Integer) As Single
            Return 0.54F - 0.46F * CSng(Math.Cos((2 * Math.PI * n) / (_N - 1)))
        End Function

        Private fftBuffer() As Single
        Private prevBuffer() As Single

        Public Function DetectPitch(ByVal buffer() As Single,
                                    ByVal inFrames As Integer) As Single Implements IPitchDetector.DetectPitch
            Dim window As Func(Of Integer, Integer, Single) = AddressOf HammingWindow
            If prevBuffer Is Nothing Then
                prevBuffer = New Single(inFrames - 1) {}
            End If

            ' double frames since we are combining present and previous buffers
            Dim frames = inFrames * 2
            If fftBuffer Is Nothing Then
                fftBuffer = New Single(frames * 2 - 1) {} ' times 2 because it is complex input
            End If

            For n = 0 To frames - 1
                If n < inFrames Then
                    fftBuffer(n * 2) = prevBuffer(n) * window(n, frames)
                    fftBuffer(n * 2 + 1) = 0 ' need to clear out as fft modifies buffer
                Else
                    fftBuffer(n * 2) = buffer(n - inFrames) * window(n, frames)
                    fftBuffer(n * 2 + 1) = 0 ' need to clear out as fft modifies buffer
                End If
            Next n

            ' assuming frames is a power of 2
            SmbPitchShift.smbFft(fftBuffer, frames, -1)

            Dim binSize = sampleRate / frames
            Dim minBin = CInt(Fix(85 / binSize))
            Dim maxBin = CInt(Fix(300 / binSize))
            Dim maxIntensity = 0.0F
            Dim maxBinIndex = 0
            For bin = minBin To maxBin
                Dim real = fftBuffer(bin * 2)
                Dim imaginary = fftBuffer(bin * 2 + 1)
                Dim intensity = real * real + imaginary * imaginary
                If intensity > maxIntensity Then
                    maxIntensity = intensity
                    maxBinIndex = bin
                End If
            Next bin
            Return binSize * maxBinIndex
        End Function
    End Class
End Namespace
