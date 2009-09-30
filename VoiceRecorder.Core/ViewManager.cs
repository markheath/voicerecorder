using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace VoiceRecorder.Core
{
    public class ViewManager : IViewManager
    {
        Dictionary<string, FrameworkElement> views;
        FrameworkElement currentView;
        string currentViewName;

        public event EventHandler<ViewChangedEventArgs> ViewChanged = delegate { };

        public ViewManager()
        {
            views = new Dictionary<string, FrameworkElement>();
        }

        public void AddView(string viewName, FrameworkElement view)
        {
            views.Add(viewName, view);

            // make the first one we add the default
            if (currentView == null)
            {
                currentView = view;
                currentViewName = viewName;
            }
        }

        public FrameworkElement Current
        {
            get
            {
                return currentView;
            }
        }

        public string CurrentViewName
        {
            get
            {
                return currentViewName;
            }
        }

        private void SetCurrentView(string name, FrameworkElement view, object state)
        {
            if (currentView != view)
            {
                var args = new ViewChangedEventArgs();
                args.OldView = currentView;
                args.OldViewName = currentViewName;
                args.NewView = view;
                args.NewViewName = name;
                args.State = state;

                this.currentView = view;
                this.currentViewName = name;

                ViewChanged(this, args);
            }
        }

        public void MoveTo(string viewName, object state)
        {
            SetCurrentView(viewName, views[viewName], state);            
        }
    }

}
