using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoiceRecorder.Core;
using System.Windows.Input;
using System.Collections.ObjectModel;
using VoiceRecorder.Audio;
using System.IO;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using System.Threading;
using GalaSoft.MvvmLight.Threading;

namespace VoiceRecorder
{
    class AutoTuneViewModel : ViewModelBase, IView
    {
        public ICommand ApplyCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private int attackTimeMilliseconds;
        private bool isAutoTuneEnabled;
        private VoiceRecorderState voiceRecorderState;
        public const string ViewName = "AutoTuneView";
        private bool isEnabled;

        public int AttackTime
        {
            get { return attackTimeMilliseconds; }
            set
            {
                if (attackTimeMilliseconds != value)
                {
                    attackTimeMilliseconds = value;
                    RaisePropertyChanged("AttackTime");
                    RaisePropertyChanged("AttackMessage");
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    RaisePropertyChanged("IsEnabled");
                    RaisePropertyChanged("ProcessingMessageVisibility");
                }
            }
        }

        public Visibility ProcessingMessageVisibility
        {
            get
            {
                return isEnabled ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public string AttackMessage
        {
            get { return String.Format("{0}ms", attackTimeMilliseconds); }
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
                    RaisePropertyChanged("IsAutoTuneEnabled");
                }
            }
        }

        public ObservableCollection<NoteViewModel> Pitches { get; private set; }
        public IEnumerable<string> Scales { get { return scalesDictionary.Keys; } }
        private string selectedScale;
        public string SelectedScale
        {
            get { return selectedScale; }
            set
            {
                if (this.selectedScale != value)
                {
                    this.selectedScale = value;
                    SelectNotes();
                }
            }
        }
        private Dictionary<string, HashSet<Note>> scalesDictionary;

        private void SelectNotes()
        {
            var scale = scalesDictionary[selectedScale];
            foreach (var p in Pitches)
            {
                p.Selected = scale.Contains(p.Note);
            }
        }

        public AutoTuneViewModel()
        {
            this.ApplyCommand = new RelayCommand(() => Apply());
            this.CancelCommand = new RelayCommand(() => Cancel());
            scalesDictionary = new Dictionary<string, HashSet<Note>>();
            scalesDictionary.Add("Chromatic", PredefinedScales.Chromatic);
            scalesDictionary.Add("Key of C / Am", PredefinedScales.MakeMajorScale(Note.C));
            scalesDictionary.Add("Key of D / Bm", PredefinedScales.MakeMajorScale(Note.D));
            scalesDictionary.Add("Key of E / C\u266Fm", PredefinedScales.MakeMajorScale(Note.E));
            scalesDictionary.Add("Key of F / Dm", PredefinedScales.MakeMajorScale(Note.F));
            scalesDictionary.Add("Key of G / Em", PredefinedScales.MakeMajorScale(Note.G));
            scalesDictionary.Add("Key of A / F\u266Fm", PredefinedScales.MakeMajorScale(Note.A));
            scalesDictionary.Add("Key of B\u266D / Gm", PredefinedScales.MakeMajorScale(Note.ASharp));
            
            scalesDictionary.Add("Pentatonic C / Am",       PredefinedScales.MakePentatonicScale(Note.C));
            scalesDictionary.Add("Pentatonic D / Bm",       PredefinedScales.MakePentatonicScale(Note.D));
            scalesDictionary.Add("Pentatonic E / C\u266Fm", PredefinedScales.MakePentatonicScale(Note.E));
            scalesDictionary.Add("Pentatonic F / Dm",       PredefinedScales.MakePentatonicScale(Note.F));
            scalesDictionary.Add("Pentatonic G / Em",       PredefinedScales.MakePentatonicScale(Note.G));
            scalesDictionary.Add("Pentatonic A / F\u266Fm", PredefinedScales.MakePentatonicScale(Note.A));
            scalesDictionary.Add("Pentatonic B\u266D / Gm", PredefinedScales.MakePentatonicScale(Note.ASharp));
            
            
            this.Pitches = new ObservableCollection<NoteViewModel>();

            this.Pitches.Add(new NoteViewModel(Note.C,"C"));
            this.Pitches.Add(new NoteViewModel(Note.CSharp,"C\u266F"));
            this.Pitches.Add(new NoteViewModel(Note.D,"D"));
            this.Pitches.Add(new NoteViewModel(Note.DSharp, "E\u266D"));
            this.Pitches.Add(new NoteViewModel(Note.E,"E"));
            this.Pitches.Add(new NoteViewModel(Note.F, "F"));
            this.Pitches.Add(new NoteViewModel(Note.FSharp, "F\u266F"));
            this.Pitches.Add(new NoteViewModel(Note.G,"G"));
            this.Pitches.Add(new NoteViewModel(Note.GSharp, "A\u266D"));
            this.Pitches.Add(new NoteViewModel(Note.A,"A"));
            this.Pitches.Add(new NoteViewModel(Note.ASharp,"B\u266D"));
            this.Pitches.Add(new NoteViewModel(Note.B,"B"));
            this.SelectedScale = "Chromatic";
            Messenger.Default.Register<ShuttingDownMessage>(this, (message) => OnShuttingDown(message));
        }

        private void Apply()
        {
            UpdateAutoTuneSettingsFromGui();
            if (voiceRecorderState.AutoTuneSettings.Enabled)
            {
                string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");
                IsEnabled = false;
                ThreadPool.QueueUserWorkItem((state) => SaveAs(tempPath));
            }
            else
            {
                NavigateToSaveView();
            }
        }

        private void NavigateToSaveView()
        {
            Messenger.Default.Send(new NavigateMessage(SaveViewModel.ViewName, this.voiceRecorderState));
        }

        private void UpdateAutoTuneSettingsFromGui()
        {
            voiceRecorderState.AutoTuneSettings.Enabled = IsAutoTuneEnabled;
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
        }

        private void SaveAs(string fileName)
        {

            AudioSaver saver = new AudioSaver(voiceRecorderState.ActiveFile);
            saver.SaveFileFormat = SaveFileFormat.Wav;
            saver.AutoTuneSettings = this.voiceRecorderState.AutoTuneSettings;

            saver.SaveAudio(fileName);

            this.voiceRecorderState.EffectedFileName = fileName;
            DispatcherHelper.CheckBeginInvokeOnUI(() => NavigateToSaveView());
        }

        public void Activated(object state)
        {
            this.voiceRecorderState = (VoiceRecorderState)state;
            this.IsEnabled = true;
            this.IsAutoTuneEnabled = true; // coming into this view turns on autotune
            this.AttackTime = (int)this.voiceRecorderState.AutoTuneSettings.AttackTimeMilliseconds;
            foreach (var viewModelPitch in this.Pitches)
            {
                viewModelPitch.Selected = false;
            }
            foreach (var pitch in voiceRecorderState.AutoTuneSettings.AutoPitches)
            {
                this.Pitches.First(p => p.Note == pitch).Selected = true;
            }
        }

        private void OnShuttingDown(ShuttingDownMessage message)
        {
            if (message.CurrentViewName == AutoTuneViewModel.ViewName)
            {
                this.voiceRecorderState.DeleteFiles();
            }
        }

        private void Cancel()
        {
            Messenger.Default.Send(new NavigateMessage(SaveViewModel.ViewName, voiceRecorderState));
        }
    }
}
