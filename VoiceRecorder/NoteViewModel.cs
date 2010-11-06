using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoiceRecorder.Audio;
using GalaSoft.MvvmLight;

namespace VoiceRecorder
{
    public class NoteViewModel : ViewModelBase
    {
        public NoteViewModel(Note note, string displayName)
        {
            this.Note = note;
            this.DisplayName = displayName;
        }

        public Note Note { get; set; }
        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected != value)
                {
                    selected = value;
                    RaisePropertyChanged("Selected");
                }
            }
        }
        public string DisplayName { get; set; }
    }
}
