using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoiceRecorder.Core;
using System.Windows.Input;
using System.Collections.ObjectModel;
using VoiceRecorder.Audio;

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
        private SaveViewActivatedArgs activatedArgs;

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

        }

        public override void OnViewActivated(object state)
        {
            this.activatedArgs = (SaveViewActivatedArgs)state;
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
            this.ViewManager.MoveTo("SaveView", new SaveViewActivatedArgs(this.activatedArgs.RecordingFileName, null));
        }

        public void Dispose()
        {
        }
    }
}
