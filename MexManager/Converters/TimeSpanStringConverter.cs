using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MexManager.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                // Convert TimeSpan to string
                return timeSpan.ToString(@"hh\:mm\:ss"); // Customize format as needed
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str && TimeSpan.TryParse(str, out TimeSpan timeSpan))
            {
                // Convert string back to TimeSpan
                return timeSpan;
            }
            return TimeSpan.Zero; // Default value in case of invalid input
        }
    }
}
