using System;
using System.Collections.Generic;
using Windows.UI;

public class GradientCalculator
{
    private Color[] _lookupTable;
    private int _tableSize;
    private int _maxIndex;
    private Color[] _colors;

    public GradientCalculator()
    {
        Colors = new List<Color> { Color.FromArgb(1, 0, 0, 0), Color.FromArgb(1, 1, 1, 1) }.ToArray(); // Default gradient
    }

    public GradientCalculator(Color[] colors)
    {
        Colors = colors;
    }

    public Color[] Colors
    {
        get => _colors;
        set
        {
            _colors = value;
            _maxIndex = _colors.Length - 1;
            PrecomputeLookupTable();
        }
    }

    private void PrecomputeLookupTable(int resolution = 512)
    {
        _tableSize = resolution;
        _lookupTable = new Color[_tableSize];

        for (int i = 0; i < _tableSize; i++)
        {
            float perc = (float)i / (_tableSize - 1);
            float scaledPerc = perc * _maxIndex;
            int index = (int)scaledPerc;
            float blendFactor = scaledPerc - index;

            if (index >= _maxIndex) index = _maxIndex - 1;

            Color c1 = _colors[index];
            Color c2 = _colors[index + 1];

            _lookupTable[i] = Color.FromArgb(
                (byte)(c1.A + (c2.A - c1.A) * blendFactor),
                (byte)(c1.R + (c2.R - c1.R) * blendFactor),
                (byte)(c1.G + (c2.G - c1.G) * blendFactor),
                (byte)(c1.B + (c2.B - c1.B) * blendFactor));
        }
    }

    public Color GetColor(float percentage)
    {
        int index = (int)(percentage * (_tableSize - 1));
        return _lookupTable[Math.Clamp(index, 0, _tableSize - 1)];
    }
}
