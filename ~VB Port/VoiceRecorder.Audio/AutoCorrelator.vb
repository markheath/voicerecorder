Namespace VoiceRecorder.Audio
    ' originally based on awesomebox, modified by Mark Heath
    Public Class AutoCorrelator
        Implements IPitchDetector
        Private prevBuffer() As Single
        Private minOffset As Integer
        Private maxOffset As Integer
        Private sampleRate As Single

        Public Sub New(ByVal sampleRate As Integer)
            Me.sampleRate = CSng(sampleRate)
            Dim minFreq = 85
            Dim maxFreq = 255

            Me.maxOffset = sampleRate \ minFreq
            Me.minOffset = sampleRate \ maxFreq
        End Sub

        Public Function DetectPitch(ByVal buffer() As Single, ByVal frames As Integer) As Single Implements IPitchDetector.DetectPitch
            If prevBuffer Is Nothing Then
                prevBuffer = New Single(frames - 1){}
            End If
            Dim secCor As Single = 0
            Dim secLag = 0

            Dim maxCorr As Single = 0
            Dim maxLag = 0

            ' starting with low frequencies, working to higher
            For lag = maxOffset To minOffset Step -1
                Dim corr As Single = 0 ' this is calculated as the sum of squares
                For i = 0 To frames - 1
                    Dim oldIndex = i - lag
                    Dim sample = (If(oldIndex < 0, prevBuffer(frames + oldIndex), buffer(oldIndex)))
                    corr += (sample * buffer(i))
                Next i
                If corr > maxCorr Then
                    maxCorr = corr
                    maxLag = lag
                End If
                If corr >= 0.9 * maxCorr Then
                    secCor = corr
                    secLag = lag
                End If
            Next lag
            For n = 0 To frames - 1
                prevBuffer(n) = buffer(n)
            Next n
            Dim noiseThreshold = frames / 1000.0F
            'Debug.WriteLine(String.Format("Max Corr: {0} ({1}), Sec Corr: {2} ({3})", Me.sampleRate / maxLag, maxCorr, Me.sampleRate / secLag, secCor))
            If maxCorr < noiseThreshold OrElse maxLag = 0 Then
                Return 0.0F
            End If
            'Return 44100.0f / secLag '--works better for singing
            Return Me.sampleRate / maxLag
        End Function
    End Class
End Namespace
