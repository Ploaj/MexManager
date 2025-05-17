using Avalonia.Data.Converters;
using mexLib;
using System;
using System.Globalization;

namespace MexManager.Converters
{
    public class StageIdConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Global.Workspace != null && value is int stage_external_id)
            {
                int internalId = MexStageIDConverter.ToInternalID(stage_external_id);

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
