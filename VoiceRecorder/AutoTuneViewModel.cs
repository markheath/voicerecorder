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
        public bool IsSnapMode { get; private set; }
        private bool anyNote;

        public bool IsSpecifiedNotes
        {
            get
            {
                return !this.anyNote;
            }
            set
            {
                this.anyNote = !value;
            }
        }
        public bool IsAnyNote
        {
            get
            {
                return this.anyNote;
            }
            set
            {
                if (this.anyNote != value)
                {
                    RaisePropertyChangedEvent("IsAnyNote");
                    RaisePropertyChangedEvent("IsSpecifiedNotes");
                }
            }
        }


        public ObservableCollection<NoteViewModel> SelectedNotes { get; private set; }

        public AutoTuneViewModel()
        {
            this.ApplyCommand = new RelayCommand(() => Apply());
            this.CancelCommand = new RelayCommand(() => Cancel());
            this.SelectedNotes = new ObservableCollection<NoteViewModel>();
            this.SelectedNotes.Add(new NoteViewModel(Notes.C,"C"));
            this.SelectedNotes.Add(new NoteViewModel(Notes.CSharp,"C#"));
            this.SelectedNotes.Add(new NoteViewModel(Notes.D,"D"));
            this.SelectedNotes.Add(new NoteViewModel(Notes.DSharp,"D#"));
            this.SelectedNotes.Add(new NoteViewModel(Notes.E,"E"));
            this.SelectedNotes.Add(new NoteViewModel(Notes.F, "F"));
            this.SelectedNotes.Add(new NoteViewModel(Notes.FSharp, "F#"));
            this.SelectedNotes.Add(new NoteViewModel(Notes.G,"G"));
            this.SelectedNotes.Add(new NoteViewModel(Notes.GSharp,"G#"));
            this.SelectedNotes.Add(new NoteViewModel(Notes.A,"A"));
            this.SelectedNotes.Add(new NoteViewModel(Notes.ASharp,"A#"));
            this.SelectedNotes.Add(new NoteViewModel(Notes.B,"B"));
        }

        private void Apply()
        {

        }

        private void Cancel()
        {
            this.ViewManager.MoveTo("SaveView", null);
        }

        public void Dispose()
        {
        }
    }
}
