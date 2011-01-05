Imports System.Text
Imports System.IO
Imports Microsoft.Win32
Imports NAudio.Wave

Namespace VoiceRecorder.Audio
    Public Enum SaveFileFormat
        Wav
        Mp3
    End Enum


    Public Class AudioSaver
        Private inputFile As String

        Public Property TrimFromStart As TimeSpan
        Public Property TrimFromEnd As TimeSpan
        Public Property AutoTuneSettings As AutoTuneSettings
        Public Property SaveFileFormat As SaveFileFormat
        Public Property LameExePath As String

        Public Sub New(ByVal inputFile As String)
            Me.inputFile = inputFile
            Me.AutoTuneSettings = New AutoTuneSettings ' default settings
        End Sub

        Public ReadOnly Property IsTrimNeeded As Boolean
            Get
                Return TrimFromStart <> TimeSpan.Zero OrElse TrimFromEnd <> TimeSpan.Zero
            End Get
        End Property

        Public Sub SaveAudio(ByVal outputFile As String)
            Dim tempFiles As New List(Of String)
            Dim fileToProcess = inputFile
            If IsTrimNeeded Then
                Dim tempFile = WavFileUtils.GetTempWavFileName()
                tempFiles.Add(tempFile)
                WavFileUtils.TrimWavFile(inputFile, tempFile, TrimFromStart, TrimFromEnd)
                fileToProcess = tempFile
            End If
            If AutoTuneSettings.Enabled Then
                Dim tempFile = WavFileUtils.GetTempWavFileName()
                tempFiles.Add(tempFile)
                AutoTuneUtils.ApplyAutoTune(fileToProcess, tempFile, AutoTuneSettings)
                fileToProcess = tempFile
            End If
            If SaveFileFormat = SaveFileFormat.Mp3 Then
                ConvertToMp3(Me.LameExePath, fileToProcess, outputFile)
            Else
                File.Copy(fileToProcess, outputFile)
            End If
            DeleteTempFiles(tempFiles)
        End Sub

        Private Sub DeleteTempFiles(ByVal tempFiles As IEnumerable(Of String))
            For Each tempFile In tempFiles
                If File.Exists(tempFile) Then
                    File.Delete(tempFile)
                End If
            Next tempFile
        End Sub

        Public Shared Sub ConvertToMp3(ByVal lameExePath As String, ByVal waveFile As String, ByVal mp3File As String)
            Dim converter = Process.Start(lameExePath, "-V2 """ & waveFile & """ """ & mp3File & """")
            converter.WaitForExit()
        End Sub
    End Class
End Namespace
