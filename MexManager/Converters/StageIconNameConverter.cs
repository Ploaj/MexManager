using Avalonia.Data.Converters;
using mexLib;
using mexLib.Types;
using System;
using System.Globalization;

namespace MexManager.Converters
{
    public class StageIconNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Global.Workspace != null && 
                value is MexStageSelectIcon icon)
            {
                switch (icon.Status)
                {
                    case MexStageSelectIcon.StageIconStatus.Unlocked:
                        {
                            var internalId = MexStageIDConverter.ToInternalID(icon.StageID);

                            if (internalId >= 0)
                                return Global.Workspace.Project.Stages[internalId].Name;
                        }
                        break;
                    case MexStageSelectIcon.StageIconStatus.Locked: return "(Locked)";
                    case MexStageSelectIcon.StageIconStatus.Decoration: return "(Decoration)";
                    case MexStageSelectIcon.StageIconStatus.Hidden: return "(Hidden)";
                    case MexStageSelectIcon.StageIconStatus.Random: return "(Random)";
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
