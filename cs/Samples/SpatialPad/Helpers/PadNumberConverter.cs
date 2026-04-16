using System;
using Microsoft.UI.Xaml.Data;

namespace Volumetric.Samples.SpatialPad.Helpers
{
    public class PadNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int id)
                return $"Pad {id + 1}";

            return "Pad ?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
