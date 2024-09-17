using Avalonia.Data.Converters;
using MeleeMedia.Audio;
using PropertyModels.Collections;
using PropertyModels.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MexManager.Converters
{
    public class SoundCommand
    {
        public SelectableList<SEMCode> Code { get; set; } = [];
    }

    public class SoundCommandConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
