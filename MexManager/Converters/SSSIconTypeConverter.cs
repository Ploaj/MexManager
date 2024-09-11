using mexLib.Types;
using mexLib;
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MexManager.Converters
{
    public class SSSIconTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Global.Workspace != null && value is MexStageSelectIcon icon)
            {
                var internalId = MexStageIDConverter.ToInternalID(icon.StageID);

                if (internalId >= 0)
                {
                    return Global.Workspace.Project.Stages[internalId].Name;
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
