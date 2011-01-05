Imports System.Text
Imports System.Windows.Threading
Imports VoiceRecorder.Core
Imports System.IO
Imports VoiceRecorder.Audio
Imports GalaSoft.MvvmLight.Messaging
Imports GalaSoft.MvvmLight.Command
Imports GalaSoft.MvvmLight

Namespace VoiceRecorder
    Friend Class RecorderViewModel
        Inherits ViewModelBase
        Implements IView
        Private _beginRecordingCommand As RelayCommand
        Private _stopCommand As RelayCommand
        Private recorder As IAudioRecorder
        Private lastPeak As Single
        Private waveFileName As String
        Public Const ViewName = "RecorderView"

        Public Sub New(ByVal _recorder As IAudioRecorder)
            Me.recorder = _recorder
            AddHandler recorder.Stopped, AddressOf recorder_Stopped
            Me._beginRecordingCommand =
                New RelayCommand(Sub() BeginRecording(), Function() _recorder.RecordingState = RecordingState.Stopped OrElse
                                                             _recorder.RecordingState = RecordingState.Monitoring)
            Me._stopCommand =
                New RelayCommand(Sub() Me.Stop(), Function() _recorder.RecordingState = RecordingState.Recording)
            AddHandler _recorder.SampleAggregator.MaximumCalculated, AddressOf recorder_MaximumCalculated
            Messenger.Default.Register(Of ShuttingDownMessage)(Me, Sub(message) OnShuttingDown(message))
        End Sub

        Private Sub recorder_Stopped(ByVal sender As Object, ByVal e As EventArgs)
            Messenger.Default.Send(New NavigateMessage(SaveViewModel.ViewName, New VoiceRecorderState(waveFileName, Nothing)))
        End Sub

        Private Sub recorder_MaximumCalculated(ByVal sender As Object, ByVal e As MaxSampleEventArgs)
            lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample))
            RaisePropertyChanged("CurrentInputLevel")
            RaisePropertyChanged("RecordedTime")
        End Sub

        Public ReadOnly Property BeginRecordingCommand As ICommand
            Get
                Return _beginRecordingCommand
            End Get
        End Property

        Public ReadOnly Property StopCommand As ICommand
            Get
                Return _stopCommand
            End Get
        End Property

        Public Sub Activated(ByVal state As Object) Implements IView.Activated
            BeginMonitoring(CInt(Fix(state)))
        End Sub

        Private Sub OnShuttingDown(ByVal message As ShuttingDownMessage)
            If message.CurrentViewName = RecorderViewModel.ViewName Then
                recorder.Stop()
            End If
        End Sub

        Public ReadOnly Property RecordedTime As String
            Get
                Dim current = recorder.RecordedTime
                Return String.Format("{0:D2}:{1:D2}.{2:D3}", current.Minutes, current.Seconds, current.Milliseconds)
            End Get
        End Property

        Private Sub BeginMonitoring(ByVal recordingDevice As Integer)
            recorder.BeginMonitoring(recordingDevice)
            RaisePropertyChanged("MicrophoneLevel")
        End Sub

        Private Sub BeginRecording()
            Me.waveFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() & ".wav")
            recorder.BeginRecording(waveFileName)
            RaisePropertyChanged("MicrophoneLevel")
            RaisePropertyChanged("ShowWaveForm")
        End Sub

        Private Sub [Stop]()
            recorder.Stop()
        End Sub

        Public Property MicrophoneLevel As Double
            Get
                Return recorder.MicrophoneLevel
            End Get
            Set(ByVal value As Double)
                recorder.MicrophoneLevel = value
            End Set
        End Property

        Public ReadOnly Property ShowWaveForm As Boolean
            Get
                Return recorder.RecordingState = RecordingState.Recording OrElse
                    recorder.RecordingState = RecordingState.RequestedStop
            End Get
        End Property

        ' multiply by 100 because the Progress bar's default maximum value is 100
        Public ReadOnly Property CurrentInputLevel As Single
            Get
                Return lastPeak * 100
            End Get
        End Property

        Public ReadOnly Property SampleAggregator As SampleAggregator
            Get
                Return recorder.SampleAggregator
            End Get
        End Property
    End Class
End Namespace