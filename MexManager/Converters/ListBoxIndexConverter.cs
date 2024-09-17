using Avalonia.Controls;
using Avalonia.Data.Converters;
using mexLib.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MexManager.Converters
{
    public class ListBoxIndexConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2)
                return null;

            if (values[0] == null || values[1] == null)
                return null;

            if (values[1] is not ListBox list)
                return null;

            int offset = 0;
            if (values.Count > 2 && values[2] is int index)
                offset += index;

            return (list.Items.IndexOf(values[0]) + offset).ToString("D3") + ". ";
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DataGridIndexConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2)
                return null;

            if (values[0] is not MexSound sound)
                return null;

            if (values[1] is not ObservableCollection<MexSound> list)
                return null;

            return $"{list.IndexOf(sound):D3}";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
