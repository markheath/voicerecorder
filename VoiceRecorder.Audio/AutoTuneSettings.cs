using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceRecorder.Audio
{
    public class AutoTuneSettings
    {
        public AutoTuneSettings()
        {
            // set up defaults
            SnapMode = true;
            PluggedIn = true;
            AutoPitches = new HashSet<Notes>();
            AutoPitches.Add(Notes.C);
            AutoPitches.Add(Notes.D);
            AutoPitches.Add(Notes.E);
            AutoPitches.Add(Notes.F);
            AutoPitches.Add(Notes.G);
            AutoPitches.Add(Notes.A);
            AutoPitches.Add(Notes.B);
            VibratoDepth = 0.0;
            VibratoRate = 4.0;
            AttackTimeMilliseconds = 0.0;
        }

        public bool Enabled { get; set; }
        public bool SnapMode { get; set; }
        public double AttackTimeMilliseconds { get; set; }
        public HashSet<Notes> AutoPitches { get; private set; }
        public bool PluggedIn { get; set; } // not currently used
        public double VibratoRate { get; set; } // not currently used
        public double VibratoDepth { get; set; } 

        /*
         *  vibRateSlider = new GuiSlider(0.2, 20.0, 4.0);
            vibDepthSlider = new GuiSlider(0.0, 0.05, 0);
            attackSlider = new GuiSlider(0.0, 200, 0.0);
         */
    }

    public enum Notes
    {
        C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B
    }
}
