using HSDRaw.GX;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace mexLib.Utilties
{
    public class ImageConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="png"></param>
        /// <returns></returns>
        public static MexImage FromPNG(Stream stream, GXTexFmt fmt, GXTlutFmt tlutFmt)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(stream);
            var bgra = GetBgraByteArrayFromPng(image, out int w, out int h);
            return new MexImage(bgra, w, h, fmt, tlutFmt);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fmt"></param>
        /// <param name="tlutFmt"></param>
        /// <returns></returns>
        public static MexImage FromPNG(Stream stream, int width, int height, GXTexFmt fmt, GXTlutFmt tlutFmt)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(stream);
            if (image.Width > width || image.Height > height)
                ResizeImage(image, width, height);
            var bgra = GetBgraByteArrayFromPng(image, out int w, out int h);
            return new MexImage(bgra, w, h, fmt, tlutFmt);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bgraPixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static byte[] ConvertBgraToPng(byte[] bgraPixels, int width, int height)
        {
            // Create an image from the BGRA pixel array
            using Image<Bgra32> image = Image.LoadPixelData<Bgra32>(bgraPixels, width, height);

            // Save the image as a PNG into a MemoryStream
            using MemoryStream ms = new ();
            image.Save(ms, new PngEncoder());

            // Return the byte array of the PNG image
            return ms.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        private static byte[] GetBgraByteArrayFromPng(Image<Rgba32> image, out int w, out int h)
        {
            int width = image.Width;
            int height = image.Height;
            byte[] bgraData = new byte[width * height * 4];
            w = width;
            h = height;

            image.ProcessPixelRows(pixels =>
            {
                for (int y = 0; y < height; y++)
                {
                    Span<Rgba32> rowSpan = pixels.GetRowSpan(y);
                    for (int x = 0; x < width; x++)
                    {
                        Rgba32 pixel = rowSpan[x];
                        int index = (y * width + x) * 4;
                        // BGRA format
                        bgraData[index + 0] = pixel.B;
                        bgraData[index + 1] = pixel.G;
                        bgraData[index + 2] = pixel.R;
                        bgraData[index + 3] = pixel.A;
                    }
                }
            });

            return bgraData;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        public static void ResizeImage(Image image, int maxWidth, int maxHeight)
        {
            // Calculate the new dimensions
            int newWidth, newHeight;
            double aspectRatio = (double)image.Width / image.Height;

            if (image.Width > image.Height)
            {
                // Landscape or square
                newWidth = Math.Min(maxWidth, image.Width);
                newHeight = (int)(newWidth / aspectRatio);
            }
            else
            {
                // Portrait
                newHeight = Math.Min(maxHeight, image.Height);
                newWidth = (int)(newHeight * aspectRatio);
            }

            // Ensure the new dimensions fit within the maxWidth and maxHeight
            if (newWidth > maxWidth)
            {
                newWidth = maxWidth;
                newHeight = (int)(newWidth / aspectRatio);
            }
            if (newHeight > maxHeight)
            {
                newHeight = maxHeight;
                newWidth = (int)(newHeight * aspectRatio);
            }

            // Resize the image
            image.Mutate(x => x.Resize(newWidth, newHeight));
        }
    }
}
