using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoiceRecorder.Core;
using System.Windows;
using VoiceRecorder.Audio;

namespace VoiceRecorder
{
    class MainWindowViewModel : ViewModelBase, IDisposable
    {
        IViewManager viewManager;
        Dictionary<string, ViewModelBase> viewModels;

        public MainWindowViewModel()
        {
            viewManager = new ViewManager();
            viewModels = new Dictionary<string, ViewModelBase>();
            
            SetupView("WelcomeView", new WelcomeView(), new WelcomeViewModel(viewManager));
            SetupView("RecorderView", new RecorderView(), new RecorderViewModel(new AudioRecorder(), viewManager));
            SetupView("SaveView", new SaveView(), new SaveViewModel(new AudioPlayer()));

            viewManager.ViewChanged += viewManager_ViewChanged;
        }

        private void SetupView(string viewName, FrameworkElement view, ViewModelBase viewModel)
        {
            view.DataContext = viewModel;
            viewManager.AddView(viewName, view);
            viewModels.Add(viewName, viewModel);            
        }

        void viewManager_ViewChanged(object sender, ViewChangedEventArgs e)
        {
            var oldViewModel = viewModels[e.OldViewName];
            oldViewModel.OnViewDeactivated(false);
            RaisePropertyChangedEvent("CurrentView");
            var viewModel = viewModels[e.NewViewName];
            viewModel.OnViewActivated(e.State);
        }

        public object CurrentView
        {
            get
            {
                return viewManager.Current;
            }
        }

        /// <summary>
        /// Allow the current view to clean itself up when we close
        /// </summary>
        public void Dispose()
        {
            var viewModel = viewModels[viewManager.CurrentViewName];
            viewModel.OnViewDeactivated(true);
        }
    }
}
