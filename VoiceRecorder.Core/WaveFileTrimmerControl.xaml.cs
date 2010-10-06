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
using VoiceRecorder.Audio;

namespace VoiceRecorder.Core
{
    /// <summary>
    /// Interaction logic for WaveFileTrimmerControl.xaml
    /// </summary>
    public partial class WaveFileTrimmerControl : UserControl
    {
        public static readonly DependencyProperty SampleAggregatorProperty = DependencyProperty.Register(
            "SampleAggregator", typeof(SampleAggregator), typeof(WaveFileTrimmerControl), new PropertyMetadata(null, OnSampleAggregatorChanged));

        public static readonly DependencyProperty TotalWaveFormSamplesProperty = DependencyProperty.Register(
            "TotalWaveFormSamples", typeof(int), typeof(WaveFileTrimmerControl), new PropertyMetadata(0, OnNotificationCountChanged));

        public static readonly DependencyProperty LeftSelectionProperty = DependencyProperty.Register(
            "LeftSelection", typeof(int), typeof(WaveFileTrimmerControl), new PropertyMetadata(0, OnLeftSelectionChanged));

        public static readonly DependencyProperty RightSelectionProperty = DependencyProperty.Register(
            "RightSelection", typeof(int), typeof(WaveFileTrimmerControl), new PropertyMetadata(0, OnRightSelectionChanged));


        public WaveFileTrimmerControl()
        {
            InitializeComponent();
            rangeSelection.SelectionChanged += new EventHandler(rangeSelection_SelectionChanged);
        }

        void rangeSelection_SelectionChanged(object sender, EventArgs e)
        {
            this.LeftSelection = (int)(rangeSelection.LeftPos / waveFormRenderer.XSpacing);
            this.RightSelection = (int)(rangeSelection.RightPos / waveFormRenderer.XSpacing);
        }

        public SampleAggregator SampleAggregator
        {
            get { return (SampleAggregator)this.GetValue(SampleAggregatorProperty); }
            set { this.SetValue(SampleAggregatorProperty, value); }
        }

        public int TotalWaveFormSamples
        {
            get { return (int)this.GetValue(TotalWaveFormSamplesProperty); }
            set { this.SetValue(TotalWaveFormSamplesProperty, value); }
        }

        public int LeftSelection
        {
            get { return (int)this.GetValue(LeftSelectionProperty); }
            set { this.SetValue(LeftSelectionProperty, value); }
        }

        public int RightSelection
        {
            get { return (int)this.GetValue(RightSelectionProperty); }
            set { this.SetValue(RightSelectionProperty, value); }
        }        

        private static void OnSampleAggregatorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            WaveFileTrimmerControl control = (WaveFileTrimmerControl)sender;
            control.Subscribe();
        }

        private static void OnNotificationCountChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            WaveFileTrimmerControl control = (WaveFileTrimmerControl)sender;
            control.SetWidth();
        }

        private static void OnLeftSelectionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            WaveFileTrimmerControl control = (WaveFileTrimmerControl)sender;
            control.rangeSelection.LeftPos = control.LeftSelection * control.waveFormRenderer.XSpacing;
        }

        private static void OnRightSelectionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            WaveFileTrimmerControl control = (WaveFileTrimmerControl)sender;
            control.rangeSelection.RightPos = control.RightSelection * control.waveFormRenderer.XSpacing;            
        }

        private void SetWidth()
        {
            waveFormRenderer.Width = TotalWaveFormSamples * waveFormRenderer.XSpacing;
            rangeSelection.Width = TotalWaveFormSamples * waveFormRenderer.XSpacing;
            //rangeSelection.SelectAll();            
        }

        private void Subscribe()
        {
            SampleAggregator.MaximumCalculated += SampleAggregator_MaximumCalculated;
            SampleAggregator.Restart += new EventHandler(SampleAggregator_Restart);
        }

        void SampleAggregator_Restart(object sender, EventArgs e)
        {
            this.waveFormRenderer.Reset();
        }

        void SampleAggregator_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            waveFormRenderer.AddValue(e.MaxSample, e.MinSample);
        }
    }
}
