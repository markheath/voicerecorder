using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace VoiceRecorder.Audio
{
    public class TrimWaveStream : WaveStream
    {
        private WaveStream source;
        private long startBytePosition;
        private long endBytePosition;
        private TimeSpan startPosition;
        private TimeSpan endPosition;
        public event EventHandler ReachedEnd;

        public TrimWaveStream(WaveStream source)
        {
            this.source = source;
            this.EndPosition = source.TotalTime;
        }

        public TimeSpan StartPosition
        {
            get
            {
                return startPosition;
            }
            set
            {
                startPosition = value;
                startBytePosition = (int)(WaveFormat.AverageBytesPerSecond * startPosition.TotalSeconds);
                startBytePosition = startBytePosition - (startBytePosition % WaveFormat.BlockAlign);
                Position = 0;
            }
        }

        public TimeSpan EndPosition
        {
            get
            {
                return endPosition;
            }
            set
            {
                endPosition = value;
                endPosition = value;
                endBytePosition = (int)Math.Round(WaveFormat.AverageBytesPerSecond * endPosition.TotalSeconds);
                endBytePosition = endBytePosition - (endBytePosition % WaveFormat.BlockAlign);
            }
        }

        public override WaveFormat WaveFormat
        {
            get { return source.WaveFormat; }
        }

        public override long Length
        {
            get { return endBytePosition - startBytePosition; }
        }

        public override long Position
        {
            get
            {
                return source.Position - startBytePosition;
            }
            set
            {
                source.Position = value + startBytePosition;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRequired = (int)Math.Min(count, Length - Position);
            int bytesRead = 0;
            if (bytesRequired > 0)
            {
                bytesRead = source.Read(buffer, offset, bytesRequired);
            }
            else
            {
                //ReachedEnd(this, EventArgs.Empty);
            }
            return bytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            source.Dispose();
            base.Dispose(disposing);
        }
    }
}
