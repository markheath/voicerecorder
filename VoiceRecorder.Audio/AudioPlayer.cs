using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace VoiceRecorder.Audio
{
    public class AudioPlayer : IAudioPlayer
    {
        private WaveOut waveOut;
        private TrimWaveStream inStream;
        
        public AudioPlayer()
        {
        }

        public void LoadFile(string path)
        {
            inStream = new TrimWaveStream(new WaveFileReader(path));
            inStream.ReachedEnd += new EventHandler(inStream_ReachedEnd);
        }

        void inStream_ReachedEnd(object sender, EventArgs e)
        {
            Stop();
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
                waveOut.PlaybackStopped += new EventHandler(waveOut_PlaybackStopped);
            }
        }

        void waveOut_PlaybackStopped(object sender, EventArgs e)
        {
            this.PlaybackState = PlaybackState.Stopped;
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
            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
            if (inStream != null)
            {
                inStream.Dispose();
                inStream = null;
            }
        }
    }
}
