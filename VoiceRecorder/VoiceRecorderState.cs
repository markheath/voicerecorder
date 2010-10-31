using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VoiceRecorder
{
    class VoiceRecorderState
    {
        public string RecordingFileName { get; private set; }
        public string EffectedFileName { get; private set; }

        public VoiceRecorderState(string recordingFileName, string effectedFileName)
        {
            this.RecordingFileName = recordingFileName;
            this.EffectedFileName = effectedFileName;
        }

        public string ActiveFile
        {
            get
            {
                if (String.IsNullOrEmpty(EffectedFileName))
                    return RecordingFileName;
                return EffectedFileName;
            }
        }

        public void DeleteFiles()
        {
            this.RecordingFileName = null;
            this.EffectedFileName = null;
        }

        private void DeleteFile(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }
}
