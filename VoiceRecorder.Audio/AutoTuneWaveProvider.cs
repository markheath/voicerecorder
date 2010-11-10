// this class based on code from awesomebox, a project created by by Ravi Parikh and Keegan Poppen, used with permission
// http://decabear.com/awesomebox.html
using System;
using NAudio.Wave;

namespace VoiceRecorder.Audio
{
    public class AutoTuneWaveProvider : IWaveProvider
    {
        private IWaveProvider source;
        private SmbPitchShifter pitchShifter;
        private IPitchDetector pitchDetector;
        private WaveBuffer waveBuffer;
        private AutoTuneSettings autoTuneSettings;

        public AutoTuneWaveProvider(IWaveProvider source) :
            this(source, new AutoTuneSettings())
        {
        }

        public AutoTuneWaveProvider(IWaveProvider source, AutoTuneSettings autoTuneSettings)
        {
            this.autoTuneSettings = autoTuneSettings;
            if (source.WaveFormat.SampleRate != 44100)
                throw new ArgumentException("AutoTune only works at 44.1kHz");
            if (source.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                throw new ArgumentException("AutoTune only works on IEEE floating point audio data");
            if (source.WaveFormat.Channels != 1)
                throw new ArgumentException("AutoTune only works on mono input sources");

            this.source = source;
            this.pitchDetector = new AutoCorrelator(source.WaveFormat.SampleRate);
            // alternative pitch detector:
            // this.pitchDetector = new FftPitchDetector(source.WaveFormat.SampleRate); 
            this.pitchShifter = new SmbPitchShifter(Settings, source.WaveFormat.SampleRate);
            this.waveBuffer = new WaveBuffer(8192);
        }

        public AutoTuneSettings Settings
        {
            get { return this.autoTuneSettings; }
        }
        private float previousPitch;
        private int release;
        private int maxHold = 1;

        public int Read(byte[] buffer, int offset, int count)
        {
            if (waveBuffer == null || waveBuffer.MaxSize < count)
            {
                waveBuffer = new WaveBuffer(count);
            }

            int bytesRead = source.Read(waveBuffer, 0, count);
            //Debug.Assert(bytesRead == count);

            // the last bit sometimes needs to be rounded up:
            if (bytesRead > 0) bytesRead = count;

            //pitchsource->getPitches();
            int frames = bytesRead / sizeof(float); // MRH: was count
            float pitch = pitchDetector.DetectPitch(waveBuffer.FloatBuffer, frames);
                
            // MRH: an attempt to make it less "warbly" by holding onto the pitch for at least one more buffer
            if (pitch == 0f && release < maxHold)
            {
                pitch = previousPitch;
                release++;
            }
            else
            {
                this.previousPitch = pitch;
                release = 0;
            }

            int midiNoteNumber = 40;
            float targetPitch = (float)(8.175 * Math.Pow(1.05946309, midiNoteNumber));

            WaveBuffer outBuffer = new WaveBuffer(buffer);

            pitchShifter.ShiftPitch(waveBuffer.FloatBuffer, pitch, targetPitch, outBuffer.FloatBuffer, frames);

            return frames * 4;
        }

        public WaveFormat WaveFormat
        {
            get { return source.WaveFormat; }
        }
    }
}
