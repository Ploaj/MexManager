using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MexManager.Converters
{
    public class BooleanToImageConverter : IValueConverter
    {
        public Bitmap? TrueImage { get; set; }
        public Bitmap? FalseImage { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return boolean ? TrueImage : FalseImage;
            }
            return FalseImage; // Default to FalseImage if value is not a boolean
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BooleanInverterConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }

            return AvaloniaProperty.UnsetValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }

            return AvaloniaProperty.UnsetValue;
        }
    }
}
