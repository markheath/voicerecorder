using NUnit.Framework;
using NAudio.Wave;
using VoiceRecorder.Audio;
using System;
using System.Diagnostics;

namespace VoiceRecorder.Tests 
{
    [TestFixture]
    public class PerformanceTests 
    {
        [Test]
        public void TimingTest() 
        {
            Stopwatch timer = new Stopwatch();
            int iterations = 10;
            long ms = timer.Time(() => ReadFromTestProvider(1024 * 1024, 4096), iterations);
            Console.WriteLine("{0} ms", ms);
        }

        [Test]
        public void ShortTest()
        {
            Stopwatch timer = new Stopwatch();
            long ms = timer.Time(() => ReadFromTestProvider(16 * 1024, 4096));
            Console.WriteLine("{0} ms", ms);
        }

        private static void ReadFromTestProvider(int bytesToRead, int bufferSize)
        {
            TestWaveProvider source = new TestWaveProvider(44100, 1);
            AutoTuneWaveProvider autoTune = new AutoTuneWaveProvider(source);
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            while (bytesRead < bytesToRead)
            {
                bytesRead += autoTune.Read(buffer, 0, buffer.Length);
            }
        }
    }

    // http://stackoverflow.com/questions/232848/wrapping-stopwatch-timing-with-a-delegate-or-lambda
    static class StopwatchExtensions
    {
        public static long Time(this Stopwatch sw, Action action)
        {
            return sw.Time(action, 1);
        }
        
        public static long Time(this Stopwatch sw, Action action, int iterations)
        {
            sw.Reset();
            sw.Start();
            for (int i = 0; i < iterations; i++)
            {
                action();
            }
            sw.Stop();

            return sw.ElapsedMilliseconds;
        }
    }

    class TestWaveProvider : WaveProvider32
    {
        float[] testData;
        int testIndex;

        public TestWaveProvider(int sampleRate, int channels) : base(sampleRate, channels)
        {
            testData = new float[sampleRate * channels * 4]; // four seconds of audio      
            // for now, our test data is a sine wave
            float Frequency = 517;
            float Amplitude = 0.25f;
            for (int sample = 0; sample < testData.Length; sample++)
            {
                testData[sample] = (float)(Amplitude * Math.Sin((2 * Math.PI * sample * Frequency) / sampleRate));
            }
        }
                
        public override int Read(float[] buffer, int offset, int sampleCount)
        {

            for (int n = 0; n < sampleCount; n++)
            {
                buffer[offset + n] = testData[testIndex++];
                if (testIndex >= testData.Length)
                {
                    testIndex = 0;
                }
            }
            return sampleCount;
        }
    }
}
