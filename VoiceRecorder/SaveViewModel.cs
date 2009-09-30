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
        private SampleAggregator sampleAggregator;
        private ICommand saveCommand;
        private ICommand selectAllCommand;
        private ICommand playCommand;
        private int leftPosition;
        private int rightPosition;
        private int totalWaveFormSamples;
        private IAudioPlayer audioPlayer;
        private int samplesPerSecond;

        public SaveViewModel(IAudioPlayer audioPlayer)
        {
            this.SampleAggregator = new SampleAggregator();
            SampleAggregator.NotificationCount = 800;
            this.audioPlayer = audioPlayer;
            this.saveCommand = new RelayCommand(() => Save());
            this.selectAllCommand = new RelayCommand(() => SelectAll());
            this.playCommand = new RelayCommand(() => Play());
        }

        public ICommand SaveCommand { get { return saveCommand; } }
        public ICommand SelectAllCommand { get { return selectAllCommand; } }
        public ICommand PlayCommand { get { return playCommand; } }

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
        }

        private void Save()
        {            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "WAV file (.wav)|*.wav|MP3 file (.mp3)|.mp3";
            saveFileDialog.DefaultExt = ".wav";
            bool? result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                SaveAs(saveFileDialog.FileName);
            }
        }

        private TimeSpan PositionToTimeSpan(int position)
        {
            int samples = SampleAggregator.NotificationCount * position;
            return TimeSpan.FromSeconds((double)samples / samplesPerSecond);
        }

        private void SaveAs(string fileName)
        {
            AudioSaver saver = new AudioSaver(recordedFile);
            saver.TrimFromStart = PositionToTimeSpan(LeftPosition);
            saver.TrimFromEnd = PositionToTimeSpan(TotalWaveFormSamples - RightPosition);

            if (fileName.ToLower().EndsWith(".wav"))
            {
                saver.SaveAsWav(fileName);
            }
            else if (fileName.ToLower().EndsWith(".mp3"))
            {
                string lameExePath = LocateLame();
                if (lameExePath != null)
                {
                    saver.SaveAsMp3(lameExePath, fileName);
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
                }
            }
            return lameExePath;
        }

        private void RenderFile()
        {
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
                    bytesRead = reader.Read(waveBuffer);
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
