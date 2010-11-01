// this class based on code from awesomebox, a project created by by Ravi Parikh and Keegan Poppen, used with permission
// http://decabear.com/awesomebox.html
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VoiceRecorder.Audio
{
    class PitchShifter
    {
        int detectedNote;
        protected float detectedPitch;  //set == inputPitch when shiftPitch is called
        protected float shiftedPitch;   //set == the target pitch when shiftPitch is called
        int numshifts;  //number of stored detectedPitch, shiftedPitch pairs stored for the viewer (more = slower, less = faster)
        Queue<PitchShift> shifts;
        protected int currPitch;
        protected int attack;
        int numElapsed;
        protected double vibRate;
        protected double vibDepth;
        double g_time;

        protected AutoTuneSettings settings;

        public PitchShifter(AutoTuneSettings settings)
        {
            this.settings = settings;
            numshifts = 5000;
            shifts = new Queue<PitchShift>(numshifts);

            currPitch = 0;
            attack = 0;
            numElapsed = 0;
            vibRate = 4.0;
            vibDepth = 0.00;
            g_time = 0.0;
        }

        protected float snapFactor(float freq)
        {
            float previousFrequency = 0.0f;
            float correctedFrequency = 0.0f;
            int previousNote = 0;
            int correctedNote = 0;
            for (int i = 1; i < 120; i++)
            {
                bool endLoop = false;
                foreach (int note in this.settings.AutoPitches)
                {
                    if (i % 12 == note)
                    {
                        previousFrequency = correctedFrequency;
                        previousNote = correctedNote;
                        correctedFrequency = (float)(8.175 * Math.Pow(1.05946309, (float)i));
                        correctedNote = i;
                        if (correctedFrequency > freq) { endLoop = true; }
                        break;
                    }
                }
                if (endLoop)
                {
                    break;
                }
            }
            if (correctedFrequency == 0.0) { return 1.0f; }
            int destinationNote = 0;
            double destinationFrequency = 0.0;
            // decide whether we are shifting up or down
            if (correctedFrequency - freq > freq - previousFrequency)
            {
                destinationNote = previousNote;
                destinationFrequency = previousFrequency;
            }
            else
            {
                destinationNote = correctedNote;
                destinationFrequency = correctedFrequency;
            }
            if (destinationNote != currPitch)
            {
                numElapsed = 0;
                currPitch = destinationNote;
            }
            if (attack > numElapsed)
            {
                double n = (destinationFrequency - freq) / attack * numElapsed;
                destinationFrequency = freq + n;
            }
            numElapsed++;
            return (float)(destinationFrequency / freq);
        }

        protected void updateShifts(float detected, float shifted, int targetNote)
        {
            if (shifts.Count >= numshifts) shifts.Dequeue();
            PitchShift shift = new PitchShift(detected, shifted, targetNote);
            Debug.WriteLine(shift);
            shifts.Enqueue(shift);
        }

        void setDetectedNote(float pitch)
        {
            for (int i = 0; i < 120; i++)
            {
                float d = (float)(8.175 * Math.Pow(1.05946309, (float)i) - pitch);
                if (-1.0 < d && d < 1.0)
                {
                    detectedNote = i;
                    return;
                }
            }
            detectedNote = -1;
        }

        bool isDetectedNote(int note)
        {
            return (note % 12) == (detectedNote % 12) && detectedNote >= 0;
        }

        protected float addVibrato(int nFrames)
        {
            g_time += nFrames;
            float d = (float)(Math.Sin(2 * 3.14159265358979 * vibRate * g_time / 44100) * vibDepth);
            return d;
        }
    }

    class SmbPitchShifter : PitchShifter
    {
        public SmbPitchShifter(AutoTuneSettings settings) : base(settings) { }

        void pitchShift(float factor, int nFrames, float[] buff)
        {
            //before the second nFrames was def_buffer_size, but def_buffer_size == nFrames (I think)
            // MRH: was nFrames but this is not a power of 2
            // 2048 works, let's try 1024
            int fftFrameSize = 2048;
            int osamp = 32; // 32 is best quality
            SmbPitchShift.smbPitchShift(factor, nFrames, fftFrameSize, osamp, 44100f, buff, buff);
        }

        public void ShiftPitch(float[] inputBuff, float inputPitch, float targetPitch, float[] outputBuff, int nFrames)
        {
            UpdateSettings();
            detectedPitch = inputPitch;
            float shiftFactor = 1.0f;
            if (this.settings.SnapMode)
            {
                if (inputPitch > 0)
                {
                    shiftFactor = snapFactor(inputPitch);
                    shiftFactor += addVibrato(nFrames);
                }
                if (shiftFactor > 2.0) shiftFactor = 2.0f;
                if (shiftFactor < 0.5) shiftFactor = 0.5f;

                float[] tempBuff = new float[nFrames];
                for (int i = 0; i < nFrames; i++)
                {
                    tempBuff[i] = (float)(inputBuff[i]);
                }

                pitchShift(shiftFactor, nFrames, tempBuff);

                for (int i = 0; i < nFrames; i++)
                {
                    outputBuff[i] = tempBuff[i]; // MRH: overwrite, don't accumulate
                }
            }
            else
            {
                //foreach (float midiPitch in pitches)
                float midiPitch = targetPitch;
                {
                    shiftFactor = 1.0f;
                    if (inputPitch > 0 && midiPitch > 0)
                    {
                        shiftFactor = midiPitch / inputPitch;
                    }

                    if (shiftFactor > 2.0) shiftFactor = 2.0f;
                    if (shiftFactor < 0.5) shiftFactor = 0.5f;

                    float[] tempBuff = new float[nFrames];
                    for (int i = 0; i < nFrames; i++)
                    {
                        tempBuff[i] = inputBuff[i];
                    }

                    pitchShift(shiftFactor, nFrames, tempBuff);

                    for (int i = 0; i < nFrames; i++)
                    {
                        outputBuff[i] = tempBuff[i];
                    }
                    //break;  //this line is a hack, because we are not polyphonic right now
                }
            }

            //vibrato
            //addVibrato(outputBuff, nFrames);

            shiftedPitch = inputPitch * shiftFactor;
            updateShifts(detectedPitch, shiftedPitch, this.currPitch);
        }

        private void UpdateSettings()
        {
            //these are going here, because this gets called once per frame
            vibRate = this.settings.VibratoRate;
            vibDepth = this.settings.VibratoDepth;
            attack = (int)((this.settings.AttackTimeMilliseconds * 441) / 1024.0);
        }
    }
    
    class PitchShift
    {
        public PitchShift(float detected, float shifted, int destNote)
        {
            this.DetectedPitch = detected;
            this.ShiftedPitch = shifted;
            this.DestinationNote = destNote;
        }

        public float DetectedPitch { get; private set; }
        public float ShiftedPitch { get; private set; }
        public int DestinationNote { get; private set; }

        public override string ToString()
        {
            return String.Format("detected {0:f2}Hz, shifted to {1:f2}Hz, {2}{3} ", DetectedPitch, ShiftedPitch,
                (Notes)(DestinationNote % 12),DestinationNote/12);
        }
    }
}
