using System.Globalization;

namespace PassVault.Converters
{
    public class EditingModeToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isEditing)
            {
                return isEditing ? Colors.White : Color.FromArgb("5CD767");
            }
            return Colors.Grey;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
