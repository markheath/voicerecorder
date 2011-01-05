' this class based on code from awesomebox, a project created by by Ravi Parikh and Keegan Poppen, used with permission
' http://decabear.com/awesomebox.html
Imports System.Text

Namespace VoiceRecorder.Audio
    Friend Class PitchShifter
        Private detectedNote As Integer
        Protected detectedPitch As Single 'set == inputPitch when shiftPitch is called
        Protected shiftedPitch As Single 'set == the target pitch when shiftPitch is called
        Private numshifts As Integer 'number of stored detectedPitch, shiftedPitch pairs stored for the viewer (more = slower, less = faster)
        Private shifts As Queue(Of PitchShift)
        Protected currPitch As Integer
        Protected attack As Integer
        Private numElapsed As Integer
        Protected vibRate As Double
        Protected vibDepth As Double
        Private g_time As Double
        Protected sampleRate As Single

        Protected settings As AutoTuneSettings

        Public Sub New(ByVal settings As AutoTuneSettings, ByVal sampleRate As Single)
            Me.settings = settings
            Me.sampleRate = sampleRate
            numshifts = 5000
            shifts = New Queue(Of PitchShift)(numshifts)

            currPitch = 0
            attack = 0
            numElapsed = 0
            vibRate = 4.0
            vibDepth = 0.00
            g_time = 0.0
        End Sub

        Protected Function snapFactor(ByVal freq As Single) As Single
            Dim previousFrequency = 0.0F
            Dim correctedFrequency = 0.0F
            Dim previousNote = 0
            Dim correctedNote = 0
            For i = 1 To 119
                Dim endLoop = False
                For Each note As Integer In Me.settings.AutoPitches
                    If i Mod 12 = note Then
                        previousFrequency = correctedFrequency
                        previousNote = correctedNote
                        correctedFrequency = CSng(8.175 * Math.Pow(1.05946309, CSng(i)))
                        correctedNote = i
                        If correctedFrequency > freq Then
                            endLoop = True
                        End If
                        Exit For
                    End If
                Next note
                If endLoop Then
                    Exit For
                End If
            Next i
            If correctedFrequency = 0.0 Then
                Return 1.0f
            End If
            Dim destinationNote = 0
            Dim destinationFrequency = 0.0
            ' decide whether we are shifting up or down
            If correctedFrequency - freq > freq - previousFrequency Then
                destinationNote = previousNote
                destinationFrequency = previousFrequency
            Else
                destinationNote = correctedNote
                destinationFrequency = correctedFrequency
            End If
            If destinationNote <> currPitch Then
                numElapsed = 0
                currPitch = destinationNote
            End If
            If attack > numElapsed Then
                Dim n = (destinationFrequency - freq) / attack * numElapsed
                destinationFrequency = freq + n
            End If
            numElapsed += 1
            Return CSng(destinationFrequency / freq)
        End Function

        Protected Sub updateShifts(ByVal detected As Single, ByVal shifted As Single, ByVal targetNote As Integer)
            If shifts.Count >= numshifts Then
                shifts.Dequeue()
            End If
            Dim shift As New PitchShift(detected, shifted, targetNote)
            Debug.WriteLine(shift)
            shifts.Enqueue(shift)
        End Sub

        Private Sub setDetectedNote(ByVal pitch As Single)
            For i = 0 To 119
                Dim d = CSng(8.175 * Math.Pow(1.05946309, CSng(i)) - pitch)
                If -1.0 < d AndAlso d < 1.0 Then
                    detectedNote = i
                    Return
                End If
            Next i
            detectedNote = -1
        End Sub

        Private Function isDetectedNote(ByVal note As Integer) As Boolean
            Return (note Mod 12) = (detectedNote Mod 12) AndAlso detectedNote >= 0
        End Function

        Protected Function addVibrato(ByVal nFrames As Integer) As Single
            g_time += nFrames
            Dim d = CSng(Math.Sin(2 * 3.14159265358979 * vibRate * g_time / sampleRate) * vibDepth)
            Return d
        End Function
    End Class


    Friend Class SmbPitchShifter
        Inherits PitchShifter
        Public Sub New(ByVal settings As AutoTuneSettings, ByVal sampleRate As Single)
            MyBase.New(settings, sampleRate)
        End Sub

        Public Sub ShiftPitch(ByVal inputBuff() As Single, ByVal inputPitch As Single,
                              ByVal targetPitch As Single, ByVal outputBuff() As Single, ByVal nFrames As Integer)
            UpdateSettings()
            detectedPitch = inputPitch
            Dim shiftFactor = 1.0F
            If Me.settings.SnapMode Then
                If inputPitch > 0 Then
                    shiftFactor = snapFactor(inputPitch)
                    shiftFactor += addVibrato(nFrames)
                End If
            Else
                Dim midiPitch = targetPitch
                shiftFactor = 1.0F
                If inputPitch > 0 AndAlso midiPitch > 0 Then
                    shiftFactor = midiPitch / inputPitch
                End If
            End If

            If shiftFactor > 2.0 Then
                shiftFactor = 2.0F
            End If
            If shiftFactor < 0.5 Then
                shiftFactor = 0.5F
            End If

            ' fftFrameSize was nFrames but can't guarantee it is a power of 2
            ' 2048 works, let's try 1024
            Dim fftFrameSize = 2048
            Dim osamp = 8 ' 32 is best quality
            SmbPitchShift.smbPitchShift(shiftFactor, nFrames, fftFrameSize, osamp, Me.sampleRate, inputBuff, outputBuff)

            'vibrato
            'addVibrato(outputBuff, nFrames);

            shiftedPitch = inputPitch * shiftFactor
            updateShifts(detectedPitch, shiftedPitch, Me.currPitch)
        End Sub

        Private Sub UpdateSettings()
            'these are going here, because this gets called once per frame
            vibRate = Me.settings.VibratoRate
            vibDepth = Me.settings.VibratoDepth
            attack = CInt(Fix((Me.settings.AttackTimeMilliseconds * 441) / 1024.0))
        End Sub
    End Class


    Friend Class PitchShift
        Public Sub New(ByVal detected As Single, ByVal shifted As Single, ByVal destNote As Integer)
            Me.DetectedPitch = detected
            Me.ShiftedPitch = shifted
            Me.DestinationNote = destNote
        End Sub

        Private _detectedPitch As Single
        Public Property DetectedPitch As Single
            Get
                Return _detectedPitch
            End Get
            Private Set(ByVal value As Single)
                _detectedPitch = value
            End Set
        End Property

        Private _shiftedPitch As Single
        Public Property ShiftedPitch As Single
            Get
                Return _shiftedPitch
            End Get
            Private Set(ByVal value As Single)
                _shiftedPitch = value
            End Set
        End Property

        Private _destinationNote As Integer
        Public Property DestinationNote As Integer
            Get
                Return _destinationNote
            End Get
            Private Set(ByVal value As Integer)
                _destinationNote = value
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("detected {0:f2}Hz, shifted to {1:f2}Hz, {2}{3} ",
                                 DetectedPitch, ShiftedPitch, CType(DestinationNote Mod 12, Note), DestinationNote \ 12)
        End Function
    End Class
End Namespace
