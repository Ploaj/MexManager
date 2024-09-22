using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using MeleeMedia.Video;
using MexManager.Tools;
using System;
using System.Globalization;
using System.IO;

namespace MexManager.Converters
{
    public class FighterMediaImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string path &&
                Global.Workspace != null)
            {
                var thpPath = Global.Workspace.GetFilePath(path);

                if (!Global.Files.Exists(thpPath))
                    return BitmapManager.MexFighterImage;

                var thp = new THP(Global.Files.Get(thpPath));
                var jpeg = thp.ToJPEG();
                using var stream = new MemoryStream(jpeg);
                var bitmap = new Bitmap(stream);
                return bitmap;            
            }

            return BitmapManager.MexFighterImage;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
