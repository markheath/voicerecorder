using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceRecorder.Audio
{
    public class AutoTuneSettings
    {
        public bool SnapMode { get; set; }
        public double AttackTimeMilliseconds { get; set; }
        public HashSet<Notes> Notes { get; set; }
    }

    public enum Notes
    {
        C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B
    }
}
