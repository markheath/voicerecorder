using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoiceRecorder.Core;
using System.Windows.Input;
using System.Collections.ObjectModel;
using VoiceRecorder.Audio;
using System.IO;

namespace VoiceRecorder
{
    public class NoteViewModel
    {
        public NoteViewModel(Notes note, string displayName)
        {
            this.Note = note;
            this.DisplayName = displayName;
        }

        public Notes Note { get; set; }
        public bool Selected { get; set; }
        public string DisplayName { get; set; }
    }

    class AutoTuneViewModel : ViewModelBase, IDisposable
    {
        public ICommand ApplyCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private int attackTimeMilliseconds;
        private bool isSnapMode;
        private bool isAutoTuneEnabled;
        private VoiceRecorderState voiceRecorderState;

        public int AttackTime
        {
            get { return attackTimeMilliseconds; }
            set
            {
                if (attackTimeMilliseconds != value)
                {
                    attackTimeMilliseconds = value;
                    RaisePropertyChangedEvent("AttackTime");
                    RaisePropertyChangedEvent("AttackMessage");
                }
            }
        }

        public string AttackMessage
        {
            get { return String.Format("{0}ms", attackTimeMilliseconds); }
        }

        public bool IsSnapMode
        {
            get
            {
                return isSnapMode;
            }
            set
            {
                if (isSnapMode != value)
                {
                    isSnapMode = value;
                    RaisePropertyChangedEvent("IsSnapMode");
                }
            }
        }

        public bool IsAutoTuneEnabled
        {
            get
            {
                return isAutoTuneEnabled;
            }
            set
            {
                if (isAutoTuneEnabled != value)
                {
                    isAutoTuneEnabled = value;
                    RaisePropertyChangedEvent("IsAutoTuneEnabled");
                }
            }
        }

        public ObservableCollection<NoteViewModel> Pitches { get; private set; }

        public AutoTuneViewModel()
        {
            this.ApplyCommand = new RelayCommand(() => Apply());
            this.CancelCommand = new RelayCommand(() => Cancel());
            this.Pitches = new ObservableCollection<NoteViewModel>();
            this.Pitches.Add(new NoteViewModel(Notes.C,"C"));
            this.Pitches.Add(new NoteViewModel(Notes.CSharp,"C#"));
            this.Pitches.Add(new NoteViewModel(Notes.D,"D"));
            this.Pitches.Add(new NoteViewModel(Notes.DSharp,"D#"));
            this.Pitches.Add(new NoteViewModel(Notes.E,"E"));
            this.Pitches.Add(new NoteViewModel(Notes.F, "F"));
            this.Pitches.Add(new NoteViewModel(Notes.FSharp, "F#"));
            this.Pitches.Add(new NoteViewModel(Notes.G,"G"));
            this.Pitches.Add(new NoteViewModel(Notes.GSharp,"G#"));
            this.Pitches.Add(new NoteViewModel(Notes.A,"A"));
            this.Pitches.Add(new NoteViewModel(Notes.ASharp,"A#"));
            this.Pitches.Add(new NoteViewModel(Notes.B,"B"));
        }

        private void Apply()
        {
            voiceRecorderState.AutoTuneSettings.Enabled = true;
            voiceRecorderState.AutoTuneSettings.SnapMode = IsSnapMode;
            voiceRecorderState.AutoTuneSettings.AttackTimeMilliseconds = this.AttackTime;
            var selectedCount = this.Pitches.Count(p => p.Selected);
            voiceRecorderState.AutoTuneSettings.AutoPitches.Clear();
            foreach (var pitch in this.Pitches)
            {
                if (pitch.Selected || selectedCount == 0)
                {
                    voiceRecorderState.AutoTuneSettings.AutoPitches.Add(pitch.Note);
                }
            }

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");
            // TODO: onto a background thread
            SaveAs(tempPath);
            this.voiceRecorderState.EffectedFileName = tempPath;
            this.ViewManager.MoveTo("SaveView", this.voiceRecorderState);
        }

        private void SaveAs(string fileName)
        {
            AudioSaver saver = new AudioSaver(voiceRecorderState.ActiveFile);
            saver.SaveFileFormat = SaveFileFormat.Wav;
            saver.AutoTuneSettings = this.voiceRecorderState.AutoTuneSettings;

            saver.SaveAudio(fileName);
        }

        public override void OnViewActivated(object state)
        {
            this.voiceRecorderState = (VoiceRecorderState)state;
            this.IsSnapMode = this.voiceRecorderState.AutoTuneSettings.SnapMode;
            this.IsAutoTuneEnabled = true; // coming into this view turns on autotune
            this.AttackTime = (int)this.voiceRecorderState.AutoTuneSettings.AttackTimeMilliseconds;
            foreach(var viewModelPitch in this.Pitches)
            {
                viewModelPitch.Selected = false;
            }
            foreach(var pitch in voiceRecorderState.AutoTuneSettings.AutoPitches)
            {
                this.Pitches.First(p => p.Note == pitch).Selected = true;
            }
        }

        public override void OnViewDeactivated(bool shuttingDown)
        {
            if (shuttingDown)
            {
                this.voiceRecorderState.DeleteFiles();
            }
            base.OnViewDeactivated(shuttingDown);
        }

        private void Cancel()
        {
            //this.voiceRecorderState.EffectedFileName = null;
            this.ViewManager.MoveTo("SaveView", voiceRecorderState);
        }

        public void Dispose()
        {
        }
    }
}
