﻿using mexLib.AssetTypes;
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
        public ZipWriter(Stream stream)
        {
            _fileStream = stream;
            _zipArchive = new ZipArchive(_fileStream, ZipArchiveMode.Create, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipPath"></param>
        public ZipWriter(string zipPath) : this(new FileStream(zipPath, FileMode.Create))
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="filePath"></param>
        public bool TryWriteFile(MexWorkspace workspace, string sourcePath, string targetPath)
        {
            string path = workspace.GetFilePath(sourcePath);
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
            MexImage? csp = asset.GetSourceImage(workspace);
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
            ZipArchiveEntry zipArchiveEntry = _zipArchive.CreateEntry(fileName, CompressionLevel.Fastest);
            using Stream zipStream = zipArchiveEntry.Open();
            zipStream.Write(data, 0, data.Length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="o"></param>
        public void WriteAsJson(string fileName, object o)
        {
            Write(fileName, MexJsonSerializer.Serialize(o).ToArray().Select(e => (byte)e).ToArray());
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
