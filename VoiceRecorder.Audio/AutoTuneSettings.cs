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
            AutoPitches = new HashSet<Note>();
            AutoPitches.Add(Note.C);
            AutoPitches.Add(Note.CSharp); 
            AutoPitches.Add(Note.D);
            AutoPitches.Add(Note.DSharp);
            AutoPitches.Add(Note.E);
            AutoPitches.Add(Note.F);
            AutoPitches.Add(Note.FSharp);
            AutoPitches.Add(Note.G);
            AutoPitches.Add(Note.GSharp); 
            AutoPitches.Add(Note.A);
            AutoPitches.Add(Note.ASharp); 
            AutoPitches.Add(Note.B);
            VibratoDepth = 0.0;
            VibratoRate = 4.0;
            AttackTimeMilliseconds = 0.0;
        }

        public bool Enabled { get; set; }
        public bool SnapMode { get; set; } // snap mode finds a note from the list to snap to, non-snap mode is provided with target pitches from outside
        public double AttackTimeMilliseconds { get; set; }
        public HashSet<Note> AutoPitches { get; private set; }
        public bool PluggedIn { get; set; } // not currently used
        public double VibratoRate { get; set; } // not currently used
        public double VibratoDepth { get; set; } 

        /*
         *  vibRateSlider = new GuiSlider(0.2, 20.0, 4.0);
            vibDepthSlider = new GuiSlider(0.0, 0.05, 0);
            attackSlider = new GuiSlider(0.0, 200, 0.0);
         */
    }

    public enum Note
    {
        C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B
    }
}
