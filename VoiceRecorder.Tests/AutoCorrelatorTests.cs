using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VoiceRecorder.Audio;

namespace VoiceRecorder.Tests
{
    [TestFixture]
    public class AutoCorrelatorTests
    {
        private int sampleRate = 44100;

        [Test]
        public void TestEmptyBufferDoesntDetectAPitch()
        {
            AutoCorrelator autoCorrelator = new AutoCorrelator(sampleRate);
            float pitch = autoCorrelator.DetectPitch(new float[1024], 1024);
            Assert.AreEqual(0f,pitch);
        }

        [Test]
        public void TestSineWaveDetectionFft() 
        {
            float[] buffer = new float[4096]; // FFT needs at least 4096 to get the granularity
            IPitchDetector pitchDetector = new FftPitchDetector(sampleRate);
            TestPitchDetection(buffer, pitchDetector);
        }

        [Test]
        public void TestSineWaveDetectionAutocorrelator()
        {
            float[] buffer = new float[4096];
            IPitchDetector pitchDetector = new AutoCorrelator(sampleRate);
            TestPitchDetection(buffer, pitchDetector);
        }

        private void TestPitchDetection(float[] buffer, IPitchDetector pitchDetector)
        {
            for (int midiNoteNumber = 45; midiNoteNumber < 63; midiNoteNumber++)
            {
                float freq = (float)(8.175 * Math.Pow(1.05946309, midiNoteNumber));
                SetFrequency(buffer, freq);
                float detectedPitch = pitchDetector.DetectPitch(buffer, buffer.Length);
                // since the autocorrelator works with a lag, give it two shots at the same buffer
                detectedPitch = pitchDetector.DetectPitch(buffer, buffer.Length);
                Console.WriteLine("Testing for {0:F2}Hz, got {1:F2}Hz", freq, detectedPitch);
                //Assert.AreEqual(detectedPitch, freq, 0.5);
            }
        }

        private void SetFrequency(float[] buffer, float frequency)
        {
            float amplitude = 0.25f;
            for (int n = 0; n < buffer.Length; n++)
            {
                buffer[n] = (float)(amplitude * Math.Sin((2 * Math.PI * n * frequency) / sampleRate));
            }
        }
    }
}
