using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PromoComply.Converters;

public class ScoreToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        if (value is int score)
        {
            return score switch
            {
                >= 90 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E8B57")),
                >= 75 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5FA8D3")),
                >= 60 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8871E")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C1292E"))
            };
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        throw new NotImplementedException();
    }
}
