using CSCore.DSP;
using Microsoft.Graphics.Canvas;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace WinformsVisualization.Visualization
{
    public class VoicePrint3DSpectrum : SpectrumBase
    {
        private readonly GradientCalculator _colorCalculator;
        private bool _isInitialized;

        public VoicePrint3DSpectrum(FftSize fftSize)
        {
            _colorCalculator = new GradientCalculator();
            Colors = new[] { Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 0, 0, 255), Color.FromArgb(255, 0, 255, 255), Color.FromArgb(255, 0, 255, 150), Color.FromArgb(255, 0, 255, 0), Color.FromArgb(255, 155, 255, 0), Color.FromArgb(255, 255, 50, 0) };

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

        public int PointCount
        {
            get { return SpectrumResolution; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");
                SpectrumResolution = value;

                UpdateFrequencyMapping();
            }
        }

        public bool DrawVoicePrint3D(Size size, CanvasDrawingSession drawingSession, float xPos,
            float lineThickness = 1f, float multiplier = 1)
        {
            if (!_isInitialized)
            {
                UpdateFrequencyMapping();
                _isInitialized = true;
            }

            var fftBuffer = new float[(int)FftSize];

            //get the fft result from the spectrumprovider
            if (SpectrumProvider.GetFftData(fftBuffer, this))
            {
                //prepare the fft result for rendering
                SpectrumPointData[] spectrumPoints = CalculateSpectrumPoints(1.0, fftBuffer);

                float currentYOffset = (float)size.Height;
                var p3 = new Vector2(xPos + lineThickness, 0);
                var p4 = new Vector2(xPos + lineThickness, (float)size.Height);
                drawingSession.DrawLine(p3, p4, Color.FromArgb(255, 255, 255, 255), lineThickness);

                //render the fft result
                for (int i = 0; i < spectrumPoints.Length; i++)
                {
                    SpectrumPointData p = spectrumPoints[i];

                    float xCoord = xPos;
                    float pointHeight = (float)(size.Height / spectrumPoints.Length);

                    //get the color based on the fft band value
                    var c = _colorCalculator.GetColor((float)(p.Value * p.Value * multiplier));

                    var p1 = new Vector2(xCoord, currentYOffset);
                    var p2 = new Vector2(xCoord, currentYOffset - pointHeight);




                    drawingSession.DrawLine(p1, p2, c, lineThickness);

                    currentYOffset -= pointHeight;

                }
                return true;
            }
            return false;
        }
    }
}
