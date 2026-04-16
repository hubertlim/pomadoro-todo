using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PomodoroWidget.Models;

namespace PomodoroWidget.Converters;

public class PriorityToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush LowBrush = FrozenBrush(34, 197, 94);
    private static readonly SolidColorBrush MediumBrush = FrozenBrush(245, 158, 11);
    private static readonly SolidColorBrush HighBrush = FrozenBrush(239, 68, 68);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (Priority)value switch
        {
            Priority.Low => LowBrush,
            Priority.Medium => MediumBrush,
            Priority.High => HighBrush,
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    private static SolidColorBrush FrozenBrush(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
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
