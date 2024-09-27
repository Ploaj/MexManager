using Avalonia.Data.Converters;
using mexLib.Types;
using MexManager.Tools;
using System;
using System.Globalization;

namespace MexManager.Converters
{
    public class TrophyIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is MexTrophy trophy &&
                Global.Workspace != null)
            {
                var index = Global.Workspace.Project.Trophies.IndexOf(trophy);
                if (index > 292)
                {
                    return BitmapManager.MexFighterImage;
                }
                else
                {
                    return BitmapManager.MeleeFighterImage;
                }
            }

            return null;
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}
