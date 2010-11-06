using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoiceRecorder.Audio;

namespace VoiceRecorder
{
    static class PredefinedScales
    {
        public static readonly HashSet<Note> Chromatic = new HashSet<Note>() { Note.C, Note.CSharp, Note.D, Note.DSharp, Note.E, Note.F, Note.FSharp, Note.G, Note.GSharp, Note.A, Note.ASharp, Note.B };

        private static HashSet<Note> MakeScale(Note start, IEnumerable<int> offsets)
        {
            HashSet<Note> scale = new HashSet<Note>();
            foreach (int n in offsets)
            {
                scale.Add((Note)(((int)start + n) % 12));
            }
            return scale;
        }

        public static HashSet<Note> MakeMajorScale(Note start)
        {
            return MakeScale(start, new[] { 0, 2, 4, 5, 7, 9, 11 });
        }

        public static HashSet<Note> MakePentatonicScale(Note start)
        {
            return MakeScale(start, new[] { 0, 2, 4, 7, 9 });
        }
    }
}
