using Avalonia.Data.Converters;
using mexLib;
using mexLib.Types;
using System;
using System.Globalization;

namespace MexManager.Converters
{
    public class CSSIconTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Global.Workspace != null && value is MexCharacterSelectIcon icon)
            {
                var internalId = MexFighterIDConverter.ToInternalID(icon.Fighter, Global.Workspace.Project.Fighters.Count);

                if (internalId < Global.Workspace.Project.Fighters.Count && internalId >= 0)
                {
                    return Global.Workspace.Project.Fighters[internalId].Name;
                }
            }
            return "none";
        }

        public object? ConvertBack(object? value, Type targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
