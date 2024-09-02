using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mexLib;
using ReactiveUI;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MexManager
{
    public static class ImageExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="rgbaPixels"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static WriteableBitmap FromRGB(this WriteableBitmap bmp, byte[] rgbaPixels)
        {
            // Validate pixel data length
            int expectedSize = bmp.PixelSize.Width * bmp.PixelSize.Height * 4; // 4 bytes per pixel (RGBA)
            if (rgbaPixels.Length != expectedSize)
            {
                throw new ArgumentException("The length of the RGBA pixel array does not match the dimensions of the bitmap.");
            }

            // Create a new array for the swapped pixel data
            byte[] swappedPixels = new byte[rgbaPixels.Length];
            Array.Copy(rgbaPixels, swappedPixels, rgbaPixels.Length);

            // Swap red and blue channels in the new array
            for (int i = 0; i < swappedPixels.Length; i += 4)
            {
                byte red = swappedPixels[i];
                byte blue = swappedPixels[i + 2];

                // Swap the red and blue values
                swappedPixels[i] = blue;
                swappedPixels[i + 2] = red;
            }

            // Create a MemoryStream from the pixel data
            using (var memoryStream = new MemoryStream(rgbaPixels))
            {
                using (var framebuffer = bmp.Lock())
                {
                    // Copy pixel data into the WriteableBitmap
                    var dataPointer = framebuffer.Address;

                    // Copy the pixel data into the WriteableBitmap
                    Marshal.Copy(swappedPixels, 0, dataPointer, swappedPixels.Length);
                }
                return bmp;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rgbaPixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Bitmap ToBitmap(this MexImage image)
        {
            var width = image.Width;
            var height = image.Height;
            var rgbaPixels = image.GetBGRA();

            // Validate pixel data length
            int expectedSize = width * height * 4; // 4 bytes per pixel (RGBA)
            if (rgbaPixels.Length != expectedSize)
            {
                throw new ArgumentException("The length of the RGBA pixel array does not match the dimensions of the bitmap.");
            }

            // Swap red and blue channels in the new array
            for (int i = 0; i < rgbaPixels.Length; i += 4)
            {
                byte red = rgbaPixels[i];
                byte blue = rgbaPixels[i + 2];

                // Swap the red and blue values
                rgbaPixels[i] = blue;
                rgbaPixels[i + 2] = red;
            }

            // Create a MemoryStream from the pixel data
            using (var memoryStream = new MemoryStream(rgbaPixels))
            {
                var pixelSize = new PixelSize(width, height);
                var stride = width * 4; // Each row in bytes (4 bytes per pixel)
                var dpi = new Vector(96, 96); // Default DPI; adjust if necessary

                // Create the Bitmap using the MemoryStream and WriteableBitmap
                var bitmap = new WriteableBitmap(pixelSize, dpi, PixelFormat.Rgba8888, AlphaFormat.Unpremul);
                using (var framebuffer = bitmap.Lock())
                {
                    // Copy pixel data into the WriteableBitmap
                    var dataPointer = framebuffer.Address;

                    // Copy the pixel data into the WriteableBitmap
                    Marshal.Copy(rgbaPixels, 0, dataPointer, rgbaPixels.Length);
                }
                return bitmap;
            }
        }
        //public static byte[] GetPixelDataFromImage(this Image imageControl)
        //{
        //    // Ensure the Image control has a Bitmap as its source
        //    if (imageControl.Source is not Bitmap bitmap)
        //    {
        //        throw new ArgumentException("Image control does not have a Bitmap source.");
        //    }

        //    // Lock the bitmap to access the pixel data
        //    using (var framebuffer = bitmap.)
        //    {
        //        // Access the pixel buffer
        //        IntPtr dataPointer = framebuffer.Address;
        //        int stride = framebuffer.Stride;
        //        int height = framebuffer.Height;
        //        int width = framebuffer.Width;

        //        // Create an array to hold the pixel data
        //        byte[] pixelData = new byte[height * stride];

        //        // Copy pixel data from the framebuffer to the byte array
        //        Marshal.Copy(dataPointer, pixelData, 0, pixelData.Length);

        //        return pixelData;
        //    }
        //}
    }
}
