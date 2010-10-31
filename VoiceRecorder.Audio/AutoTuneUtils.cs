using System;
using NAudio.Wave;

namespace VoiceRecorder.Audio
{
    public class AutoTuneUtils
    {
        public static void ApplyAutoTune(string fileToProcess, string tempFile)
        {
            using (WaveFileReader reader = new WaveFileReader(fileToProcess))
            {
                IWaveProvider stream32 = new Wave16toIeeeProvider(reader);
                IWaveProvider streamEffect = new AutoTuneWaveProvider(stream32);
                IWaveProvider stream16 = new WaveIeeeTo16Provider(streamEffect);
                using (WaveFileWriter converted = new WaveFileWriter(tempFile, stream16.WaveFormat))
                {
                    byte[] buffer = new byte[2048 * 4];
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
