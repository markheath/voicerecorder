using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace VoiceRecorder.Core
{
    public interface IViewManager
    {
        event EventHandler<ViewChangedEventArgs> ViewChanged;

        FrameworkElement Current { get; }
        string CurrentViewName { get; }

        void AddView(string viewName, FrameworkElement view);

        void MoveTo(string viewName, object state);
    }

    public class ViewChangedEventArgs : EventArgs
    {
        public string OldViewName { get; set; }
        public string NewViewName { get; set; }
        public FrameworkElement OldView { get; set; }
        public FrameworkElement NewView { get; set; }
        public object State { get; set; }
    }

}
