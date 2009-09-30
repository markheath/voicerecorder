using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceRecorder.Core
{
    public interface IWaveFormRenderer
    {
        void AddValue(float maxValue, float minValue);
    }
}
