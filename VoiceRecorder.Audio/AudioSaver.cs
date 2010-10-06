using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using NAudio.Wave;

namespace VoiceRecorder.Audio
{
    public enum SaveFileFormat
    {
        Wav,
        Mp3
    }

    public class AudioSaver
    {
        private string inputFile;

        public TimeSpan TrimFromStart { get; set; }
        public TimeSpan TrimFromEnd { get; set; }
        public bool ApplyAutoTune { get; set; }
        public SaveFileFormat SaveFileFormat { get; set; }
        public string LameExePath { get; set; }

        public AudioSaver(string inputFile)
        {
            this.inputFile = inputFile;
        }

        public bool IsTrimNeeded
        {
            get
            {
                return TrimFromStart != TimeSpan.Zero || TrimFromEnd != TimeSpan.Zero;
            }
        }

        public void SaveAudio(string outputFile)
        {
            List<string> tempFiles = new List<string>();
            string fileToProcess = inputFile;
            if (IsTrimNeeded)
            {
                string tempFile = WavFileUtils.GetTempWavFileName();
                tempFiles.Add(tempFile);
                WavFileUtils.TrimWavFile(inputFile, tempFile, TrimFromStart, TrimFromEnd);
                fileToProcess = tempFile;
            }
            if (ApplyAutoTune)
            {
                string tempFile = WavFileUtils.GetTempWavFileName();
                tempFiles.Add(tempFile);
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
                fileToProcess = tempFile;
            }
            if (SaveFileFormat == SaveFileFormat.Mp3)
            {
                ConvertToMp3(this.LameExePath, fileToProcess, outputFile);
            }
            else
            {
                File.Copy(fileToProcess, outputFile);
            }
            DeleteTempFiles(tempFiles);
        }

        private void DeleteTempFiles(IEnumerable<string> tempFiles)
        {
            foreach (string tempFile in tempFiles)
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public static void ConvertToMp3(string lameExePath, string waveFile, string mp3File)
        {
            Process converter = Process.Start(lameExePath, "-V2 \"" + waveFile + "\" \"" + mp3File + "\"");
            converter.WaitForExit();
        }
    }
}
