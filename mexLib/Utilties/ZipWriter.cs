using System.IO.Compression;

namespace mexLib.Utilties
{
    public class ZipWriter : IDisposable
    {
        private readonly Stream _fileStream;
        private readonly ZipArchive _zipArchive;

        public ZipWriter(string zipPath)
        {
            _fileStream = new FileStream(zipPath, FileMode.Create);
            _zipArchive = new ZipArchive(_fileStream, ZipArchiveMode.Create, true);
        }

        public void Write(string fileName, byte[] data)
        {
            var zipArchiveEntry = _zipArchive.CreateEntry(fileName, CompressionLevel.Fastest);
            using var zipStream = zipArchiveEntry.Open();
            zipStream.Write(data, 0, data.Length);
        }

        public void Dispose()
        {
            _zipArchive.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
