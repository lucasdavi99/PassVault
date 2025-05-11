using System.Globalization;

namespace PassVault.Converters
{
    internal class TabVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string selectedTab && parameter is string targetTab)
            {
                if (targetType == typeof(bool))
                {
                    return selectedTab == targetTab;
                }
                else // Para cores
                {
                    return selectedTab == targetTab ? Colors.White : Colors.Gray;
                }
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}