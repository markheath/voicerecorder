Imports System.Text
Imports VoiceRecorder.Audio

Namespace VoiceRecorder.Core
    ''' <summary>
    ''' Interaction logic for PolylineWaveFormControl.xaml
    ''' </summary>
    Partial Public Class PolygonWaveFormControl
        Inherits UserControl
        Implements IWaveFormRenderer
        Public Shared ReadOnly SampleAggregatorProperty As DependencyProperty =
            DependencyProperty.Register("SampleAggregator", GetType(SampleAggregator),
                                        GetType(PolygonWaveFormControl),
                                        New PropertyMetadata(Nothing, AddressOf OnSampleAggregatorChanged))

        Public Property SampleAggregator As SampleAggregator
            Get
                Return CType(Me.GetValue(SampleAggregatorProperty), SampleAggregator)
            End Get
            Set(ByVal value As SampleAggregator)
                Me.SetValue(SampleAggregatorProperty, value)
            End Set
        End Property

        Private Shared Sub OnSampleAggregatorChanged(ByVal sender As Object, ByVal e As DependencyPropertyChangedEventArgs)
            Dim control As PolygonWaveFormControl = CType(sender, PolygonWaveFormControl)
            control.Subscribe()
        End Sub

        Public Sub Subscribe()
            AddHandler SampleAggregator.MaximumCalculated, AddressOf SampleAggregator_MaximumCalculated
        End Sub

        Private Sub SampleAggregator_MaximumCalculated(ByVal sender As Object, ByVal e As MaxSampleEventArgs)
            If Me.IsEnabled Then
                Me.AddValue(e.MaxSample, e.MinSample)
            End If
        End Sub

        Private renderPosition As Integer
        Private yTranslate As Double = 40
        Private yScale As Double = 40
        Private xScale As Double = 2
        Private blankZone As Integer = 10

        Private waveForm As New Polygon

        Public Sub New()
            InitializeComponent()
            waveForm.Stroke = Me.Foreground
            waveForm.StrokeThickness = 1
            waveForm.Fill = New SolidColorBrush(Colors.Bisque)
            mainCanvas.Children.Add(waveForm)
        End Sub

        Private Sub OnSizeChanged(ByVal sender As Object, ByVal e As SizeChangedEventArgs) Handles Me.SizeChanged
            ' We will remove everything as we are going to rescale vertically
            renderPosition = 0
            ClearAllPoints()

            Me.yTranslate = Me.ActualHeight / 2
            Me.yScale = Me.ActualHeight / 2
        End Sub

        Private Sub ClearAllPoints()
            waveForm.Points.Clear()
        End Sub

        Private ReadOnly Property Points As Integer
            Get
                Return waveForm.Points.Count \ 2
            End Get
        End Property

        Public Sub AddValue(ByVal maxValue As Single, ByVal minValue As Single) Implements IWaveFormRenderer.AddValue
            Dim visiblePixels = CInt(Fix(ActualWidth / xScale))
            If visiblePixels > 0 Then
                CreatePoint(maxValue, minValue)

                If renderPosition > visiblePixels Then
                    renderPosition = 0
                End If
                Dim erasePosition = (renderPosition + blankZone) Mod visiblePixels
                If erasePosition < Points Then
                    Dim yPos = SampleToYPosition(0)
                    waveForm.Points(erasePosition) = New Point(erasePosition * xScale, yPos)
                    waveForm.Points(BottomPointIndex(erasePosition)) = New Point(erasePosition * xScale, yPos)
                End If
            End If
        End Sub

        Private Function BottomPointIndex(ByVal position As Integer) As Integer
            Return waveForm.Points.Count - position - 1
        End Function

        Private Function SampleToYPosition(ByVal value As Single) As Double
            Return yTranslate + value * yScale
        End Function

        Private Sub CreatePoint(ByVal topValue As Single, ByVal bottomValue As Single)
            Dim topYPos = SampleToYPosition(topValue)
            Dim bottomYPos = SampleToYPosition(bottomValue)
            Dim xPos = renderPosition * xScale
            If renderPosition >= Points Then
                Dim insertPos = Points
                waveForm.Points.Insert(insertPos, New Point(xPos, topYPos))
                waveForm.Points.Insert(insertPos + 1, New Point(xPos, bottomYPos))
            Else
                waveForm.Points(renderPosition) = New Point(xPos, topYPos)
                waveForm.Points(BottomPointIndex(renderPosition)) = New Point(xPos, bottomYPos)
            End If
            renderPosition += 1
        End Sub

        ''' <summary>
        ''' Clears the waveform and repositions on the left
        ''' </summary>
        Public Sub Reset()
            renderPosition = 0
            ClearAllPoints()
        End Sub
    End Class
End Namespace
