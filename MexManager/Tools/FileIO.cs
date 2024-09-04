using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MexManager.Tools
{
    public static class FileIO
    {
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

            // return url
            return file.Path.AbsolutePath;
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

            // Return the absolute path of the first selected file
            return files[0]?.Path?.AbsolutePath;
        }
    }
}
