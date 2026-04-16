using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace CsMaterialExplorer
{
    public class TextureStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TextureLoadStatus status)
            {
                switch (status)
                {
                    case TextureLoadStatus.Success:
                        return "\uE73E"; // Checkmark icon
                    case TextureLoadStatus.Error:
                        return "\uE783"; // Error icon
                    case TextureLoadStatus.Loading:
                        return "\uE898"; // Loading icon
                    case TextureLoadStatus.None:
                    default:
                        return string.Empty; // No icon
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class TextureStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TextureLoadStatus status)
            {
                switch (status)
                {
                    case TextureLoadStatus.Success:
                        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 200, 0)); // Green
                    case TextureLoadStatus.Error:
                        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 0, 0)); // Red
                    case TextureLoadStatus.Loading:
                        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 200, 0)); // Yellow
                    case TextureLoadStatus.None:
                    default:
                        return new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)); // Transparent
                }
            }
            return new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
