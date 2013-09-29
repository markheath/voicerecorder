using System;
using NAudio.Wave;

namespace VoiceRecorder.Audio
{
    public class AudioPlayer : IAudioPlayer
    {
        private WaveOut waveOut;
        private TrimWaveStream inStream;

        public void LoadFile(string path)
        {            
            CloseWaveOut();
            CloseInStream();
            inStream = new TrimWaveStream(new WaveFileReader(path));
        }

        public void Play()
        {
            CreateWaveOut();                
            if (waveOut.PlaybackState == PlaybackState.Stopped)
            {
                inStream.Position = 0;
                waveOut.Play();
            }
        }

        private void CreateWaveOut()
        {
            if (waveOut == null)
            {
                waveOut = new WaveOut();                
                waveOut.Init(inStream);
                waveOut.PlaybackStopped += OnPlaybackStopped;
            }
        }

        void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            PlaybackState = PlaybackState.Stopped;
        }

        public void Stop()
        {
            waveOut.Stop();
            inStream.Position = 0;
        }

        public TimeSpan StartPosition 
        {
            get { return inStream.StartPosition; }
            set { inStream.StartPosition = value; }
        }

        public TimeSpan EndPosition
        {
            get { return inStream.EndPosition; }
            set { inStream.EndPosition = value; }
        }

        public TimeSpan CurrentPosition { get; set; }
        public PlaybackState PlaybackState { get; private set; }

        public void Dispose()
        {
            CloseWaveOut();
            CloseInStream();
        }

        private void CloseInStream()
        {
            if (inStream != null)
            {
                inStream.Dispose();
                inStream = null;
            }
        }

        private void CloseWaveOut()
        {
            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
        }
    }
}
