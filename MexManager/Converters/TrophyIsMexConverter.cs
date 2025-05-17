using Avalonia.Data.Converters;
using mexLib.Types;
using System;
using System.Globalization;

namespace MexManager.Converters
{
    public class TrophyIsMexConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is MexTrophy trophy &&
                Global.Workspace != null)
            {
                int index = Global.Workspace.Project.Trophies.IndexOf(trophy);
                if (index > 292)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}
