using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace VoiceRecorder.Audio
{
    public class AudioSaver
    {
        private string inputFile;

        public TimeSpan TrimFromStart { get; set; }
        public TimeSpan TrimFromEnd { get; set; }

        public AudioSaver(string inputFile)
        {
            this.inputFile = inputFile;
        }

        public void SaveAsWav(string outputFile)
        {
            if (IsTrimNeeded)
            {
                WavFileUtils.TrimWavFile(inputFile, outputFile, TrimFromStart, TrimFromEnd);
            }
            else
            {
                File.Copy(inputFile, outputFile);
            }
        }

        public bool IsTrimNeeded
        {
            get
            {
                return TrimFromStart != TimeSpan.Zero || TrimFromEnd != TimeSpan.Zero;
            }
        }

        public void SaveAsMp3(string lameExePath, string outputFile)
        {
            if (IsTrimNeeded)
            {
                string tempFile = Path.Combine(Path.GetTempPath(), new Guid().ToString() + ".wav");
                try
                {
                    WavFileUtils.TrimWavFile(inputFile, tempFile, TrimFromStart, TrimFromEnd);
                    ConvertToMp3(lameExePath, tempFile, outputFile);
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
            else
            {
                ConvertToMp3(lameExePath, inputFile, outputFile);
            }
            
        }

        public static void ConvertToMp3(string lameExePath, string waveFile, string mp3File)
        {
            Process converter = Process.Start(lameExePath, "-V2 \"" + waveFile + "\" \"" + mp3File + "\"");
            converter.WaitForExit();
        }
    }
}
