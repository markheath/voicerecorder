Imports System.Text
Imports VoiceRecorder.Audio

Namespace VoiceRecorder
    Friend NotInheritable Class PredefinedScales
        Public Shared ReadOnly Chromatic As New HashSet(Of Note) From {Note.C,
                                                                       Note.CSharp,
                                                                       Note.D,
                                                                       Note.DSharp,
                                                                       Note.E,
                                                                       Note.F,
                                                                       Note.FSharp,
                                                                       Note.G,
                                                                       Note.GSharp,
                                                                       Note.A,
                                                                       Note.ASharp,
                                                                       Note.B}

        Private Sub New()
        End Sub

        Private Shared Function MakeScale(ByVal start As Note, ByVal offsets As IEnumerable(Of Integer)) As HashSet(Of Note)
            Dim scale As New HashSet(Of Note)
            For Each n In offsets
                scale.Add(CType((CInt(Fix(start)) + n) Mod 12, Note))
            Next n
            Return scale
        End Function

        Public Shared Function MakeMajorScale(ByVal start As Note) As HashSet(Of Note)
            Return MakeScale(start, { 0, 2, 4, 5, 7, 9, 11 })
        End Function

        Public Shared Function MakePentatonicScale(ByVal start As Note) As HashSet(Of Note)
            Return MakeScale(start, { 0, 2, 4, 7, 9 })
        End Function
    End Class
End Namespace
