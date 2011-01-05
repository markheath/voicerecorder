Imports System.Text
Imports VoiceRecorder.Core
Imports VoiceRecorder.Audio
Imports GalaSoft.MvvmLight
Imports GalaSoft.MvvmLight.Messaging

Namespace VoiceRecorder
    Friend Class MainWindowViewModel
        Inherits ViewModelBase
        Private views As Dictionary(Of String, FrameworkElement)
        Private _currentView As FrameworkElement
        Private currentViewName As String

        Public Sub New()
            Messenger.Default.Register(Of NavigateMessage)(Me, Sub(message) OnNavigate(message))
            views = New Dictionary(Of String, FrameworkElement)

            SetupView(WelcomeViewModel.ViewName, New WelcomeView, New WelcomeViewModel)
            SetupView(RecorderViewModel.ViewName, New RecorderView, New RecorderViewModel(New AudioRecorder))
            SetupView(SaveViewModel.ViewName, New SaveView, New SaveViewModel(New AudioPlayer))
            SetupView(AutoTuneViewModel.ViewName, New AutoTuneView, New AutoTuneViewModel)

            Messenger.Default.Send(Of NavigateMessage)(New NavigateMessage(WelcomeViewModel.ViewName, Nothing))
        End Sub

        Private Sub OnNavigate(ByVal message As NavigateMessage)
            Me.CurrentView = views(message.TargetView)
            Me.currentViewName = message.TargetView
            CType(Me.CurrentView.DataContext, IView).Activated(message.State)
        End Sub

        Private Sub SetupView(ByVal viewName As String, ByVal view As FrameworkElement, ByVal viewModel As ViewModelBase)
            view.DataContext = viewModel
            views.Add(viewName, view)
        End Sub

        Public Property CurrentView As FrameworkElement
            Get
                Return _currentView
            End Get
            Set(ByVal value As FrameworkElement)
                If Me._currentView IsNot value Then
                    _currentView = value
                    RaisePropertyChanged("CurrentView")
                End If
            End Set
        End Property

        Protected Overrides Overloads Sub Dispose(ByVal disposing As Boolean)
            Messenger.Default.Send(New ShuttingDownMessage(currentViewName))
            CType(CurrentView.DataContext, IDisposable).Dispose()
            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace
