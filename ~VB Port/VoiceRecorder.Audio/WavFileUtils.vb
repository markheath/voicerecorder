Imports System.Text
Imports NAudio.Wave
Imports System.IO

Namespace VoiceRecorder.Audio
    Public NotInheritable Class WavFileUtils
        Private Sub New()
        End Sub

        Public Shared Sub TrimWavFile(ByVal inPath As String, ByVal outPath As String,
                                      ByVal cutFromStart As TimeSpan, ByVal cutFromEnd As TimeSpan)
            Using reader As New WaveFileReader(inPath)
                Using writer As New WaveFileWriter(outPath, reader.WaveFormat)
                    Dim bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond \ 1000

                    Dim startPos = CInt(Fix(cutFromStart.TotalMilliseconds)) * bytesPerMillisecond
                    startPos = startPos - startPos Mod reader.WaveFormat.BlockAlign

                    Dim endBytes = CInt(Fix(cutFromEnd.TotalMilliseconds)) * bytesPerMillisecond
                    endBytes = endBytes - endBytes Mod reader.WaveFormat.BlockAlign
                    Dim endPos = CInt(Fix(reader.Length)) - endBytes

                    TrimWavFile(reader, writer, startPos, endPos)
                End Using
            End Using
        End Sub

        Public Shared Function GetTempWavFileName() As String
            Return Path.Combine(Path.GetTempPath(), New Guid().ToString() & ".wav")
        End Function

        Private Shared Sub TrimWavFile(ByVal reader As WaveFileReader, ByVal writer As WaveFileWriter,
                                       ByVal startPos As Integer, ByVal endPos As Integer)
            reader.Position = startPos
            Dim buffer(1023) As Byte
            Do While reader.Position < endPos
                Dim bytesRequired = CInt(Fix(endPos - reader.Position))
                If bytesRequired > 0 Then
                    Dim bytesToRead = Math.Min(bytesRequired, buffer.Length)
                    Dim bytesRead = reader.Read(buffer, 0, bytesToRead)
                    If bytesRead > 0 Then
                        writer.WriteData(buffer, 0, bytesRead)
                    End If
                End If
            Loop
        End Sub
    End Class
End Namespace
