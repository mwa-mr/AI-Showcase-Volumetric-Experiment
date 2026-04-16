using Microsoft.UI.Xaml.Data;
using System;

namespace Volumetric.Samples.SpatialPad.Helpers
{
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool b ? !b : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
