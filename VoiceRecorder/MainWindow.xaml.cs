using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VoiceRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.Closed += new EventHandler(MainWindow_Closed);
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            IDisposable dataContext = this.DataContext as IDisposable;
            if (dataContext != null)
            {
                dataContext.Dispose();
            }
        }
    }
}
