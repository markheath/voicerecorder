using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceRecorder.Audio
{
    // FFT based pitch detector. seems to work best with block sizes of 4096
    public class FftPitchDetector : IPitchDetector
    {
        private float sampleRate;

        public FftPitchDetector(float sampleRate)
        {
            this.sampleRate = sampleRate;
        }

        // http://en.wikipedia.org/wiki/Window_function
        private float HammingWindow(int n, int N) 
        {
            return 0.54f - 0.46f * (float)Math.Cos((2 * Math.PI * n) / (N - 1));
        }

        private float[] fftBuffer;
        public float DetectPitch(float[] buffer, int frames)
        {
            Func<int, int, float> window = HammingWindow;
            if (fftBuffer == null)
            {
                fftBuffer = new float[frames * 2];
            }
            for (int n = 0; n < frames; n++)
            {
                fftBuffer[n * 2] = buffer[n]; // *window(n, frames);
                fftBuffer[n * 2 + 1] = 0; // need to clear out as fft modifies buffer
            }

            // assuming frames is a power of 2
            SmbPitchShift.smbFft(fftBuffer, frames, -1);

            float binSize = sampleRate / frames;
            int minBin = (int)(85 / binSize);
            int maxBin = (int)(300 / binSize);
            float maxIntensity = 0f;
            int maxBinIndex = 0;
            for (int bin = minBin; bin <= maxBin; bin++)
            {
                float intensity = fftBuffer[bin * 2] * fftBuffer[bin * 2] + fftBuffer[bin * 2 + 1] * fftBuffer[bin * 2 + 1];
                if (intensity > maxIntensity)
                {
                    maxIntensity = intensity;
                    maxBinIndex = bin;
                }
            }
            return binSize * maxBinIndex;
        }
    }
}
