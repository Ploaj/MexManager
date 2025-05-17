using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using mexLib;
using mexLib.Utilties;
using System.IO;

namespace MexManager.Tools
{
    public static class StageBannerGenerator
    {
        public static MexImage DrawTextToImageAsync(string location, string name)
        {
            // Create a bitmap (image size: 400x200)
            RenderTargetBitmap bitmap = new(new PixelSize(224, 56), new Vector(96, 96));

            // Open the drawing context
            using DrawingContext context = bitmap.CreateDrawingContext();

            // Define brushes
            IImmutableSolidColorBrush textBrush = Brushes.White;

            {
                Typeface firstFont = new("avares://MexManager/Assets/Fonts/A_OTF_Folk_Pro_H.otf#A-OTF Folk Pro");

                // Target width for the horizontally stretched text
                int targetWidth = 208;
                int targetHeight = 74;

                // Draw first line of text
                FormattedText formattedText = new(name, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, firstFont, 40, textBrush);

                // Measure the initial text size
                Size initialTextSize = new(formattedText.Width, formattedText.Height);

                // Calculate the horizontal scaling factor (how much to stretch horizontally)
                double scaleX = initialTextSize.Width > targetWidth ? targetWidth / initialTextSize.Width : 1;
                double scaleY = initialTextSize.Height > targetHeight ? targetHeight / initialTextSize.Height : 1;

                // Apply a horizontal-only scaling transformation
                Matrix transform = Matrix.CreateScale(scaleX, scaleY);
                Matrix skew = Matrix.CreateSkew(-10, 0);

                // Calculate the centered Y position (since we're only scaling horizontally, we center vertically)
                Point position = new(
                    (bitmap.Size.Width / 2 - initialTextSize.Width * scaleX / 2 + 10) / scaleX,  // Horizontally center the scaled text
                    18 / scaleY // Vertically center the text
                );

                // Push the scaling transformation before drawing the text
                using DrawingContext.PushedState sca = context.PushTransform(transform);
                using DrawingContext.PushedState ske = context.PushTransform(skew);

                // Draw the horizontally stretched text
                context.DrawText(formattedText, position);
            }
            {
                Typeface firstFont = new("avares://MexManager/Assets/Fonts/Palatino-Linotype-Bold.ttf#Palatino Linotype");

                // Target width for the horizontally stretched text
                int targetWidth = 160;

                // Draw first line of text
                FormattedText formattedText = new(location, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, firstFont, 18, textBrush);

                // Measure the initial text size
                Size initialTextSize = new(formattedText.Width, formattedText.Height);

                // Calculate the horizontal scaling factor (how much to stretch horizontally)
                double scaleX = targetWidth / initialTextSize.Width;

                // Apply a horizontal-only scaling transformation
                Matrix transform = Matrix.CreateScale(scaleX, 1); // Horizontal scaling only (X scaled, Y not scaled)

                // Calculate the centered Y position (since we're only scaling horizontally, we center vertically)
                Point position = new(
                    (bitmap.Size.Width - targetWidth) / 2,  // Horizontally center the scaled text
                    1 // Vertically center the text
                );

                // Push the scaling transformation before drawing the text
                using DrawingContext.PushedState sca = context.PushTransform(transform);

                // Draw the horizontally stretched text
                context.DrawText(formattedText, position);
            }

            // Save the image as a PNG file
            using MemoryStream stream = new();
            bitmap.Save(stream);

            // return mex image
            stream.Position = 0;
            return ImageConverter.FromPNG(stream, HSDRaw.GX.GXTexFmt.I4, HSDRaw.GX.GXTlutFmt.IA8);
        }
    }
}
