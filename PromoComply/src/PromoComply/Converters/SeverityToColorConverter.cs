using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PromoComply.Models;

namespace PromoComply.Converters;

public class SeverityToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        if (value is IssueSeverity severity)
        {
            return severity switch
            {
                IssueSeverity.Critical => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C1292E")),
                IssueSeverity.Major => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8871E")),
                IssueSeverity.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4A460")),
                IssueSeverity.Info => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5FA8D3")),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        throw new NotImplementedException();
    }
}
