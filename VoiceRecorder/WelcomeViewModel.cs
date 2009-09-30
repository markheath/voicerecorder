using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using NAudio.Wave;
using System.Windows.Input;
using VoiceRecorder.Core;

namespace VoiceRecorder
{
    class WelcomeViewModel : ViewModelBase
    {
        IViewManager viewManager;
        ObservableCollection<string> recordingDevices;
        int selectedRecordingDeviceIndex;
        ICommand continueCommand;        

        public WelcomeViewModel(IViewManager viewManager)
        {
            this.viewManager = viewManager;            
            this.recordingDevices = new ObservableCollection<string>();
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                recordingDevices.Add(WaveIn.GetCapabilities(n).ProductName);
            }
            this.continueCommand = new RelayCommand(() => MoveToRecorder());            
        }

        public ICommand ContinueCommand { get { return continueCommand; } }

        private void MoveToRecorder()
        {
            viewManager.MoveTo("RecorderView", SelectedIndex);
        }

        public ObservableCollection<string> RecordingDevices 
        {
            get { return recordingDevices; }
        }

        public int SelectedIndex
        {
            get
            {
                return selectedRecordingDeviceIndex;
            }
            set
            {
                if (selectedRecordingDeviceIndex != value)
                {
                    selectedRecordingDeviceIndex = value;
                    RaisePropertyChangedEvent("SelectedIndex");
                }
            }
        }
    }
}
