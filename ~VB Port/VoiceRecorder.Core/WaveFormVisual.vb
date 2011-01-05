Imports System.Text

Namespace VoiceRecorder.Core
    Public Class WaveFormVisual
        Inherits FrameworkElement
        Implements IWaveFormRenderer
        Private visualCollection As VisualCollection
        Private yTranslate As Double = 40
        Private yScale As Double = 40
        Private _xSpacing As Double = 2
        Private maxValues As List(Of Single)
        Private minValues As List(Of Single)

        Public Sub New()
            visualCollection = New VisualCollection(Me)
            Me.Reset()
        End Sub

        Private Sub WaveFormVisual_SizeChanged(ByVal sender As Object, ByVal e As SizeChangedEventArgs) Handles Me.SizeChanged
            Dim padding = e.NewSize.Height / 10 ' 10 percent padding
            yScale = (e.NewSize.Height - padding) / 2
            yTranslate = padding + yScale
            Redraw()
        End Sub

        Public ReadOnly Property XSpacing As Double
            Get
                Return _xSpacing
            End Get
        End Property

        Private Function CreateWaveFormVisual() As DrawingVisual
            Dim drawingVisual As New DrawingVisual

            ' Retrieve the DrawingContext in order to create new drawing content.
            Dim drawingContext = drawingVisual.RenderOpen()
            If maxValues.Count > 0 Then
                RenderPolygon(drawingContext)
            End If

            ' Persist the drawing content.
            drawingContext.Close()

            Return drawingVisual
        End Function

        Private Sub RenderPolygon(ByVal drawingContext As DrawingContext)
            Dim fillBrush = Brushes.Bisque
            Dim borderPen = New Pen(Brushes.Black, 1.0)

            Dim myPathFigure As New PathFigure
            myPathFigure.StartPoint = CreatePoint(maxValues, 0)

            Dim myPathSegmentCollection As New PathSegmentCollection

            For i = 1 To maxValues.Count - 1
                myPathSegmentCollection.Add(New LineSegment(CreatePoint(maxValues, i), True))
            Next i
            For i = minValues.Count - 1 To 0 Step -1
                myPathSegmentCollection.Add(New LineSegment(CreatePoint(minValues, i), True))
            Next i

            myPathFigure.Segments = myPathSegmentCollection
            Dim myPathGeometry As New PathGeometry

            myPathGeometry.Figures.Add(myPathFigure)

            drawingContext.DrawGeometry(fillBrush, borderPen, myPathGeometry)
        End Sub

        Private Function CreatePoint(ByVal values As List(Of Single), ByVal xpos As Integer) As Point
            Return New Point(xpos * _xSpacing, SampleToYPosition(values(xpos)))
        End Function

        ' Provide a required override for the VisualChildrenCount property.
        Protected Overrides ReadOnly Property VisualChildrenCount As Integer
            Get
                Return visualCollection.Count
            End Get
        End Property

        ' Provide a required override for the GetVisualChild method.
        Protected Overrides Function GetVisualChild(ByVal index As Integer) As Visual
            If index < 0 OrElse index >= visualCollection.Count Then
                Throw New ArgumentOutOfRangeException
            End If

            Return visualCollection(index)
        End Function

        #Region "IWaveFormRenderer Members"

        Public Sub AddValue(ByVal maxValue As Single, ByVal minValue As Single) Implements IWaveFormRenderer.AddValue
            maxValues.Add(maxValue)
            minValues.Add(minValue)
            Redraw()
        End Sub

        Private Sub Redraw()
            visualCollection.Clear()
            visualCollection.Add(CreateWaveFormVisual())
            Me.InvalidateVisual()
        End Sub

        Private Function SampleToYPosition(ByVal value As Single) As Double
            Return yTranslate + value * yScale
        End Function
        #End Region

        Public Sub Reset()
            maxValues = New List(Of Single)
            minValues = New List(Of Single)
            visualCollection.Clear()
            visualCollection.Add(CreateWaveFormVisual())
        End Sub
    End Class
End Namespace
