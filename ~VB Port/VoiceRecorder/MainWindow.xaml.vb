Imports System.Text
Imports GalaSoft.MvvmLight.Messaging
Imports GalaSoft.MvvmLight.Threading

Namespace VoiceRecorder
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow
        Inherits Window
        Public Sub New()
            DispatcherHelper.Initialize()
            Dim vm = New MainWindowViewModel
            AddHandler Me.Closed, Sub(s, e) vm.Dispose()
            InitializeComponent()
            Me.DataContext = vm
        End Sub
    End Class
End Namespace
