using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoiceRecorder.Core;
using NAudio.Wave;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using System.IO;
using VoiceRecorder.Audio;
using VoiceRecorder.Properties;

namespace VoiceRecorder
{
    class SaveViewModel : ViewModelBase
    {
        private string recordedFile;
        private string originalFile;
        private SampleAggregator sampleAggregator;
        private int leftPosition;
        private int rightPosition;
        private int totalWaveFormSamples;
        private IAudioPlayer audioPlayer;
        private int samplesPerSecond;
        private bool isAutoTuneApplied;

        public SaveViewModel(IAudioPlayer audioPlayer)
        {
            this.SampleAggregator = new SampleAggregator();
            SampleAggregator.NotificationCount = 800; // gets set correctly later on
            this.audioPlayer = audioPlayer;
            this.SaveCommand = new RelayCommand(() => Save());
            this.SelectAllCommand = new RelayCommand(() => SelectAll());
            this.PlayCommand = new RelayCommand(() => Play());
            this.AutoTuneCommand = new RelayCommand(() => AutoTune(), ()=>CanAutoTune);
        }

        public bool CanAutoTune
        {
            get
            {
                return !isAutoTuneApplied;
            }
        }

        private void AutoTune()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");
            // TODO: onto a background thread
            SaveAs(tempPath, true);
            isAutoTuneApplied = true;
            CommandManager.InvalidateRequerySuggested();
            this.originalFile = this.recordedFile;
            OnViewActivated(tempPath);
        }

        public ICommand SaveCommand { get; private set; }
        public ICommand SelectAllCommand { get; private set;  }
        public ICommand PlayCommand { get; private set; }
        public ICommand AutoTuneCommand { get; private set; }

        public override void OnViewActivated(object state)
        {
            this.recordedFile = (string)state;
            RenderFile();            
            base.OnViewActivated(state);
        }

        public override void OnViewDeactivated(bool shuttingDown)
        {
            audioPlayer.Dispose();            
            File.Delete(recordedFile);
            this.recordedFile = null;
            if (!String.IsNullOrEmpty(originalFile) && File.Exists(originalFile))
            {
                File.Decrypt(originalFile);
                this.originalFile = null;
            }
        }

        private void Save()
        {            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "WAV file (.wav)|*.wav|MP3 file (.mp3)|.mp3";
            saveFileDialog.DefaultExt = ".wav";
            bool? result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                SaveAs(saveFileDialog.FileName, false);
            }
        }

        private TimeSpan PositionToTimeSpan(int position)
        {
            int samples = SampleAggregator.NotificationCount * position;
            return TimeSpan.FromSeconds((double)samples / samplesPerSecond);
        }

        private void SaveAs(string fileName, bool autoTune)
        {
            AudioSaver saver = new AudioSaver(recordedFile);
            saver.TrimFromStart = PositionToTimeSpan(LeftPosition);
            saver.TrimFromEnd = PositionToTimeSpan(TotalWaveFormSamples - RightPosition);
            saver.ApplyAutoTune = autoTune;

            if (fileName.ToLower().EndsWith(".wav"))
            {
                saver.SaveFileFormat = SaveFileFormat.Wav;
                saver.SaveAudio(fileName);
            }
            else if (fileName.ToLower().EndsWith(".mp3"))
            {
                string lameExePath = LocateLame();
                if (lameExePath != null)
                {
                    saver.SaveFileFormat = SaveFileFormat.Mp3;
                    saver.LameExePath = lameExePath;
                    saver.SaveAudio(fileName);
                }
            }
            else
            {
                MessageBox.Show("Please select a supported output format");
            }
        }

        public int LeftPosition
        {
            get
            {
                return leftPosition;
            }
            set
            {
                if (leftPosition != value)
                {
                    leftPosition = value;
                    RaisePropertyChangedEvent("LeftPosition");
                }
            }
        }

        public int RightPosition
        {
            get
            {
                return rightPosition;
            }
            set
            {
                if (rightPosition != value)
                {
                    rightPosition = value;
                    RaisePropertyChangedEvent("RightPosition");
                }
            }
        }

        public string LocateLame()
        {
            string lameExePath = Settings.Default.LameExePath;

            if (String.IsNullOrEmpty(lameExePath) || !File.Exists(lameExePath))
            {
                if (MessageBox.Show("To save as MP3 requires LAME.exe, please locate",
                    "Save as MP3",
                    MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.FileName = "lame.exe";
                    bool? result = ofd.ShowDialog();
                    if (result != null && result.HasValue)
                    {
                        if (File.Exists(ofd.FileName) && ofd.FileName.ToLower().EndsWith("lame.exe"))
                        {
                            Settings.Default.LameExePath = ofd.FileName;
                            Settings.Default.Save();
                            return ofd.FileName;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            return lameExePath;
        }

        private void RenderFile()
        {
            SampleAggregator.RaiseRestart();
            using (WaveFileReader reader = new WaveFileReader(recordedFile))
            {
                this.samplesPerSecond = reader.WaveFormat.SampleRate;
                SampleAggregator.NotificationCount = reader.WaveFormat.SampleRate/10;
            
                byte[] buffer = new byte[1024];
                WaveBuffer waveBuffer = new WaveBuffer(buffer);
                waveBuffer.ByteBufferCount = buffer.Length;
                int bytesRead;
                do
                {
                    bytesRead = reader.Read(waveBuffer, 0, buffer.Length);
                    int samples = bytesRead / 2;
                    for (int sample = 0; sample < samples; sample++)
                    {
                        if (bytesRead > 0)
                        {
                            sampleAggregator.Add(waveBuffer.ShortBuffer[sample] / 32768f);
                        }
                    }
                } while (bytesRead > 0);
                int totalSamples = (int)reader.Length / 2;
                TotalWaveFormSamples = totalSamples / sampleAggregator.NotificationCount;
                SelectAll();
            }
            audioPlayer.LoadFile(recordedFile);
        }

        private void Play()
        {
            audioPlayer.StartPosition = PositionToTimeSpan(LeftPosition);
            audioPlayer.EndPosition = PositionToTimeSpan(RightPosition);
            audioPlayer.Play();
        }

        private void SelectAll()
        {
            LeftPosition = 0;
            RightPosition = TotalWaveFormSamples;
        }

        public SampleAggregator SampleAggregator
        {
            get 
            {
                return sampleAggregator;  
            }
            set
            {
                if (sampleAggregator != value)
                {
                    sampleAggregator = value; 
                    RaisePropertyChangedEvent("SampleAggregator");
                }
            }
        }        

        public int TotalWaveFormSamples
        {
            get
            {
                return totalWaveFormSamples;
            }
            set
            {
                if (totalWaveFormSamples != value)
                {
                    totalWaveFormSamples = value;
                    RaisePropertyChangedEvent("TotalWaveFormSamples");
                }
            }
        }
    }
}
