Imports NAudio.Wave

Namespace VoiceRecorder.Audio
    Public Class AutoTuneUtils
        Public Shared Sub ApplyAutoTune(ByVal fileToProcess As String,
                                        ByVal tempFile As String,
                                        ByVal autotuneSettings As AutoTuneSettings)
            Using reader As New WaveFileReader(fileToProcess)
                Dim stream32 As IWaveProvider = New Wave16ToFloatProvider(reader)
                Dim streamEffect As IWaveProvider = New AutoTuneWaveProvider(stream32, autotuneSettings)
                Dim stream16 As IWaveProvider = New WaveFloatTo16Provider(streamEffect)
                Using converted As New WaveFileWriter(tempFile, stream16.WaveFormat)
                    ' buffer length needs to be a power of 2 for FFT to work nicely
                    ' however, make the buffer too long and pitches aren't detected fast enough
                    ' successful buffer sizes: 8192, 4096, 2048, 1024
                    ' (some pitch detection algorithms need at least 2048)
                    Dim buffer(8191) As Byte
                    Dim bytesRead As Integer
                    Do
                        bytesRead = stream16.Read(buffer, 0, buffer.Length)
                        converted.WriteData(buffer, 0, bytesRead)
                    Loop While bytesRead <> 0 AndAlso converted.Length < reader.Length
                End Using
            End Using
        End Sub
    End Class
End Namespace
