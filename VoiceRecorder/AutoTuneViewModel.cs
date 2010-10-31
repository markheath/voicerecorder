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

        private bool isSnapMode;
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
        private VoiceRecorderState activatedArgs;

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
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");
            // TODO: onto a background thread
            SaveAs(tempPath);

            this.ViewManager.MoveTo("SaveView", new VoiceRecorderState(this.activatedArgs.RecordingFileName, tempPath));
        }

        private void SaveAs(string fileName)
        {
            AudioSaver saver = new AudioSaver(activatedArgs.ActiveFile);
            saver.SaveFileFormat = SaveFileFormat.Wav;
            saver.AutoTuneSettings.Enabled = true;
            saver.AutoTuneSettings.SnapMode = IsSnapMode;
            saver.AutoTuneSettings.AttackTimeMilliseconds = this.AttackTime;
            saver.AutoTuneSettings.AutoPitches.Clear();
            foreach(var pitch in this.Pitches)
            {
                if (pitch.Selected)
                {
                    saver.AutoTuneSettings.AutoPitches.Add(pitch.Note);
                }
            }
            saver.SaveAudio(fileName);
        }

        public override void OnViewActivated(object state)
        {
            this.activatedArgs = (VoiceRecorderState)state;
        }

        public override void OnViewDeactivated(bool shuttingDown)
        {
            if (shuttingDown)
            {
                this.activatedArgs.DeleteFiles();
            }
            base.OnViewDeactivated(shuttingDown);
        }

        private void Cancel()
        {
            // TODO: delete autotune file if necessary
            this.ViewManager.MoveTo("SaveView", new VoiceRecorderState(this.activatedArgs.RecordingFileName, null));
        }

        public void Dispose()
        {
        }
    }
}
