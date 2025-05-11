using System.Globalization;

namespace PassVault.Converters
{
    internal class HexToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
            {
                return Color.FromArgb(hexColor);
            }
            return Colors.Purple;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return color.ToHex();
            }
            return "#800080";
        }
    }
}
