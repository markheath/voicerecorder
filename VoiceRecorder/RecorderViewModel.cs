using System;
using System.Windows.Input;
using System.IO;
using VoiceRecorder.Audio;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;

namespace VoiceRecorder
{
    class RecorderViewModel : ViewModelBase, IView
    {
        private readonly RelayCommand beginRecordingCommand;
        private readonly RelayCommand stopCommand;
        private readonly IAudioRecorder recorder;
        private float lastPeak;
        private string waveFileName;
        public const string ViewName = "RecorderView";

        public RecorderViewModel(IAudioRecorder recorder)
        {
            this.recorder = recorder;
            this.recorder.Stopped += OnRecorderStopped;
            beginRecordingCommand = new RelayCommand(BeginRecording,
                () => recorder.RecordingState == RecordingState.Stopped ||
                      recorder.RecordingState == RecordingState.Monitoring);
            stopCommand = new RelayCommand(Stop,
                () => recorder.RecordingState == RecordingState.Recording);
            recorder.SampleAggregator.MaximumCalculated += OnRecorderMaximumCalculated;
            Messenger.Default.Register<ShuttingDownMessage>(this, OnShuttingDown);
        }

        void OnRecorderStopped(object sender, EventArgs e)
        {
            Messenger.Default.Send(new NavigateMessage(SaveViewModel.ViewName, 
                new VoiceRecorderState(waveFileName, null)));
        }

        void OnRecorderMaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
            RaisePropertyChanged("CurrentInputLevel");
            RaisePropertyChanged("RecordedTime");
        }

        public ICommand BeginRecordingCommand { get { return beginRecordingCommand; } }
        public ICommand StopCommand { get { return stopCommand; } }

        public void Activated(object state)
        {
            BeginMonitoring((int)state);
        }

        private void OnShuttingDown(ShuttingDownMessage message)
        {
            if (message.CurrentViewName == ViewName)
            {
                recorder.Stop();
            }
        }

        public string RecordedTime
        {
            get
            {
                var current = recorder.RecordedTime;
                return String.Format("{0:D2}:{1:D2}.{2:D3}", 
                    current.Minutes, current.Seconds, current.Milliseconds);
            }
        }

        private void BeginMonitoring(int recordingDevice)
        {
            recorder.BeginMonitoring(recordingDevice);
            RaisePropertyChanged("MicrophoneLevel");
        }

        private void BeginRecording()
        {
            waveFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            recorder.BeginRecording(waveFileName);
            RaisePropertyChanged("MicrophoneLevel");
            RaisePropertyChanged("ShowWaveForm");
        }

        private void Stop()
        {
            recorder.Stop();
        }
        
        public double MicrophoneLevel
        {
            get { return recorder.MicrophoneLevel; }
            set { recorder.MicrophoneLevel = value; }
        }

        public bool ShowWaveForm
        {
            get { return recorder.RecordingState == RecordingState.Recording || 
                recorder.RecordingState == RecordingState.RequestedStop; }
        }

        // multiply by 100 because the Progress bar's default maximum value is 100
        public float CurrentInputLevel { get { return lastPeak * 100; } }

        public SampleAggregator SampleAggregator 
        {
            get
            {
                return recorder.SampleAggregator;
            }
        }
    }
}