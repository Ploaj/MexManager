using mexLib;
using System;
using Avalonia.Data.Converters;
using System.Globalization;
using MexManager.Tools;
using System.IO;
using mexLib.Types;

namespace MexManager.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is MexFighter item &&
                Global.Workspace != null)
            {
                var index = Global.Workspace.Project.Fighters.IndexOf(item);
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
                    var iconPath = Path.GetFileNameWithoutExtension(item.Costumes[0].File.FileName);
                    iconPath = Global.Workspace.GetAssetPath($"icons//{iconPath}.tex");

                    if (Global.Files.Exists(iconPath))
                    {
                        return MexImage.FromByteArray(Global.Files.Get(iconPath)).ToBitmap();
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
