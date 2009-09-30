using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceRecorder.Audio
{
    public enum RecordingState
    {
        Stopped,
        Monitoring,
        Recording,
        RequestedStop
    }
}
