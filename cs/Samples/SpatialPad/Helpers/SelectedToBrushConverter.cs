using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Volumetric.Samples.SpatialPad.Helpers
{
    public class SelectedToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isSelected = (bool)value;
            return isSelected ? new SolidColorBrush(Microsoft.UI.Colors.Black)
                              : new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
