using Avalonia.Data.Converters;
using mexLib;
using System;
using System.Globalization;

namespace MexManager.Converters
{
    public class FighterIDTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Global.Workspace != null && value is int fighter_index)
            {
                var internalId = MexFighterIDConverter.ToInternalID(fighter_index, Global.Workspace.Project.Fighters.Count);

                if (internalId < Global.Workspace.Project.Fighters.Count && internalId >= 0)
                {
                    return Global.Workspace.Project.Fighters[internalId].Name;
                }
            }
            return "Null";
        }

        public object? ConvertBack(object? value, Type targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
