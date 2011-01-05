Imports System.Text
Imports NAudio.Wave

Namespace VoiceRecorder.Audio
    Public Class TrimWaveStream
        Inherits WaveStream
        Private source As WaveStream
        Private startBytePosition As Long
        Private endBytePosition As Long
        Private _startPosition As TimeSpan
        Private _endPosition As TimeSpan

        Public Sub New(ByVal source As WaveStream)
            Me.source = source
            Me.EndPosition = source.TotalTime
        End Sub

        Public Property StartPosition As TimeSpan
            Get
                Return _startPosition
            End Get
            Set(ByVal value As TimeSpan)
                _startPosition = value
                startBytePosition = CInt(Fix(WaveFormat.AverageBytesPerSecond * _startPosition.TotalSeconds))
                startBytePosition = startBytePosition - (startBytePosition Mod WaveFormat.BlockAlign)
                Position = 0
            End Set
        End Property

        Public Property EndPosition As TimeSpan
            Get
                Return _endPosition
            End Get
            Set(ByVal value As TimeSpan)
                _endPosition = value
                _endPosition = value
                endBytePosition = CInt(Fix(Math.Round(WaveFormat.AverageBytesPerSecond * _endPosition.TotalSeconds)))
                endBytePosition = endBytePosition - (endBytePosition Mod WaveFormat.BlockAlign)
            End Set
        End Property

        Public Overrides ReadOnly Property WaveFormat As WaveFormat
            Get
                Return source.WaveFormat
            End Get
        End Property

        Public Overrides ReadOnly Property Length As Long
            Get
                Return endBytePosition - startBytePosition
            End Get
        End Property

        Public Overrides Property Position As Long
            Get
                Return source.Position - startBytePosition
            End Get
            Set(ByVal value As Long)
                source.Position = value + startBytePosition
            End Set
        End Property

        Public Overrides Function Read(ByVal buffer() As Byte, ByVal offset As Integer, ByVal count As Integer) As Integer
            Dim bytesRequired = CInt(Fix(Math.Min(count, Length - Position)))
            Dim bytesRead = 0
            If bytesRequired > 0 Then
                bytesRead = source.Read(buffer, offset, bytesRequired)
            End If
            Return bytesRead
        End Function

        Protected Overrides Overloads Sub Dispose(ByVal disposing As Boolean)
            source.Dispose()
            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace
