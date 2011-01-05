Imports System.Text
Imports VoiceRecorder.Core
Imports NAudio.Wave
Imports Microsoft.Win32
Imports System.IO
Imports VoiceRecorder.Audio
Imports GalaSoft.MvvmLight.Command
Imports GalaSoft.MvvmLight
Imports GalaSoft.MvvmLight.Messaging
Imports My

Namespace VoiceRecorder
    Friend Class SaveViewModel
        Inherits ViewModelBase
        Implements IView
        Private voiceRecorderState As VoiceRecorderState
        Private _sampleAggregator As SampleAggregator
        Private _leftPosition As Integer
        Private _rightPosition As Integer
        Private _totalWaveFormSamples As Integer
        Private audioPlayer As IAudioPlayer
        Private samplesPerSecond As Integer
        Public Const ViewName = "SaveView"

        Public Sub New(ByVal audioPlayer As IAudioPlayer)
            Me.SampleAggregator = New SampleAggregator
            SampleAggregator.NotificationCount = 800 ' gets set correctly later on
            Me.audioPlayer = audioPlayer
            Me.SaveCommand = New RelayCommand(Sub() Save())
            Me.SelectAllCommand = New RelayCommand(Sub() SelectAll())
            Me.PlayCommand = New RelayCommand(Sub() Play())
            Me.StopCommand = New RelayCommand(Sub() Me.Stop())
            Me.AutoTuneCommand = New RelayCommand(Sub() OnAutoTune())
            Messenger.Default.Register(Of ShuttingDownMessage)(Me, Sub(message) OnShuttingDown(message))
        End Sub

        Private Sub OnAutoTune()
            audioPlayer.Dispose() ' needed to relinquish the file as it may get deleted
            Messenger.Default.Send(New NavigateMessage("AutoTuneView", Me.voiceRecorderState))
        End Sub

        Private _saveCommand As ICommand
        Public Property SaveCommand As ICommand
            Get
                Return _saveCommand
            End Get
            Private Set(ByVal value As ICommand)
                _saveCommand = value
            End Set
        End Property

        Private _selectAllCommand As ICommand
        Public Property SelectAllCommand As ICommand
            Get
                Return _selectAllCommand
            End Get
            Private Set(ByVal value As ICommand)
                _selectAllCommand = value
            End Set
        End Property

        Private _playCommand As ICommand
        Public Property PlayCommand As ICommand
            Get
                Return _playCommand
            End Get
            Private Set(ByVal value As ICommand)
                _playCommand = value
            End Set
        End Property

        Private _stopCommand As ICommand
        Public Property StopCommand As ICommand
            Get
                Return _stopCommand
            End Get
            Private Set(ByVal value As ICommand)
                _stopCommand = value
            End Set
        End Property

        Private _autoTuneCommand As ICommand
        Public Property AutoTuneCommand As ICommand
            Get
                Return _autoTuneCommand
            End Get
            Private Set(ByVal value As ICommand)
                _autoTuneCommand = value
            End Set
        End Property

        Public Sub Activated(ByVal state As Object) Implements IView.Activated
            Me.voiceRecorderState = CType(state, VoiceRecorderState)
            RenderFile()
        End Sub

        Private Sub OnShuttingDown(ByVal message As ShuttingDownMessage)
            audioPlayer.Dispose()

            If message.CurrentViewName = SaveViewModel.ViewName Then
                Me.voiceRecorderState.DeleteFiles()
            End If
        End Sub

        Private Sub Save()
            Dim saveFileDialog As New SaveFileDialog
            saveFileDialog.Filter = "WAV file (.wav)|*.wav|MP3 file (.mp3)|.mp3"
            saveFileDialog.DefaultExt = ".wav"
            Dim result? As Boolean = saveFileDialog.ShowDialog()
            If result.HasValue AndAlso result.Value Then
                SaveAs(saveFileDialog.FileName)
            End If
        End Sub

        Private Function PositionToTimeSpan(ByVal position As Integer) As TimeSpan
            Dim samples = SampleAggregator.NotificationCount * position
            Return TimeSpan.FromSeconds(CDbl(samples) / samplesPerSecond)
        End Function

        Private Sub SaveAs(ByVal fileName As String)
            Dim saver As New AudioSaver(voiceRecorderState.ActiveFile)
            saver.TrimFromStart = PositionToTimeSpan(LeftPosition)
            saver.TrimFromEnd = PositionToTimeSpan(TotalWaveFormSamples - RightPosition)

            If fileName.ToLower().EndsWith(".wav") Then
                saver.SaveFileFormat = SaveFileFormat.Wav
                saver.SaveAudio(fileName)
            ElseIf fileName.ToLower().EndsWith(".mp3") Then
                Dim lameExePath = LocateLame()
                If lameExePath IsNot Nothing Then
                    saver.SaveFileFormat = SaveFileFormat.Mp3
                    saver.LameExePath = lameExePath
                    saver.SaveAudio(fileName)
                End If
            Else
                MessageBox.Show("Please select a supported output format")
            End If
        End Sub

        Public Property LeftPosition As Integer
            Get
                Return _leftPosition
            End Get
            Set(ByVal value As Integer)
                If _leftPosition <> value Then
                    _leftPosition = value
                    RaisePropertyChanged("LeftPosition")
                End If
            End Set
        End Property

        Public Property RightPosition As Integer
            Get
                Return _rightPosition
            End Get
            Set(ByVal value As Integer)
                If _rightPosition <> value Then
                    _rightPosition = value
                    RaisePropertyChanged("RightPosition")
                End If
            End Set
        End Property

        Public Function LocateLame() As String
            Dim lameExePath = Settings.Default.LameExePath

            If String.IsNullOrEmpty(lameExePath) OrElse (Not File.Exists(lameExePath)) Then
                If MessageBox.Show("To save as MP3 requires LAME.exe, please locate",
                                   "Save as MP3", MessageBoxButton.OKCancel) = MessageBoxResult.OK Then
                    Dim ofd As New OpenFileDialog
                    ofd.FileName = "lame.exe"
                    Dim result? = ofd.ShowDialog()
                    If result IsNot Nothing AndAlso result.HasValue Then
                        If File.Exists(ofd.FileName) AndAlso ofd.FileName.ToLower().EndsWith("lame.exe") Then
                            Settings.Default.LameExePath = ofd.FileName
                            Settings.Default.Save()
                            Return ofd.FileName
                        End If
                    Else
                        Return Nothing
                    End If
                Else
                    Return Nothing
                End If
            End If
            Return lameExePath
        End Function

        Private Sub RenderFile()
            SampleAggregator.RaiseRestart()
            Using reader As New WaveFileReader(Me.voiceRecorderState.ActiveFile)
                Me.samplesPerSecond = reader.WaveFormat.SampleRate
                SampleAggregator.NotificationCount = reader.WaveFormat.SampleRate \ 10

                Dim buffer(1023) As Byte
                Dim waveBuffer As New WaveBuffer(buffer)
                waveBuffer.ByteBufferCount = buffer.Length
                Dim bytesRead As Integer
                Do
                    bytesRead = reader.Read(waveBuffer, 0, buffer.Length)
                    Dim samples = bytesRead \ 2
                    For sample = 0 To samples - 1
                        If bytesRead > 0 Then
                            _sampleAggregator.Add(waveBuffer.ShortBuffer(sample) / 32768.0F)
                        End If
                    Next sample
                Loop While bytesRead > 0
                Dim totalSamples = CInt(Fix(reader.Length)) \ 2
                TotalWaveFormSamples = totalSamples \ _sampleAggregator.NotificationCount
                SelectAll()
            End Using
            audioPlayer.LoadFile(Me.voiceRecorderState.ActiveFile)
        End Sub

        Private Sub Play()
            audioPlayer.StartPosition = PositionToTimeSpan(LeftPosition)
            audioPlayer.EndPosition = PositionToTimeSpan(RightPosition)
            audioPlayer.Play()
        End Sub

        Private Sub [Stop]()
            audioPlayer.Stop()
        End Sub

        Private Sub SelectAll()
            LeftPosition = 0
            RightPosition = TotalWaveFormSamples
        End Sub

        Public Property SampleAggregator As SampleAggregator
            Get
                Return _sampleAggregator
            End Get
            Set(ByVal value As SampleAggregator)
                If _sampleAggregator IsNot value Then
                    _sampleAggregator = value
                    RaisePropertyChanged("SampleAggregator")
                End If
            End Set
        End Property

        Public Property TotalWaveFormSamples As Integer
            Get
                Return _totalWaveFormSamples
            End Get
            Set(ByVal value As Integer)
                If _totalWaveFormSamples <> value Then
                    _totalWaveFormSamples = value
                    RaisePropertyChanged("TotalWaveFormSamples")
                End If
            End Set
        End Property
    End Class
End Namespace
