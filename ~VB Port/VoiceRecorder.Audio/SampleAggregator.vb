Imports System.Text

Namespace VoiceRecorder.Audio
    Public Class SampleAggregator
        ' volume
        Public Event MaximumCalculated As EventHandler(Of MaxSampleEventArgs)
        Public Event Restart As EventHandler
        Public maxValue As Single
        Public minValue As Single
        Public Property NotificationCount As Integer
        Private count As Integer

        Public Sub New()
        End Sub

        Public Sub RaiseRestart()
            RaiseEvent Restart(Me, EventArgs.Empty)
        End Sub

        Private Sub Reset()
            count = 0
            minValue = 0
            maxValue = minValue
        End Sub

        Public Sub Add(ByVal value As Single)
            maxValue = Math.Max(maxValue, value)
            minValue = Math.Min(minValue, value)
            count += 1
            If count >= NotificationCount AndAlso NotificationCount > 0 Then
                RaiseEvent MaximumCalculated(Me, New MaxSampleEventArgs(minValue, maxValue))
                Reset()
            End If
        End Sub
    End Class

    Public Class MaxSampleEventArgs
        Inherits EventArgs
        <DebuggerStepThrough>
        Public Sub New(ByVal minValue As Single, ByVal maxValue As Single)
            Me.MaxSample = maxValue
            Me.MinSample = minValue
        End Sub

        Private _maxSample As Single
        Public Property MaxSample As Single
            Get
                Return _maxSample
            End Get
            Private Set(ByVal value As Single)
                _maxSample = value
            End Set
        End Property

        Private _minSample As Single
        Public Property MinSample As Single
            Get
                Return _minSample
            End Get
            Private Set(ByVal value As Single)
                _minSample = value
            End Set
        End Property
    End Class
End Namespace
