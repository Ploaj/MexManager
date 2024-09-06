using mexLib.AssetTypes;
using System.IO.Compression;

namespace mexLib.Utilties
{
    public class ZipWriter : IDisposable
    {
        private readonly Stream _fileStream;
        private readonly ZipArchive _zipArchive;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipPath"></param>
        public ZipWriter(string zipPath)
        {
            _fileStream = new FileStream(zipPath, FileMode.Create);
            _zipArchive = new ZipArchive(_fileStream, ZipArchiveMode.Create, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="filePath"></param>
        public bool TryWriteFile(MexWorkspace workspace, string sourcePath, string targetPath)
        {
            var path = workspace.GetFilePath(sourcePath);
            if (workspace.FileManager.Exists(path))
            {
                Write(targetPath, workspace.FileManager.Get(path));
                return true;
            }

            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="asset"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool TryWriteTextureAsset(MexWorkspace workspace, MexTextureAsset asset, string filePath)
        {
            var csp = asset.GetSourceImage(workspace);
            if (csp != null)
            {
                Write(filePath, csp.ToPNG());
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        public void Write(string fileName, byte[] data)
        {
            var zipArchiveEntry = _zipArchive.CreateEntry(fileName, CompressionLevel.Fastest);
            using var zipStream = zipArchiveEntry.Open();
            zipStream.Write(data, 0, data.Length);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _zipArchive.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
