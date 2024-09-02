using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace MexManager.Converters
{
    public class ValidationColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Brushes.Transparent : Brushes.DarkRed;
            }
            return Brushes.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
