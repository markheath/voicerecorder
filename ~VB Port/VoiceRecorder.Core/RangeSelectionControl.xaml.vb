Imports System.Text

Namespace VoiceRecorder.Core
    ''' <summary>
    ''' Interaction logic for RangeSelectionControl.xaml
    ''' </summary>
    Partial Public Class RangeSelectionControl
        Inherits UserControl
        Private _dragEdge As Edge
        Private _leftPos As Double
        Private _rightPos As Double
        Private minSelectionWidth As Double = 20

        Public Event SelectionChanged As EventHandler

        Private Enum Edge
            None
            Left
            Right
        End Enum

        Public Property LeftPos As Double
            Get
                Return _leftPos
            End Get
            Set(ByVal value As Double)
                If _leftPos <> value Then
                    _leftPos = value
                    highlightRect.SetValue(Canvas.LeftProperty, value)
                    UpdateRightHandPosition(_rightPos) ' keep right hand edge where it was
                    RaiseEvent SelectionChanged(Me, EventArgs.Empty)
                End If
            End Set
        End Property

        Public Property RightPos As Double
            Get
                Return _rightPos
            End Get
            Set(ByVal value As Double)
                If _rightPos <> value Then
                    UpdateRightHandPosition(value)
                    RaiseEvent SelectionChanged(Me, EventArgs.Empty)
                End If
            End Set
        End Property

        Private Sub UpdateRightHandPosition(ByVal value As Double)
            _rightPos = value
            highlightRect.Width = value - LeftPos
        End Sub

        Private Property DragEdge As Edge
            Get
                Return _dragEdge
            End Get
            Set(ByVal value As Edge)
                If _dragEdge <> value Then
                    _dragEdge = value
                    If _dragEdge = Edge.None Then
                        Cursor = Cursors.Arrow
                    End If
                End If
            End Set
        End Property

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub RangeSelectionControl_MouseUp(ByVal sender As Object, ByVal e As MouseButtonEventArgs) Handles mainCanvas.MouseUp
            If mainCanvas.IsMouseCaptured Then
                mainCanvas.ReleaseMouseCapture()
            End If

            DragEdge = Edge.None
        End Sub

        Private Sub RangeSelectionControl_SizeChanged(ByVal sender As Object, ByVal e As SizeChangedEventArgs) Handles Me.SizeChanged
            highlightRect.Height = e.NewSize.Height
        End Sub

        Private Sub RangeSelectionControl_MouseLeave(ByVal sender As Object, ByVal e As MouseEventArgs) Handles mainCanvas.MouseLeave
            'DragEdge = Edge.None
        End Sub

        Private Sub RangeSelectionControl_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs) Handles mainCanvas.MouseMove
            Dim position = e.GetPosition(Me)

            If _dragEdge = Edge.None Then
                If EdgeAtPosition(position.X) <> Edge.None Then
                    Cursor = Cursors.SizeWE
                Else
                    Cursor = Cursors.Arrow
                End If
            ElseIf _dragEdge = Edge.Left Then
                If position.X < 0 Then
                    LeftPos = 0
                ElseIf position.X < RightPos - minSelectionWidth Then
                    LeftPos = position.X
                End If
            ElseIf _dragEdge = Edge.Right Then
                If position.X > Me.ActualWidth Then
                    RightPos = Me.ActualWidth
                ElseIf position.X > LeftPos + minSelectionWidth Then
                    RightPos = position.X
                End If
            End If
        End Sub

        Private Sub RangeSelectionControl_MouseDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs) Handles mainCanvas.MouseDown
            If e.LeftButton = MouseButtonState.Pressed Then
                Dim position = e.GetPosition(Me)
                Dim edge As Edge = EdgeAtPosition(position.X)
                DragEdge = edge
                If DragEdge <> edge.None Then
                    mainCanvas.CaptureMouse()
                End If
            End If
        End Sub

        Private Function EdgeAtPosition(ByVal X As Double) As Edge
            Dim tolerance As Double = 2
            If X >= (_leftPos - tolerance) AndAlso X <= (_leftPos + tolerance) Then
                Return Edge.Left
            End If
            If X >= (_rightPos - tolerance) AndAlso X <= (_rightPos + tolerance) Then
                Return Edge.Right
            End If
            Return Edge.None
        End Function

        Public Sub SelectAll()
            LeftPos = 0
            RightPos = Width
        End Sub
    End Class
End Namespace