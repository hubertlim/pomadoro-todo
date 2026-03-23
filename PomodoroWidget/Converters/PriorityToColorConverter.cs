using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PomodoroWidget.Models;

namespace PomodoroWidget.Converters;

public class PriorityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (Priority)value switch
        {
            Priority.Low => new SolidColorBrush(Color.FromRgb(34, 197, 94)),    // green
            Priority.Medium => new SolidColorBrush(Color.FromRgb(234, 179, 8)), // yellow
            Priority.High => new SolidColorBrush(Color.FromRgb(239, 68, 68)),   // red
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (bool)value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (bool)value ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
