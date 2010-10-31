using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VoiceRecorder.Audio;

namespace VoiceRecorder
{
    class VoiceRecorderState
    {
        private string recordingFileName;
        private string effectedFileName;
        private AutoTuneSettings autoTuneSettings;

        public VoiceRecorderState(string recordingFileName, string effectedFileName)
        {
            this.RecordingFileName = recordingFileName;
            this.EffectedFileName = effectedFileName;
            this.autoTuneSettings = new AutoTuneSettings();
        }

        public string RecordingFileName
        {
            get
            {
                return recordingFileName;
            }
            set
            {
                if ((recordingFileName != null) && (recordingFileName != value))
                {
                    DeleteFile(recordingFileName);
                }
                this.recordingFileName = value;
            }
        }

        public string EffectedFileName
        {
            get
            {
                return effectedFileName;
            }
            set
            {
                if ((effectedFileName != null) && (effectedFileName != value))
                {
                    DeleteFile(effectedFileName);
                }
                this.effectedFileName = value;
            }
        }

        public string ActiveFile
        {
            get
            {
                if (autoTuneSettings.Enabled && !String.IsNullOrEmpty(EffectedFileName))
                {
                    return EffectedFileName;
                }
                return RecordingFileName;
            }
        }

        public AutoTuneSettings AutoTuneSettings
        {
            get
            {
                return autoTuneSettings;
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
