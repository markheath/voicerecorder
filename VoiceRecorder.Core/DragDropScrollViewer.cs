using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;

// Code from Dan Crevier
// http://blogs.msdn.com/llobo/archive/2006/09/06/Scrolling-Scrollviewer-on-Mouse-Drag-at-the-boundaries.aspx
// Modified by Mark Heath for left-right scrolling instead of up-down

namespace VoiceRecorder.Core
{
    public class MouseUtilities
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hwnd, ref Win32Point pt);

        public static Point GetMousePosition(Visual relativeTo)
        {
            Win32Point mouse = new Win32Point();
            GetCursorPos(ref mouse);

            System.Windows.Interop.HwndSource presentationSource =
                (System.Windows.Interop.HwndSource)PresentationSource.FromVisual(relativeTo);

            ScreenToClient(presentationSource.Handle, ref mouse);

            GeneralTransform transform = relativeTo.TransformToAncestor(presentationSource.RootVisual);

            Point offset = transform.Transform(new Point(0, 0));

            return new Point(mouse.X - offset.X, mouse.Y - offset.Y);
        }


    };

    public class DragDropScrollViewer : ScrollViewer
    {
        protected override void OnPreviewQueryContinueDrag(QueryContinueDragEventArgs args)
        {
            base.OnPreviewQueryContinueDrag(args);

            if (args.Action == DragAction.Cancel || args.Action == DragAction.Drop)
            {
                CancelDrag();
            }
            else if (args.Action == DragAction.Continue)
            {
                Point p = MouseUtilities.GetMousePosition(this);
                if ((p.X < s_dragMargin) || (p.X > RenderSize.Width - s_dragMargin))
                {
                    if (_dragScrollTimer == null)
                    {
                        _dragVelocity = s_dragInitialVelocity;
                        _dragScrollTimer = new DispatcherTimer();
                        _dragScrollTimer.Tick += TickDragScroll;
                        _dragScrollTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)s_dragInterval);
                        _dragScrollTimer.Start();
                    }
                }
            }
        }

        private void TickDragScroll(object sender, EventArgs e)
        {
            bool isDone = true;

            if (this.IsLoaded)
            {
                Rect bounds = new Rect(RenderSize);
                Point p = MouseUtilities.GetMousePosition(this);
                if (bounds.Contains(p))
                {
                    if (p.X < s_dragMargin)
                    {
                        DragScroll(DragDirection.Left);
                        isDone = false;
                    }
                    else if (p.X > RenderSize.Width - s_dragMargin)
                    {
                        DragScroll(DragDirection.Right);
                        isDone = false;
                    }
                }
            }

            if (isDone)
            {
                CancelDrag();
            }
        }

        private void CancelDrag()
        {
            if (_dragScrollTimer != null)
            {
                _dragScrollTimer.Tick -= TickDragScroll;
                _dragScrollTimer.Stop();
                _dragScrollTimer = null;
            }
        }

        private enum DragDirection
        {
            Left,
            Right
        };

        private void DragScroll(DragDirection direction)
        {
            bool isLeft = (direction == DragDirection.Left);
            double offset = Math.Max(0.0, HorizontalOffset + (isLeft ? -(_dragVelocity * s_dragInterval) : (_dragVelocity * s_dragInterval)));
            ScrollToHorizontalOffset(offset);
            _dragVelocity = Math.Min(s_dragMaxVelocity, _dragVelocity + (s_dragAcceleration * s_dragInterval));
        }

        private static readonly double s_dragInterval = 10; // milliseconds
        private static readonly double s_dragAcceleration = 0.0005; // pixels per millisecond^2
        private static readonly double s_dragMaxVelocity = 2.0; // pixels per millisecond
        private static readonly double s_dragInitialVelocity = 0.05; // pixels per millisecond
        private static double s_dragMargin = 40.0;
        private DispatcherTimer _dragScrollTimer = null;
        private double _dragVelocity;
    }
}

