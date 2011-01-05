Imports System.Text

Namespace VoiceRecorder.Audio
    Public Class AutoTuneSettings
        Public Sub New()
            ' set up defaults
            SnapMode = True
            PluggedIn = True
            AutoPitches = New HashSet(Of Note)
            AutoPitches.Add(Note.C)
            AutoPitches.Add(Note.CSharp)
            AutoPitches.Add(Note.D)
            AutoPitches.Add(Note.DSharp)
            AutoPitches.Add(Note.E)
            AutoPitches.Add(Note.F)
            AutoPitches.Add(Note.FSharp)
            AutoPitches.Add(Note.G)
            AutoPitches.Add(Note.GSharp)
            AutoPitches.Add(Note.A)
            AutoPitches.Add(Note.ASharp)
            AutoPitches.Add(Note.B)
            VibratoDepth = 0.0
            VibratoRate = 4.0
            AttackTimeMilliseconds = 0.0
        End Sub

        Public Property Enabled As Boolean

        Public Property SnapMode As Boolean

        Public Property AttackTimeMilliseconds As Double

        Private _autoPitches As HashSet(Of Note)
        Public Property AutoPitches As HashSet(Of Note)
            Get
                Return _autoPitches
            End Get
            Private Set(ByVal value As HashSet(Of Note))
                _autoPitches = value
            End Set
        End Property

        Public Property PluggedIn As Boolean

        Public Property VibratoRate As Double

        Public Property VibratoDepth As Double

        '        
        '         *  vibRateSlider = New GuiSlider(0.2, 20.0, 4.0)
        '            vibDepthSlider = New GuiSlider(0.0, 0.05, 0)
        '            attackSlider = New GuiSlider(0.0, 200, 0.0)
        '        
    End Class


    Public Enum Note
        C
        CSharp
        D
        DSharp
        E
        F
        FSharp
        G
        GSharp
        A
        ASharp
        B
    End Enum
End Namespace
