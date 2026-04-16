using CSCore.DSP;
using Microsoft.Graphics.Canvas;
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using TagLib.Ape;
using Windows.Foundation;
using Windows.UI;

namespace WinformsVisualization.Visualization
{
    public class LineSpectrum : SpectrumBase
    {
        private int _barCount;
        private double _barSpacing;
        private double _barWidth;
        private Size _currentSize;
        private readonly GradientCalculator _colorCalculator;

        public LineSpectrum(FftSize fftSize)
        {
            ;
            _colorCalculator = new GradientCalculator();
            //var c = new[] { Color.FromArgb(255, 25, 185, 20), Color.FromArgb(255, 15, 125, 10), Color.FromArgb(255, 180, 107, 20), Color.FromArgb(255, 255, 0, 0) };
            var c = new[] { Color.FromArgb(255, 200, 200, 255), Color.FromArgb(255, 0, 0, 255), Color.FromArgb(255, 0, 255, 255), Color.FromArgb(255, 0, 255, 150), Color.FromArgb(255, 0, 255, 0), Color.FromArgb(255, 155, 255, 0), Color.FromArgb(255, 255, 50, 0) };

            c.Reverse();
            Colors = c;
            FftSize = fftSize;
        }

        public Color[] Colors
        {
            get { return _colorCalculator.Colors; }
            set
            {
                if (value == null || value.Length <= 0)
                    throw new ArgumentException("value");

                _colorCalculator.Colors = value;
            }
        }


        [Browsable(false)]
        public double BarWidth
        {
            get { return _barWidth; }
        }

        public double BarSpacing
        {
            get { return _barSpacing; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");
                _barSpacing = value;
                UpdateFrequencyMapping();

                RaisePropertyChanged("BarSpacing");
                RaisePropertyChanged("BarWidth");
            }
        }

        public int BarCount
        {
            get { return _barCount; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");
                _barCount = value;
                SpectrumResolution = value;
                UpdateFrequencyMapping();

                RaisePropertyChanged("BarCount");
                RaisePropertyChanged("BarWidth");
            }
        }

        [BrowsableAttribute(false)]
        public Size CurrentSize
        {
            get { return _currentSize; }
            protected set
            {
                _currentSize = value;
                RaisePropertyChanged("CurrentSize");
            }
        }

        float[] lastBuffer;
        public double[] DrawSpectrumLine(Size size, CanvasDrawingSession drawingSession, float multiplier = 1)
        {
            if (!UpdateFrequencyMappingIfNessesary(size))
                return null;
            var fftBuffer = new float[(int)FftSize];
            // get the fft result from the spectrum provider
            if (SpectrumProvider.GetFftData(fftBuffer, this))
            {
                lastBuffer = fftBuffer;
                drawingSession.Clear(Color.FromArgb(0, 1, 1, 1));
                return CreateSpectrumLineInternal(drawingSession, fftBuffer, size, multiplier);
            }
            else
            {
                drawingSession.Clear(Color.FromArgb(1, 1, 0, 0));
                return CreateSpectrumLineInternal(drawingSession, lastBuffer, size, multiplier);

            }
        }

        private double[] CreateSpectrumLineInternal(CanvasDrawingSession drawingSession, float[] fftBuffer, Size size, float multiplier = 1)
        {
            int height = (int)size.Height;
            //prepare the fft result for rendering 
            SpectrumPointData[] spectrumPoints = CalculateSpectrumPoints(height, fftBuffer);

            //connect the calculated points with lines
            for (int i = 0; i < spectrumPoints.Length; i++)
            {
                SpectrumPointData p = spectrumPoints[i];
                int barIndex = p.SpectrumPointIndex;
                double xCoord = BarSpacing * (barIndex + 1) + (_barWidth * barIndex) + _barWidth / 2;

                var p1 = new Vector2((float)xCoord, height);
                var p2 = new Vector2((float)xCoord, height - (float)p.Value * multiplier - 1);

                var c = _colorCalculator.GetColor((float)(p.Value) * .01f);
                drawingSession.DrawLine(p1, p2, c, (float)_barWidth);
            }
            return [.. spectrumPoints.Select(item => item.Value)];
        }

        protected override void UpdateFrequencyMapping()
        {
            _barWidth = Math.Max(((_currentSize.Width - (BarSpacing * (BarCount + 1))) / BarCount), 0.00001);
            base.UpdateFrequencyMapping();
        }

        private bool UpdateFrequencyMappingIfNessesary(Size newSize)
        {
            if (newSize != CurrentSize)
            {
                CurrentSize = newSize;
                UpdateFrequencyMapping();
            }

            return newSize.Width > 0 && newSize.Height > 0;
        }
    }
}
