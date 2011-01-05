Imports System.Text
Imports System.Collections.ObjectModel
Imports NAudio.Wave
Imports VoiceRecorder.Core
Imports GalaSoft.MvvmLight.Command
Imports GalaSoft.MvvmLight.Messaging
Imports GalaSoft.MvvmLight

Namespace VoiceRecorder
    Friend Class WelcomeViewModel
        Inherits ViewModelBase
        Implements IView
        Private _recordingDevices As ObservableCollection(Of String)
        Private selectedRecordingDeviceIndex As Integer
        Private _continueCommand As ICommand
        Public Const ViewName = "WelcomeView"

        Public Sub New()
            Me._recordingDevices = New ObservableCollection(Of String)
            Me._continueCommand = New RelayCommand(Sub() MoveToRecorder())
        End Sub

        Public ReadOnly Property ContinueCommand As ICommand
            Get
                Return _continueCommand
            End Get
        End Property

        Public Sub Activated(ByVal state As Object) Implements IView.Activated
            Me._recordingDevices.Clear()
            For n = 0 To WaveIn.DeviceCount - 1
                Me._recordingDevices.Add(WaveIn.GetCapabilities(n).ProductName)
            Next n
        End Sub

        Private Sub MoveToRecorder()
            Messenger.Default.Send(New NavigateMessage(RecorderViewModel.ViewName, SelectedIndex))
        End Sub

        Public ReadOnly Property RecordingDevices As ObservableCollection(Of String)
            Get
                Return _recordingDevices
            End Get
        End Property

        Public Property SelectedIndex As Integer
            Get
                Return selectedRecordingDeviceIndex
            End Get
            Set(ByVal value As Integer)
                If selectedRecordingDeviceIndex <> value Then
                    selectedRecordingDeviceIndex = value
                    RaisePropertyChanged("SelectedIndex")
                End If
            End Set
        End Property
    End Class
End Namespace
