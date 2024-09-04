using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace mexLib
{
    public abstract class MexAssetContainerBase : INotifyPropertyChanged
    {
        public abstract void GenerateAssetPaths(MexWorkspace ws);
        public abstract void RemoveAssets(MexWorkspace ws);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// removes both .tex and .png from assets
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="filePath"></param>
        protected static void RemoveTexAsset(MexWorkspace ws, string filePath)
        {
            ws.FileManager.Remove(ws.GetAssetPath(filePath));
            ws.FileManager.Remove(ws.GetAssetPath(filePath).Replace(".png", ".tex"));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="filePath">current filepath data</param>
        /// <param name="folder">folder</param>
        /// <param name="hint">base filename</param>
        /// <param name="extension">externsion</param>
        /// <returns></returns>
        protected static string GeneratePathIfNotExists(MexWorkspace ws, string filePath, string folder, string hint, string extension)
        {
            var path = ws.GetAssetPath(filePath);

            if (ws.FileManager.Exists(path))
            {
                return filePath;
            }

            return GeneratePath(ws, folder, hint, extension);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="hint"></param>
        /// <returns></returns>
        public static string GeneratePath(MexWorkspace ws, string folder, string hint, string extension)
        {
            // Combine directory and filename to get the full path
            string fullPath = Path.Combine(folder, hint) + extension;

            // Check if file exists, and if so, generate a new filename with a number appended
            if (!ws.FileManager.Exists(ws.GetAssetPath(fullPath)))
            {
                return fullPath;
            }

            int counter = 0;

            do
            {
                // Format the new filename with a counter, ensuring the number has three digits
                fullPath = $"{Path.Combine(folder, hint)}_{counter:000}{extension}";
                counter++;
            } while (ws.FileManager.Exists(ws.GetAssetPath(fullPath)));

            return fullPath;
        }
    }
}
