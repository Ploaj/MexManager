using Avalonia.Media.Imaging;
using Avalonia.Media;
using Avalonia;
using System.IO;
using mexLib.Utilties;
using mexLib;

namespace MexManager.Tools
{
    public static class StageBannerGenerator
    {
        public static MexImage DrawTextToImageAsync(string location, string name)
        {
            // Create a bitmap (image size: 400x200)
            var bitmap = new RenderTargetBitmap(new PixelSize(224, 56), new Vector(96, 96));

            // Open the drawing context
            using var context = bitmap.CreateDrawingContext();

            // Define brushes
            var textBrush = Brushes.White;

            {
                var firstFont = new Typeface("avares://MexManager/Assets/Fonts/A_OTF_Folk_Pro_H.otf#A-OTF Folk Pro");

                // Target width for the horizontally stretched text
                var targetWidth = 208;
                var targetHeight = 74;

                // Draw first line of text
                var formattedText = new FormattedText(name, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, firstFont, 40, textBrush);

                // Measure the initial text size
                var initialTextSize = new Size(formattedText.Width, formattedText.Height);

                // Calculate the horizontal scaling factor (how much to stretch horizontally)
                double scaleX = initialTextSize.Width > targetWidth ? targetWidth / initialTextSize.Width : 1;
                double scaleY = initialTextSize.Height > targetHeight ? targetHeight / initialTextSize.Height : 1;

                // Apply a horizontal-only scaling transformation
                var transform = Matrix.CreateScale(scaleX, scaleY);
                var skew = Matrix.CreateSkew(-10, 0);

                // Calculate the centered Y position (since we're only scaling horizontally, we center vertically)
                var position = new Point(
                    (bitmap.Size.Width / 2 - initialTextSize.Width * scaleX / 2 + 10) / scaleX,  // Horizontally center the scaled text
                    18 / scaleY // Vertically center the text
                );

                // Push the scaling transformation before drawing the text
                using var sca = context.PushTransform(transform);
                using var ske = context.PushTransform(skew);

                // Draw the horizontally stretched text
                context.DrawText(formattedText, position);
            }
            {
                var firstFont = new Typeface("avares://MexManager/Assets/Fonts/Palatino-Linotype-Bold.ttf#Palatino Linotype");

                // Target width for the horizontally stretched text
                var targetWidth = 160;

                // Draw first line of text
                var formattedText = new FormattedText(location, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, firstFont, 18, textBrush);

                // Measure the initial text size
                var initialTextSize = new Size(formattedText.Width, formattedText.Height);

                // Calculate the horizontal scaling factor (how much to stretch horizontally)
                double scaleX = targetWidth / initialTextSize.Width;

                // Apply a horizontal-only scaling transformation
                var transform = Matrix.CreateScale(scaleX, 1); // Horizontal scaling only (X scaled, Y not scaled)

                // Calculate the centered Y position (since we're only scaling horizontally, we center vertically)
                var position = new Point(
                    (bitmap.Size.Width - targetWidth) / 2,  // Horizontally center the scaled text
                    1 // Vertically center the text
                );

                // Push the scaling transformation before drawing the text
                using var sca = context.PushTransform(transform);

                // Draw the horizontally stretched text
                context.DrawText(formattedText, position);
            }

            // Save the image as a PNG file
            using var stream = new MemoryStream();
            bitmap.Save(stream);

            // return mex image
            stream.Position = 0;
            return ImageConverter.FromPNG(stream, HSDRaw.GX.GXTexFmt.I4, HSDRaw.GX.GXTlutFmt.IA8);
        }
    }
}
