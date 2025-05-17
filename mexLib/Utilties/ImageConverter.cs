using HSDRaw.GX;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace mexLib.Utilties
{
    public class ImageConverter
    {
        private static readonly IResampler Resampler = KnownResamplers.Bicubic;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static MexImage Resize(MexImage tex, int width, int height)
        {
            using var img = Image.Load(tex.ToPNG());
            using var stream = new MemoryStream();
            img.Mutate(x => x.Resize(width, height, Resampler));
            img.SaveAsPng(stream);
            stream.Position = 0;
            return FromPNG(stream, tex.Format, tex.TlutFormat);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalImage"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public static byte[] ResizeBGRA_Nearest(byte[] originalImage, int width, int height, int newWidth, int newHeight)
        {
            // Create a new byte array for the resized image
            byte[] resizedImage = new byte[newWidth * newHeight * 4]; // BGRA format, 4 bytes per pixel

            // Calculate the ratio between the original size and the new size
            float xRatio = (float)(width) / newWidth;
            float yRatio = (float)(height) / newHeight;

            for (int newY = 0; newY < newHeight; newY++)
            {
                for (int newX = 0; newX < newWidth; newX++)
                {
                    // Calculate the source pixel position in the original image
                    int srcX = (int)(newX * xRatio);
                    int srcY = (int)(newY * yRatio);

                    // Ensure the source coordinates are within bounds
                    srcX = Math.Min(srcX, width - 1);
                    srcY = Math.Min(srcY, height - 1);

                    // Get the index of the nearest pixel in the original image
                    int srcIndex = (srcY * width + srcX) * 4;

                    // Copy the pixel data from the original image to the resized image
                    int destIndex = (newY * newWidth + newX) * 4;
                    Array.Copy(originalImage, srcIndex, resizedImage, destIndex, 4);
                }
            }

            return resizedImage;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bgra"></param>
        /// <param name="fmt"></param>
        /// <param name="tlutFmt"></param>
        private static void ProcessIA(ref byte[] bgra, GXTexFmt fmt)
        {
            if (fmt == GXTexFmt.I4 || fmt == GXTexFmt.I8)
            {
                for (int i = 0; i < bgra.Length; i += 4)
                {
                    bgra[i] = bgra[i + 3];
                    bgra[i + 1] = bgra[i + 3];
                    bgra[i + 2] = bgra[i + 3];
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="png"></param>
        /// <returns></returns>
        public static MexImage FromPNG(Stream stream, GXTexFmt fmt, GXTlutFmt tlutFmt)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(stream);
            var bgra = GetBgraByteArrayFromPng(image, out int w, out int h);
            ProcessIA(ref bgra, fmt);
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
            ProcessIA(ref bgra, fmt);
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
            image.Mutate(x => x.Resize(newWidth, newHeight, Resampler));
        }
    }
}
