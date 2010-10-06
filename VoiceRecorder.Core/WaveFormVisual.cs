using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace VoiceRecorder.Core
{
    public class WaveFormVisual : FrameworkElement, IWaveFormRenderer
    {
        private VisualCollection visualCollection;
        double yTranslate = 40;
        double yScale = 40;
        double xSpacing = 2;
        private List<float> maxValues;
        private List<float> minValues;

        public WaveFormVisual()
        {
            visualCollection = new VisualCollection(this);
            this.Reset();
            this.SizeChanged += new SizeChangedEventHandler(WaveFormVisual_SizeChanged);               
        }

        void WaveFormVisual_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double padding = e.NewSize.Height / 10; // 10 percent padding
            yScale = (e.NewSize.Height  - padding) / 2;
            yTranslate = padding + yScale;
            Redraw();
        }

        public double XSpacing
        {
            get
            {
                return xSpacing;
            }
        }

        private DrawingVisual CreateWaveFormVisual()
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            // Retrieve the DrawingContext in order to create new drawing content.
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            if (maxValues.Count > 0)
            {
                RenderPolygon(drawingContext);
            }

            // Persist the drawing content.
            drawingContext.Close();

            return drawingVisual;
        }

        private void RenderPolygon(DrawingContext drawingContext)
        {
            var fillBrush = Brushes.Bisque;
            var borderPen = new Pen(Brushes.Black, 1.0);

            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = CreatePoint(maxValues, 0);

            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();

            for (int i = 1; i < maxValues.Count; i++)
            {
                myPathSegmentCollection.Add(new LineSegment(CreatePoint(maxValues,i), true));
            }
            for (int i = minValues.Count - 1; i >= 0; i--)
            {
                myPathSegmentCollection.Add(new LineSegment(CreatePoint(minValues,i), true));
            }

            myPathFigure.Segments = myPathSegmentCollection;
            PathGeometry myPathGeometry = new PathGeometry();

            myPathGeometry.Figures.Add(myPathFigure);

            drawingContext.DrawGeometry(fillBrush, borderPen, myPathGeometry);
        }

        private Point CreatePoint(List<float> values, int xpos)
        {
            return new Point(xpos * xSpacing, SampleToYPosition(values[xpos]));
        }

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return visualCollection.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= visualCollection.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return visualCollection[index];
        }

        #region IWaveFormRenderer Members

        public void AddValue(float maxValue, float minValue)
        {
            maxValues.Add(maxValue);
            minValues.Add(minValue);
            Redraw();       
        }

        private void Redraw()
        {
            visualCollection.Clear();
            visualCollection.Add(CreateWaveFormVisual());
            this.InvalidateVisual();
        }

        private double SampleToYPosition(float value)
        {
            return yTranslate + value * yScale;
        }
        #endregion

        public void Reset()
        {
            maxValues = new List<float>();
            minValues = new List<float>();
            visualCollection.Clear();
            visualCollection.Add(CreateWaveFormVisual());
        }
    }
}
