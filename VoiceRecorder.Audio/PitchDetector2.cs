//Ben Fisher, 2008
//halfhourhacks.blogspot.com
//GPL
// modified for VoiceRecorder .NET by Mark Heath 
// mark-dot-net.blogspot.com

// References:
// http://www.fit.vutbr.cz/~cernocky/sig/
// http://ccrma.stanford.edu/~pdelac/154/m154paper.htm

using System;

namespace VoiceRecorder.Audio
{
    public static class PitchDetection
    {
        /// <summary>
        /// Autocorrelation deals with noise better. Amdf slightly better with reverb, but seems weaker overall.
        /// </summary>
        public enum PitchDetectAlgorithm
        {
            Autocorrelation, Amdf
        }

        public static float DetectPitch(float[] buffer, int nFrames)
        {
            //adult male 85 to 155 Hz
            //adult female from 165 to 255 Hz

            int minHz = 85;
            int maxHz = 255;
            return (float)detectPitchCalculation(buffer, nFrames, minHz, maxHz, 1, 1, PitchDetectAlgorithm.Autocorrelation)[0];
        }

        // These work by shifting the signal until it seems to correlate with itself.
        // In other words if the signal looks very similar to (signal shifted 200 samples) than the fundamental period is probably 200 samples
        // Note that the algorithm only works well when there's only one prominent fundamental.
        // This could be optimized by looking at the rate of change to determine a maximum without testing all periods.
        /// <param name="nResolution">Resolution. A value of 1, default, is slowest but most precise.</param>
        /// <param name="nCandidates">Number of results to return</param>
        private static double[] detectPitchCalculation(float[] samples, int sampleCount, double minHz, double maxHz, int nCandidates, int nResolution, PitchDetectAlgorithm algorithm)
        {
            int sampleRate = 44100;
            int channels = 1;
            // note that higher frequency means lower period
            int nLowPeriodInSamples = hzToPeriodInSamples(maxHz, sampleRate);
            int nHiPeriodInSamples = hzToPeriodInSamples(minHz, sampleRate);
            if (nHiPeriodInSamples <= nLowPeriodInSamples) throw new Exception("Bad range for pitch detection.");
            if (channels != 1) throw new Exception("Only mono supported.");
            if (sampleCount < nHiPeriodInSamples) throw new Exception("Not enough samples.");

            // both algorithms work in a similar way
            // they yield an array of data, and then we find the index at which the value is highest.
            double[] results = new double[nHiPeriodInSamples - nLowPeriodInSamples];

            if (algorithm == PitchDetectAlgorithm.Amdf)
            {
                for (int period = nLowPeriodInSamples; period < nHiPeriodInSamples; period += nResolution)
                {
                    double sum = 0;
                    // for each sample, see how close it is to a sample n away. Then sum these.
                    for (int i = 0; i < sampleCount - period; i++)
                        sum += Math.Abs(samples[i] - samples[i + period]);

                    double mean = sum / (double)sampleCount;
                    mean *= -1; //somewhat of a hack. We are trying to find the minimum value, but our findBestCandidates finds the max. value.
                    results[period - nLowPeriodInSamples] = mean;
                }
            }
            else if (algorithm == PitchDetectAlgorithm.Autocorrelation)
            {
                for (int period = nLowPeriodInSamples; period < nHiPeriodInSamples; period += nResolution)
                {
                    double sum = 0;
                    // for each sample, find correlation. (If they are far apart, small)
                    for (int i = 0; i < sampleCount - period; i++)
                        sum += samples[i] * samples[i + period];

                    double mean = sum / (double)sampleCount;
                    results[period - nLowPeriodInSamples] = mean;
                }
            }

            // find the best indices
            int[] bestIndices = findBestCandidates(nCandidates, results); //note findBestCandidates modifies parameter
            // convert back to Hz
            double[] res = new double[nCandidates];
            for (int i = 0; i < nCandidates; i++)
                res[i] = periodInSamplesToHz(bestIndices[i] + nLowPeriodInSamples, sampleRate);
            return res;
        }

        /// <summary>
        /// Finds n "best" values from an array. Returns the indices of the best parts.
        /// (One way to do this would be to sort the array, but that could take too long.
        /// Warning: Changes the contents of the array!!! Do not use result array afterwards.
        /// </summary>
        private static int[] findBestCandidates(int n, double[] inputs)
        {
            if (inputs.Length < n) throw new Exception("Length of inputs is not long enough.");
            int[] res = new int[n]; // will hold indices with the highest amounts.

            for (int c = 0; c < n; c++)
            {
                // find the highest.
                double fBestValue = double.MinValue;
                int nBestIndex = -1;
                for (int i = 0; i < inputs.Length; i++)
                    if (inputs[i] > fBestValue) { nBestIndex = i; fBestValue = inputs[i]; }

                // record this highest value
                res[c] = nBestIndex;

                // now blank out that index.
                inputs[nBestIndex] = double.MinValue;
            }
            return res;
        }

        private static int hzToPeriodInSamples(double hz, int sampleRate)
        {
            return (int)(1 / (hz / (double)sampleRate));
        }
        private static double periodInSamplesToHz(int period, int sampleRate)
        {
            return 1 / (period / (double)sampleRate);
        }
    }
}
