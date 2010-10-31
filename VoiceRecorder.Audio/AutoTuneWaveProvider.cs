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
        private AutoCorrelator pitchDetector;
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
            this.pitchDetector = new AutoCorrelator();
            this.pitchShifter = new SmbPitchShifter(Settings);
            this.waveBuffer = new WaveBuffer(8192);
        }

        public AutoTuneSettings Settings
        {
            get { return this.autoTuneSettings; }
        }

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
            int nFrames = bytesRead / sizeof(float); // MRH: was count
            float pitch = pitchDetector.DetectPitch(waveBuffer.FloatBuffer, nFrames);

            int midiNoteNumber = 40;
            float targetPitch = (float)(8.175 * Math.Pow(1.05946309, midiNoteNumber));

            WaveBuffer outBuffer = new WaveBuffer(buffer);

            pitchShifter.ShiftPitch(waveBuffer.FloatBuffer, pitch, targetPitch, outBuffer.FloatBuffer, nFrames);

            return nFrames * 4;
        }

        public WaveFormat WaveFormat
        {
            get { return source.WaveFormat; }
        }
    }
}
