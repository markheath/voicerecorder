using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using NAudio.Mixer;
using System.IO;

namespace VoiceRecorder.Audio
{
    public class AudioRecorder : IAudioRecorder
    {
        WaveIn waveIn;
        SampleAggregator sampleAggregator;
        UnsignedMixerControl volumeControl;
        double desiredVolume = 100;
        RecordingState recordingState;
        WaveFileWriter writer;
        WaveFormat recordingFormat;

        public event EventHandler Stopped = delegate { };
        
        public AudioRecorder()
        {
            sampleAggregator = new SampleAggregator();
            RecordingFormat = new WaveFormat(8000, 1);
        }

        public WaveFormat RecordingFormat
        {
            get
            {
                return recordingFormat;
            }
            set
            {
                recordingFormat = value;
                sampleAggregator.NotificationCount = value.SampleRate / 10;
            }
        }

        public void BeginMonitoring(int recordingDevice)
        {
            if(recordingState != RecordingState.Stopped)
            {
                throw new InvalidOperationException("Can't begin monitoring while we are in this state: " + recordingState.ToString());
            }
            waveIn = new WaveIn();
            waveIn.DeviceNumber = recordingDevice;
            waveIn.DataAvailable += waveIn_DataAvailable;
            waveIn.RecordingStopped += new EventHandler(waveIn_RecordingStopped);
            waveIn.WaveFormat = recordingFormat;
            waveIn.StartRecording();
            TryGetVolumeControl();
            recordingState = RecordingState.Monitoring;
        }

        void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            recordingState = RecordingState.Stopped;
            writer.Dispose();
            Stopped(this, EventArgs.Empty);
        }

        public void BeginRecording(string waveFileName)
        {
            if (recordingState != RecordingState.Monitoring)
            {
                throw new InvalidOperationException("Can't begin recording while we are in this state: " + recordingState.ToString());
            }
            writer = new WaveFileWriter(waveFileName, recordingFormat);
            recordingState = RecordingState.Recording;
        }

        public void Stop()
        {
            if (recordingState == RecordingState.Recording)
            {
                recordingState = RecordingState.RequestedStop;
                waveIn.StopRecording();
            }
        }

        private void TryGetVolumeControl()
        {
            int waveInDeviceNumber = 0;
            var mixerLine = new MixerLine((IntPtr)waveInDeviceNumber, 0, MixerFlags.WaveIn);
            foreach (var control in mixerLine.Controls)
            {
                if (control.ControlType == MixerControlType.Volume)
                {
                    volumeControl = control as UnsignedMixerControl;
                    MicrophoneLevel = desiredVolume;
                    break;
                }
            }
        }

        public double MicrophoneLevel
        {
            get
            {
                return desiredVolume;
            }
            set
            {
                desiredVolume = value;
                if (volumeControl != null)
                {
                    volumeControl.Percent = value;
                }
            }
        }

        public SampleAggregator SampleAggregator
        {
            get
            {
                return sampleAggregator;
            }
        }

        public RecordingState RecordingState
        {
            get
            {
                return recordingState;
            }
        }

        public TimeSpan RecordedTime
        {
            get
            {
                if(writer == null)
                {
                    return TimeSpan.Zero;
                }
                else
                {
                    return TimeSpan.FromSeconds((double)writer.Length / writer.WaveFormat.AverageBytesPerSecond);
                }
            }
        }

        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;
            WriteToFile(buffer, bytesRecorded);

            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((buffer[index + 1] << 8) |
                                        buffer[index + 0]);
                float sample32 = sample / 32768f;
                sampleAggregator.Add(sample32);
            }
        }

        private void WriteToFile(byte[] buffer, int bytesRecorded)
        {
            long maxFileLength = this.recordingFormat.AverageBytesPerSecond * 60;
 
            if (recordingState == RecordingState.Recording
                || recordingState == RecordingState.RequestedStop)
            {
                int toWrite = (int)Math.Min(maxFileLength - writer.Length, bytesRecorded);
                if (toWrite > 0)
                {
                    writer.WriteData(buffer, 0, bytesRecorded);
                }
                else
                {
                    Stop();
                }
            }
        }
    }
}
