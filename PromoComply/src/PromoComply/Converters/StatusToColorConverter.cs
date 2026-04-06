using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PromoComply.Models;

namespace PromoComply.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        if (value is DocumentStatus status)
        {
            return status switch
            {
                DocumentStatus.Pending => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5FA8D3")),
                DocumentStatus.Reviewing => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8871E")),
                DocumentStatus.Reviewed => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5FA8D3")),
                DocumentStatus.Approved => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E8B57")),
                DocumentStatus.Flagged => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C1292E")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999"))
            };
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        throw new NotImplementedException();
    }
}
