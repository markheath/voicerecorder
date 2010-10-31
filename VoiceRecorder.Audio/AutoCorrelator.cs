using System;

namespace VoiceRecorder.Audio
{
    class AutoCorrelator
    {
        public float DetectPitch(float[] buffer, int nFrames)
        {
            if (prevBuffer == null)
            {
                prevBuffer = new float[nFrames];
                return 0.0f;
            }
            float secCor = 0;
            int secLag = 0;

            float maxCorr = 0;
            int maxLag = 0;
            for (int lag = 512; lag >= 40; lag--)
            {
                float corr = 0;
                for (int i = 0; i < nFrames; i++)
                {
                    float lagVal = ((i - lag < 0) ? prevBuffer[nFrames - (lag - i)] : buffer[i - lag]);
                    corr += (lagVal * buffer[i]);
                }
                if (corr > maxCorr)
                {
                    maxCorr = corr;
                    maxLag = lag;
                }
                if (corr >= 0.9 * maxCorr)
                {
                    secCor = corr;
                    secLag = lag;
                }
            }
            Array.Copy(buffer, prevBuffer, nFrames);
            if (maxCorr < 0.1) return 0.0f;
            return 44100.0f / secLag;   //--works better for singing
            //return 44100.0 / maxLag;
        }
        private float[] prevBuffer;
    }


}
