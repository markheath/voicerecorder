' this class based on code from awesomebox, a project created by by Ravi Parikh and Keegan Poppen, used with permission
' http://decabear.com/awesomebox.html
Imports NAudio.Wave

Namespace VoiceRecorder.Audio
    Public Class AutoTuneWaveProvider
        Implements IWaveProvider
        Private source As IWaveProvider
        Private pitchShifter As SmbPitchShifter
        Private pitchDetector As IPitchDetector
        Private waveBuffer As WaveBuffer
        Private autoTuneSettings As AutoTuneSettings

        Public Sub New(ByVal source As IWaveProvider)
            Me.New(source, New AutoTuneSettings)
        End Sub

        Public Sub New(ByVal source As IWaveProvider, ByVal autoTuneSettings As AutoTuneSettings)
            Me.autoTuneSettings = autoTuneSettings
            If source.WaveFormat.SampleRate <> 44100 Then
                Throw New ArgumentException("AutoTune only works at 44.1kHz")
            End If
            If source.WaveFormat.Encoding <> WaveFormatEncoding.IeeeFloat Then
                Throw New ArgumentException("AutoTune only works on IEEE floating point audio data")
            End If
            If source.WaveFormat.Channels <> 1 Then
                Throw New ArgumentException("AutoTune only works on mono input sources")
            End If

            Me.source = source
            Me.pitchDetector = New AutoCorrelator(source.WaveFormat.SampleRate)
            ' alternative pitch detector:
            ' Me.pitchDetector = New FftPitchDetector(source.WaveFormat.SampleRate)
            Me.pitchShifter = New SmbPitchShifter(Settings, source.WaveFormat.SampleRate)
            Me.waveBuffer = New WaveBuffer(8192)
        End Sub

        Public ReadOnly Property Settings As AutoTuneSettings
            Get
                Return Me.autoTuneSettings
            End Get
        End Property

        Private previousPitch As Single
        Private release As Integer
        Private maxHold As Integer = 1

        Public Function Read(ByVal buffer() As Byte, ByVal offset As Integer,
                             ByVal count As Integer) As Integer Implements NAudio.Wave.IWaveProvider.Read
            If waveBuffer Is Nothing OrElse waveBuffer.MaxSize < count Then
                waveBuffer = New WaveBuffer(count)
            End If

            Dim bytesRead = source.Read(waveBuffer, 0, count)
            'Debug.Assert(bytesRead = count)

            ' the last bit sometimes needs to be rounded up:
            If bytesRead > 0 Then
                bytesRead = count
            End If

            'pitchsource->getPitches()
            Dim frames = bytesRead \ Len(New Single) ' MRH: was count
            Dim pitch = pitchDetector.DetectPitch(waveBuffer.FloatBuffer, frames)

            ' MRH: an attempt to make it less "warbly" by holding onto the pitch for at least one more buffer
            If pitch = 0.0F AndAlso release < maxHold Then
                pitch = previousPitch
                release += 1
            Else
                Me.previousPitch = pitch
                release = 0
            End If

            Dim midiNoteNumber = 40
            Dim targetPitch = CSng(8.175 * Math.Pow(1.05946309, midiNoteNumber))

            Dim outBuffer As New WaveBuffer(buffer)

            pitchShifter.ShiftPitch(waveBuffer.FloatBuffer, pitch, targetPitch, outBuffer.FloatBuffer, frames)

            Return frames * 4
        End Function

        Public ReadOnly Property WaveFormat As WaveFormat Implements NAudio.Wave.IWaveProvider.WaveFormat
            Get
                Return source.WaveFormat
            End Get
        End Property
    End Class
End Namespace
