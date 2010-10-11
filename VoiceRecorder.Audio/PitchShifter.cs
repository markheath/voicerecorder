// this class based on code from awesomebox, a project created by by Ravi Parikh and Keegan Poppen, used with permission
// http://decabear.com/awesomebox.html
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceRecorder.Audio
{
    class PitchShifter
    {
        int detectedNote;
        protected float pitch;	//set == inputPitch when shiftPitch is called
        protected float shiftedPitch;	//set == the target pitch when shiftPitch is called
        int numshifts;	//number of stored detectedPitch, shiftedPitch pairs stored for the viewer (more = slower, less = faster)
        Queue<PitchShift> shifts;
        int currPitch;
        int attack;
        int numElapsed;
        double vibRate;
        double vibDepth;
        double g_time;
        bool midiPluggedIn;

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

        protected void updateShifts()
        {
            if (shifts.Count >= numshifts) shifts.Dequeue();
            PitchShift shift;
            shift.detectedPitch = pitch;
            shift.shiftedPitch = shiftedPitch;
            shifts.Enqueue(shift);

            //these are going here, because this gets called once per frame
            vibRate = this.settings.VibratoRate;
            vibDepth = this.settings.VibratoDepth; 
            attack = (int)((this.settings.AttackTimeMilliseconds * 441) / 1024.0);
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
            int fftFrameSize = 2048; // MRH: was nFrames but this is not a power of 2
            SmbPitchShift.smbPitchShift(factor, nFrames, fftFrameSize, 32, 44100f, buff, buff);
        }

        public void ShiftPitch(float[] inputBuff, float inputPitch, float targetPitch, float[] outputBuff, int nFrames)
        {
            pitch = inputPitch;
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
                    outputBuff[i] = tempBuff[i]; // MRH: this was accumulating into output buffer?
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
                        outputBuff[i] += tempBuff[i];
                    }
                    //break;  //this line is a hack, because we are not polyphonic right now
                }
            }

            //vibrato
            //addVibrato(outputBuff, nFrames);

            shiftedPitch = inputPitch * shiftFactor;
            updateShifts();
        }
    }
    
    struct PitchShift
    {
        public float detectedPitch;
        public float shiftedPitch; //eventually this needs to handle multiple pitches . . . (?)
    }

    class GuiSlider
    {
        public GuiSlider(double minVal, double maxVal, double initialVal)
        {
            this.Value = initialVal;
        }

        public double Value { get; private set; }
    }
}
