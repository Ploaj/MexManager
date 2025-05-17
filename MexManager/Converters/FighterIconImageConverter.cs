using Avalonia.Data.Converters;
using mexLib;
using mexLib.Types;
using MexManager.Tools;
using System;
using System.Globalization;

namespace MexManager.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is MexFighter item &&
                Global.Workspace != null)
            {
                int index = Global.Workspace.Project.Fighters.IndexOf(item);
                if (index >= 0x21 - 6 && index < Global.Workspace.Project.Fighters.Count - 6)
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

    public class StockIconImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is MexFighter item &&
                Global.Workspace != null)
            {
                if (item.Costumes.Count > 0)
                {
                    MexImage? icon = item.Costumes[0].IconAsset.GetSourceImage(Global.Workspace);
                    if (icon != null)
                    {
                        return icon.ToBitmap();
                    }
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
