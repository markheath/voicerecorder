Imports System.Text
Imports VoiceRecorder.Audio

Namespace VoiceRecorder.Core
    ''' <summary>
    ''' Interaction logic for WaveFileTrimmerControl.xaml
    ''' </summary>
    Partial Public Class WaveFileTrimmerControl
        Inherits UserControl
        Public Shared ReadOnly SampleAggregatorProperty As DependencyProperty =
            DependencyProperty.Register("SampleAggregator", GetType(SampleAggregator),
                                        GetType(WaveFileTrimmerControl),
                                        New PropertyMetadata(Nothing, AddressOf OnSampleAggregatorChanged))

        Public Shared ReadOnly TotalWaveFormSamplesProperty As DependencyProperty =
            DependencyProperty.Register("TotalWaveFormSamples", GetType(Integer),
                                        GetType(WaveFileTrimmerControl),
                                        New PropertyMetadata(0, AddressOf OnNotificationCountChanged))

        Public Shared ReadOnly LeftSelectionProperty As DependencyProperty =
            DependencyProperty.Register("LeftSelection", GetType(Integer),
                                        GetType(WaveFileTrimmerControl),
                                        New PropertyMetadata(0, AddressOf OnLeftSelectionChanged))

        Public Shared ReadOnly RightSelectionProperty As DependencyProperty =
            DependencyProperty.Register("RightSelection", GetType(Integer),
                                        GetType(WaveFileTrimmerControl),
                                        New PropertyMetadata(0, AddressOf OnRightSelectionChanged))


        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub rangeSelection_SelectionChanged(ByVal sender As Object, ByVal e As EventArgs) Handles rangeSelection.SelectionChanged
            Me.LeftSelection = CInt(Fix(rangeSelection.LeftPos / waveFormRenderer.XSpacing))
            Me.RightSelection = CInt(Fix(rangeSelection.RightPos / waveFormRenderer.XSpacing))
        End Sub

        Public Property SampleAggregator As SampleAggregator
            Get
                Return CType(Me.GetValue(SampleAggregatorProperty), SampleAggregator)
            End Get
            Set(ByVal value As SampleAggregator)
                Me.SetValue(SampleAggregatorProperty, value)
            End Set
        End Property

        Public Property TotalWaveFormSamples As Integer
            Get
                Return CInt(Fix(Me.GetValue(TotalWaveFormSamplesProperty)))
            End Get
            Set(ByVal value As Integer)
                Me.SetValue(TotalWaveFormSamplesProperty, value)
            End Set
        End Property

        Public Property LeftSelection As Integer
            Get
                Return CInt(Fix(Me.GetValue(LeftSelectionProperty)))
            End Get
            Set(ByVal value As Integer)
                Me.SetValue(LeftSelectionProperty, value)
            End Set
        End Property

        Public Property RightSelection As Integer
            Get
                Return CInt(Fix(Me.GetValue(RightSelectionProperty)))
            End Get
            Set(ByVal value As Integer)
                Me.SetValue(RightSelectionProperty, value)
            End Set
        End Property

        Private Shared Sub OnSampleAggregatorChanged(ByVal sender As Object, ByVal e As DependencyPropertyChangedEventArgs)
            Dim control = CType(sender, WaveFileTrimmerControl)
            control.Subscribe()
        End Sub

        Private Shared Sub OnNotificationCountChanged(ByVal sender As Object, ByVal e As DependencyPropertyChangedEventArgs)
            Dim control = CType(sender, WaveFileTrimmerControl)
            control.SetWidth()
        End Sub

        Private Shared Sub OnLeftSelectionChanged(ByVal sender As Object, ByVal e As DependencyPropertyChangedEventArgs)
            Dim control = CType(sender, WaveFileTrimmerControl)
            control.rangeSelection.LeftPos = control.LeftSelection * control.waveFormRenderer.XSpacing
        End Sub

        Private Shared Sub OnRightSelectionChanged(ByVal sender As Object, ByVal e As DependencyPropertyChangedEventArgs)
            Dim control = CType(sender, WaveFileTrimmerControl)
            control.rangeSelection.RightPos = control.RightSelection * control.waveFormRenderer.XSpacing
        End Sub

        Private Sub SetWidth()
            waveFormRenderer.Width = TotalWaveFormSamples * waveFormRenderer.XSpacing
            rangeSelection.Width = TotalWaveFormSamples * waveFormRenderer.XSpacing
            'rangeSelection.SelectAll();            
        End Sub

        Private Sub Subscribe()
            AddHandler SampleAggregator.MaximumCalculated, AddressOf SampleAggregator_MaximumCalculated
            AddHandler SampleAggregator.Restart, AddressOf SampleAggregator_Restart
        End Sub

        Private Sub SampleAggregator_Restart(ByVal sender As Object, ByVal e As EventArgs)
            Me.waveFormRenderer.Reset()
        End Sub

        Private Sub SampleAggregator_MaximumCalculated(ByVal sender As Object, ByVal e As MaxSampleEventArgs)
            waveFormRenderer.AddValue(e.MaxSample, e.MinSample)
        End Sub
    End Class
End Namespace
