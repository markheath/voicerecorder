Imports System.Text
Imports VoiceRecorder.Core
Imports System.Collections.ObjectModel
Imports VoiceRecorder.Audio
Imports System.IO
Imports GalaSoft.MvvmLight.Command
Imports GalaSoft.MvvmLight
Imports GalaSoft.MvvmLight.Messaging
Imports System.Threading
Imports GalaSoft.MvvmLight.Threading
Imports System.Linq

Namespace VoiceRecorder
    Friend Class AutoTuneViewModel
        Inherits ViewModelBase
        Implements IView
        Private _applyCommand As ICommand
        Public Property ApplyCommand As ICommand
            Get
                Return _applyCommand
            End Get
            Private Set(ByVal value As ICommand)
                _applyCommand = value
            End Set
        End Property

        Private _cancelCommand As ICommand
        Public Property CancelCommand As ICommand
            Get
                Return _cancelCommand
            End Get
            Private Set(ByVal value As ICommand)
                _cancelCommand = value
            End Set
        End Property

        Private attackTimeMilliseconds As Integer
        Private _isAutoTuneEnabled As Boolean
        Private voiceRecorderState As VoiceRecorderState
        Public Const ViewName = "AutoTuneView"
        Private _isEnabled As Boolean

        Public Property AttackTime As Integer
            Get
                Return attackTimeMilliseconds
            End Get
            Set(ByVal value As Integer)
                If attackTimeMilliseconds <> value Then
                    attackTimeMilliseconds = value
                    RaisePropertyChanged("AttackTime")
                    RaisePropertyChanged("AttackMessage")
                End If
            End Set
        End Property

        Public Property IsEnabled As Boolean
            Get
                Return _isEnabled
            End Get
            Set(ByVal value As Boolean)
                If _isEnabled <> value Then
                    _isEnabled = value
                    RaisePropertyChanged("IsEnabled")
                    RaisePropertyChanged("ProcessingMessageVisibility")
                End If
            End Set
        End Property

        Public ReadOnly Property ProcessingMessageVisibility As Visibility
            Get
                Return If(_isEnabled, Visibility.Collapsed, Visibility.Visible)
            End Get
        End Property

        Public ReadOnly Property AttackMessage As String
            Get
                Return String.Format("{0}ms", attackTimeMilliseconds)
            End Get
        End Property

        Public Property IsAutoTuneEnabled As Boolean
            Get
                Return _isAutoTuneEnabled
            End Get
            Set(ByVal value As Boolean)
                If _isAutoTuneEnabled <> value Then
                    _isAutoTuneEnabled = value
                    RaisePropertyChanged("IsAutoTuneEnabled")
                End If
            End Set
        End Property

        Private _pitches As ObservableCollection(Of NoteViewModel)
        Public Property Pitches As ObservableCollection(Of NoteViewModel)
            Get
                Return _pitches
            End Get
            Private Set(ByVal value As ObservableCollection(Of NoteViewModel))
                _pitches = value
            End Set
        End Property

        Public ReadOnly Property Scales As IEnumerable(Of String)
            Get
                Return scalesDictionary.Keys
            End Get
        End Property

        Private _selectedScale As String
        Public Property SelectedScale As String
            Get
                Return _selectedScale
            End Get
            Set(ByVal value As String)
                If Me._selectedScale <> value Then
                    Me._selectedScale = value
                    SelectNotes()
                End If
            End Set
        End Property

        Private scalesDictionary As Dictionary(Of String, HashSet(Of Note))

        Private Sub SelectNotes()
            Dim scale = scalesDictionary(_selectedScale)
            For Each p In Pitches
                p.Selected = scale.Contains(p.Note)
            Next p
        End Sub

        Public Sub New()
            Me.ApplyCommand = New RelayCommand(Sub() Apply())
            Me.CancelCommand = New RelayCommand(Sub() Cancel())
            scalesDictionary = New Dictionary(Of String, HashSet(Of Note))
            scalesDictionary.Add("Chromatic", PredefinedScales.Chromatic)
            scalesDictionary.Add("Key of C / Am", PredefinedScales.MakeMajorScale(Note.C))
            scalesDictionary.Add("Key of D / Bm", PredefinedScales.MakeMajorScale(Note.D))
            scalesDictionary.Add("Key of E / C" & ChrW(&H266F).ToString() & "m",
                                 PredefinedScales.MakeMajorScale(Note.E))
            scalesDictionary.Add("Key of F / Dm", PredefinedScales.MakeMajorScale(Note.F))
            scalesDictionary.Add("Key of G / Em", PredefinedScales.MakeMajorScale(Note.G))
            scalesDictionary.Add("Key of A / F" & ChrW(&H266F).ToString() & "m",
                                 PredefinedScales.MakeMajorScale(Note.A))
            scalesDictionary.Add("Key of B" & ChrW(&H266D).ToString() & " / Gm",
                                 PredefinedScales.MakeMajorScale(Note.ASharp))

            scalesDictionary.Add("Pentatonic C / Am", PredefinedScales.MakePentatonicScale(Note.C))
            scalesDictionary.Add("Pentatonic D / Bm", PredefinedScales.MakePentatonicScale(Note.D))
            scalesDictionary.Add("Pentatonic E / C" & ChrW(&H266F).ToString() & "m",
                                 PredefinedScales.MakePentatonicScale(Note.E))
            scalesDictionary.Add("Pentatonic F / Dm", PredefinedScales.MakePentatonicScale(Note.F))
            scalesDictionary.Add("Pentatonic G / Em", PredefinedScales.MakePentatonicScale(Note.G))
            scalesDictionary.Add("Pentatonic A / F" & ChrW(&H266F).ToString() & "m",
                                 PredefinedScales.MakePentatonicScale(Note.A))
            scalesDictionary.Add("Pentatonic B" & ChrW(&H266D).ToString() & " / Gm",
                                 PredefinedScales.MakePentatonicScale(Note.ASharp))


            Me.Pitches = New ObservableCollection(Of NoteViewModel)

            Me.Pitches.Add(New NoteViewModel(Note.C, "C"))
            Me.Pitches.Add(New NoteViewModel(Note.CSharp, "C" & ChrW(&H266F).ToString()))
            Me.Pitches.Add(New NoteViewModel(Note.D, "D"))
            Me.Pitches.Add(New NoteViewModel(Note.DSharp, "E" & ChrW(&H266D).ToString()))
            Me.Pitches.Add(New NoteViewModel(Note.E, "E"))
            Me.Pitches.Add(New NoteViewModel(Note.F, "F"))
            Me.Pitches.Add(New NoteViewModel(Note.FSharp, "F" & ChrW(&H266F).ToString()))
            Me.Pitches.Add(New NoteViewModel(Note.G, "G"))
            Me.Pitches.Add(New NoteViewModel(Note.GSharp, "A" & ChrW(&H266D).ToString()))
            Me.Pitches.Add(New NoteViewModel(Note.A, "A"))
            Me.Pitches.Add(New NoteViewModel(Note.ASharp, "B" & ChrW(&H266D).ToString()))
            Me.Pitches.Add(New NoteViewModel(Note.B, "B"))
            Me.SelectedScale = "Chromatic"
            Messenger.Default.Register(Of ShuttingDownMessage)(Me, Sub(message) OnShuttingDown(message))
        End Sub

        Private Sub Apply()
            UpdateAutoTuneSettingsFromGui()
            If voiceRecorderState.AutoTuneSettings.Enabled Then
                Dim tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() & ".wav")
                IsEnabled = False
                ThreadPool.QueueUserWorkItem(Sub(state) SaveAs(tempPath))
            Else
                NavigateToSaveView()
            End If
        End Sub

        Private Sub NavigateToSaveView()
            Messenger.Default.Send(New NavigateMessage(SaveViewModel.ViewName, Me.voiceRecorderState))
        End Sub

        Private Sub UpdateAutoTuneSettingsFromGui()
            voiceRecorderState.AutoTuneSettings.Enabled = IsAutoTuneEnabled
            voiceRecorderState.AutoTuneSettings.AttackTimeMilliseconds = Me.AttackTime
            Dim selectedCount = Me.Pitches.Where(Function(p) p.Selected).Count()
            voiceRecorderState.AutoTuneSettings.AutoPitches.Clear()
            For Each pitch In Me.Pitches
                If pitch.Selected OrElse selectedCount = 0 Then
                    voiceRecorderState.AutoTuneSettings.AutoPitches.Add(pitch.Note)
                End If
            Next pitch
        End Sub

        Private Sub SaveAs(ByVal fileName As String)

            Dim saver As New AudioSaver(voiceRecorderState.ActiveFile)
            saver.SaveFileFormat = SaveFileFormat.Wav
            saver.AutoTuneSettings = Me.voiceRecorderState.AutoTuneSettings

            saver.SaveAudio(fileName)

            Me.voiceRecorderState.EffectedFileName = fileName
            DispatcherHelper.CheckBeginInvokeOnUI(Sub() NavigateToSaveView())
        End Sub

        Public Sub Activated(ByVal state As Object) Implements IView.Activated
            Me.voiceRecorderState = CType(state, VoiceRecorderState)
            Me.IsEnabled = True
            Me.IsAutoTuneEnabled = True ' coming into this view turns on autotune
            Me.AttackTime = CInt(Fix(Me.voiceRecorderState.AutoTuneSettings.AttackTimeMilliseconds))
            For Each viewModelPitch In Me.Pitches
                viewModelPitch.Selected = False
            Next viewModelPitch
            For Each pitch In voiceRecorderState.AutoTuneSettings.AutoPitches
                Dim temp = pitch
                Me.Pitches.First(Function(p) p.Note = temp).Selected = True
            Next (pitch)
        End Sub

        Private Sub OnShuttingDown(ByVal message As ShuttingDownMessage)
            If message.CurrentViewName = AutoTuneViewModel.ViewName Then
                Me.voiceRecorderState.DeleteFiles()
            End If
        End Sub

        Private Sub Cancel()
            Messenger.Default.Send(New NavigateMessage(SaveViewModel.ViewName, voiceRecorderState))
        End Sub
    End Class
End Namespace
