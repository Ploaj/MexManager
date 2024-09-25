using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MexManager.Converters
{
    public class KirbyCostumeNameConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Global.Workspace == null ||
                Global.Workspace.Project.Fighters.Count < 4)
                return null;

            if (values.Count < 2)
                return null;

            if (values[0] == null || values[1] == null)
                return null;

            if (values[1] is not ListBox list)
                return null;

            var index = list.Items.IndexOf(values[0]);
            var kirby = Global.Workspace.Project.Fighters[4].Costumes[index].Name;

            return $"{index:D3}. {kirby}";
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
