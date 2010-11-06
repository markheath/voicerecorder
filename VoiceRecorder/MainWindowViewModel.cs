using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoiceRecorder.Core;
using System.Windows;
using VoiceRecorder.Audio;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace VoiceRecorder
{
    class MainWindowViewModel : ViewModelBase
    {
        Dictionary<string, FrameworkElement> views;
        private FrameworkElement currentView;
        private string currentViewName;

        public MainWindowViewModel()
        {
            Messenger.Default.Register<NavigateMessage>(this, (message) => OnNavigate(message));
            views = new Dictionary<string, FrameworkElement>();
            
            SetupView(WelcomeViewModel.ViewName, new WelcomeView(), new WelcomeViewModel());
            SetupView(RecorderViewModel.ViewName, new RecorderView(), new RecorderViewModel(new AudioRecorder()));
            SetupView(SaveViewModel.ViewName, new SaveView(), new SaveViewModel(new AudioPlayer()));
            SetupView(AutoTuneViewModel.ViewName, new AutoTuneView(), new AutoTuneViewModel());

            Messenger.Default.Send<NavigateMessage>(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

        private void OnNavigate(NavigateMessage message) 
        {
            this.CurrentView = views[message.TargetView];
            this.currentViewName = message.TargetView;
            ((IView)this.CurrentView.DataContext).Activated(message.State);
        }

        private void SetupView(string viewName, FrameworkElement view, ViewModelBase viewModel)
        {
            view.DataContext = viewModel;
            views.Add(viewName, view);
        }

        public FrameworkElement CurrentView
        {
            get
            {
                return currentView;
            }
            set
            {
                if (this.currentView != value)
                {
                    currentView = value;
                    RaisePropertyChanged("CurrentView");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            Messenger.Default.Send(new ShuttingDownMessage(currentViewName));
            ((IDisposable)CurrentView.DataContext).Dispose();
            base.Dispose(disposing);
        }
    }
}
