using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MexManager.Tools
{
    public static class FileIO
    {
        public static readonly FilePickerFileType[] FilterISO =
        [
            new ("Gamecube ISO")
            {
                Patterns = [ "*.iso" ],
            },
        ];

        public static readonly FilePickerFileType[] FilterAll =
        [
            new ("All Files")
            {
                Patterns = [ "*" ],
            },
        ];

        public static readonly FilePickerFileType[] FilterZip =
        [
            new ("Zip Archive")
            {
                Patterns = [ "*.zip" ],
            },
        ];

        public static readonly FilePickerFileType[] FilterMexProject =
        [
            new ("m-ex Project")
            {
                Patterns = [ "*.mexproj" ],
            },
            new ("All Files")
            {
                Patterns = [ "*" ],
            },
        ];

        public static readonly FilePickerFileType[] FilterJson =
        [
            new FilePickerFileType("Json")
                {
                    Patterns = [ "*.json" ],
                },
        ];

        public static readonly FilePickerFileType[] FilterJpeg =
        [
            new FilePickerFileType("JPEG")
                {
                    Patterns = [ "*.jpg", "*.jpeg" ],
                },
        ];

        public static readonly FilePickerFileType[] FilterPng =
        [
            new FilePickerFileType("PNG")
                {
                    Patterns = [ "*.png", ],
                },
        ];

        public static readonly FilePickerFileType[] FilterMusic =
        [
            new FilePickerFileType("Support Audio Formats")
                {
                    Patterns = [ "*.wav", "*.brstm" ],
                },
        ];

        public static readonly FilePickerFileType[] FilterWav =
        [
            new FilePickerFileType("Support Audio Formats")
                {
                    Patterns = [ "*.wav" ],
                },
        ];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async static Task<string?> TrySaveFile(string title, string fileName, IReadOnlyList<FilePickerFileType> types)
        {
            // Get top level
            var topLevel = App.TopLevel;

            // Check for null top level
            if (topLevel == null)
                return null;

            // Start async operation to open the dialog.
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = title,
                    SuggestedFileName = fileName,
                    FileTypeChoices = types
                });

            // check if file was found
            if (file == null)
                return null;

            // Get the absolute path and decode URI-encoded characters (like %20 for spaces)
            var filePath = file.Path?.AbsolutePath;

            // Decode the path
            return filePath != null ? Uri.UnescapeDataString(filePath) : null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async static Task<string?> TryOpenFile(string title, string fileName, IReadOnlyList<FilePickerFileType> types)
        {
            // Get the top-level window
            var topLevel = App.TopLevel;

            // Check if top-level window is available
            if (topLevel == null)
                return null;

            // Open the file picker dialog
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = title,
                    SuggestedFileName = fileName,
                    FileTypeFilter = types
                });

            // Check if any files were selected
            if (files == null || files.Count == 0)
                return null;

            // Get the absolute path and decode URI-encoded characters (like %20 for spaces)
            var filePath = files[0]?.Path?.AbsolutePath;

            // Decode the path
            return filePath != null ? Uri.UnescapeDataString(filePath) : null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string SanitizeFilename(string filename)
        {
            // Define a regular expression to match invalid filename characters
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegex = $"[{invalidChars}]";

            // Remove invalid characters
            string sanitizedFilename = Regex.Replace(filename, invalidRegex, "");

            // Optionally, you can also replace spaces with underscores or dashes to make it more URL-safe
            sanitizedFilename = sanitizedFilename.Replace(" ", "_");

            // Trim the filename to a reasonable length (e.g., 255 characters, common limit)
            if (sanitizedFilename.Length > 255)
            {
                sanitizedFilename = sanitizedFilename[0..255];
            }

            // Return the sanitized filename
            return sanitizedFilename;
        }
    }
}
