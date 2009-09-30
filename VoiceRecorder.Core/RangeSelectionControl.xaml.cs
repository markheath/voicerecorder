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

namespace VoiceRecorder.Core
{
    /// <summary>
    /// Interaction logic for RangeSelectionControl.xaml
    /// </summary>
    public partial class RangeSelectionControl : UserControl
    {
        private Edge dragEdge;
        private double leftPos;
        private double rightPos;
        private double minSelectionWidth = 20;

        public event EventHandler SelectionChanged = delegate { };

        enum Edge
        {
            None,
            Left,
            Right
        }

        public double LeftPos
        {
            get
            {
                return leftPos;
            }
            set
            {
                if (leftPos != value)
                {
                    leftPos = value;
                    highlightRect.SetValue(Canvas.LeftProperty, value);
                    UpdateRightHandPosition(rightPos); // keep right hand edge where it was
                    SelectionChanged(this, EventArgs.Empty);
                }
            }
        }

        public double RightPos
        {
            get
            {
                return rightPos;
            }
            set
            {
                if (rightPos != value)
                {
                    UpdateRightHandPosition(value);
                    SelectionChanged(this, EventArgs.Empty);
                }
            }
        }

        private void UpdateRightHandPosition(double value)
        {
            rightPos = value;
            highlightRect.Width = value - LeftPos;
        }

        Edge DragEdge
        {
            get
            {
                return dragEdge;
            }
            set
            {
                if (dragEdge != value)
                {
                    dragEdge = value;
                    if (dragEdge == Edge.None)
                    {
                        Cursor = Cursors.Arrow;
                    }
                }
            }
        }

        public RangeSelectionControl()
        {
            InitializeComponent();
            mainCanvas.MouseDown += new MouseButtonEventHandler(RangeSelectionControl_MouseDown);
            mainCanvas.MouseMove += new MouseEventHandler(RangeSelectionControl_MouseMove);
            mainCanvas.MouseLeave += new MouseEventHandler(RangeSelectionControl_MouseLeave);
            mainCanvas.MouseUp += new MouseButtonEventHandler(RangeSelectionControl_MouseUp);
            this.SizeChanged += new SizeChangedEventHandler(RangeSelectionControl_SizeChanged);
        }

        void RangeSelectionControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mainCanvas.IsMouseCaptured)
                mainCanvas.ReleaseMouseCapture();

            DragEdge = Edge.None;
        }

        void RangeSelectionControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            highlightRect.Height = e.NewSize.Height;
        }

        void RangeSelectionControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //DragEdge = Edge.None;
        }

        void RangeSelectionControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(this);

            if (dragEdge == Edge.None)
            {
                if (EdgeAtPosition(position.X) != Edge.None)
                {
                    Cursor = Cursors.SizeWE;
                }
                else
                {
                    Cursor = Cursors.Arrow;
                }
            }
            else if (dragEdge == Edge.Left)
            {
                if (position.X < 0)
                {
                    LeftPos = 0;
                }
                else if (position.X < RightPos - minSelectionWidth)
                {
                    LeftPos = position.X;
                }
            }
            else if (dragEdge == Edge.Right)
            {
                if (position.X > this.ActualWidth)
                {
                    RightPos = this.ActualWidth;
                }
                else if (position.X > LeftPos + minSelectionWidth)
                {
                    RightPos = position.X;
                }
            }
        }

        void RangeSelectionControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(this);
                Edge edge = EdgeAtPosition(position.X);
                DragEdge = edge;
                if (DragEdge != Edge.None)
                {
                    mainCanvas.CaptureMouse();
                }
            }
        }

        private Edge EdgeAtPosition(double X)
        {
            double tolerance = 2;
            if (X >= (leftPos - tolerance) && X <= (leftPos + tolerance))
            {
                return Edge.Left;
            }
            if (X >= (rightPos - tolerance) && X <= (rightPos + tolerance))
            {
                return Edge.Right;
            }
            return Edge.None;
        }

        public void SelectAll()
        {
            LeftPos = 0;
            RightPos = Width;
        }
    }
}