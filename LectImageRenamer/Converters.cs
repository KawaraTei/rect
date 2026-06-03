using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LectImageRenamer;

public sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType == typeof(Visibility))
        {
            return value is bool booleanValue && booleanValue ? Visibility.Collapsed : Visibility.Visible;
        }

        return value is bool boolean && !boolean;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }

        return value is bool boolean && !boolean;
    }
}
