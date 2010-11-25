using System;
using NAudio.Wave;

namespace VoiceRecorder.Audio
{
    public class AutoTuneUtils
    {
        public static void ApplyAutoTune(string fileToProcess, string tempFile, AutoTuneSettings autotuneSettings)
        {
            using (WaveFileReader reader = new WaveFileReader(fileToProcess))
            {
                IWaveProvider stream32 = new Wave16ToFloatProvider(reader);
                IWaveProvider streamEffect = new AutoTuneWaveProvider(stream32, autotuneSettings);
                IWaveProvider stream16 = new WaveFloatTo16Provider(streamEffect);
                using (WaveFileWriter converted = new WaveFileWriter(tempFile, stream16.WaveFormat))
                {
                    // buffer length needs to be a power of 2 for FFT to work nicely
                    // however, make the buffer too long and pitches aren't detected fast enough
                    // successful buffer sizes: 8192, 4096, 2048, 1024
                    // (some pitch detection algorithms need at least 2048)
                    byte[] buffer = new byte[8192]; 
                    int bytesRead;
                    do
                    {
                        bytesRead = stream16.Read(buffer, 0, buffer.Length);
                        converted.WriteData(buffer, 0, bytesRead);
                    } while (bytesRead != 0 && converted.Length < reader.Length);
                }
            }
        }
    }
}
