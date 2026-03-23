using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PomodoroWidget.Converters;

/// <summary>
/// Converts an angle (0-360) to a PathGeometry arc for the circular timer.
/// </summary>
public class AngleToArcConverter : IValueConverter
{
    private const double Radius = 54;
    private const double CenterX = 60;
    private const double CenterY = 60;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double angle = (double)value;
        if (angle <= 0) return Geometry.Empty;
        if (angle >= 360) angle = 359.99;

        double radians = (angle - 90) * Math.PI / 180;
        double endX = CenterX + Radius * Math.Cos(radians);
        double endY = CenterY + Radius * Math.Sin(radians);

        bool isLargeArc = angle > 180;

        var fig = new PathFigure
        {
            StartPoint = new Point(CenterX, CenterY - Radius), // top center
            IsClosed = false
        };
        fig.Segments.Add(new ArcSegment
        {
            Point = new Point(endX, endY),
            Size = new Size(Radius, Radius),
            IsLargeArc = isLargeArc,
            SweepDirection = SweepDirection.Clockwise
        });

        var geo = new PathGeometry();
        geo.Figures.Add(fig);
        return geo;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
