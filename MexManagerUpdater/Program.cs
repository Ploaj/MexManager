using System.Diagnostics;
using System.IO.Compression;

namespace MexManagerUpdater
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Logger.WriteLine("Usage: <url> <version> [-r]");
                return;
            }

            var downloadUrl = args[0];
            var version = args[1];
            var runToolAfter = args.Length >= 3 && args[2] == "-r";

            const string zipPath = "update.zip";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Logger.WriteLine("Downloading update...");
                    using (var response = await client.GetAsync(downloadUrl))
                    {
                        response.EnsureSuccessStatusCode();

                        using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await response.Content.CopyToAsync(fs);
                        }
                    }
                }

                Logger.WriteLine("Extracting...");
                using (FileStream stream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.CompressedLength == 0)
                            continue;

                        var fullPath = Path.GetFullPath(entry.FullName);

                        // Prevent directory traversal
                        if (!fullPath.StartsWith(Directory.GetCurrentDirectory()))
                            continue;

                        var directory = Path.GetDirectoryName(fullPath);
                        if (!string.IsNullOrEmpty(directory))
                            Directory.CreateDirectory(directory);

                        using (var input = entry.Open())
                        using (var output = new FileStream(fullPath, FileMode.Create))
                            await input.CopyToAsync(output);

                        Logger.WriteLine($"Extracted: {entry.FullName} ({entry.CompressedLength:X})");
                    }
                }

                File.Delete(zipPath);
                await File.WriteAllTextAsync("version.txt", version);

                if (runToolAfter)
                    Process.Start("MexManager.Desktop.exe");

                Logger.WriteLine("Update completed successfully.");
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Error during update: " + ex.Message);
            }
        }
    }
    public static class Logger
    {
        private static readonly FileStream _stream = new(@"update_log.txt", FileMode.Create);
        private static readonly StreamWriter _writer = new(_stream) { AutoFlush = true };
        private static readonly object _lock = new();
        private static bool _disposed = false;
        public static void WriteLine(string line)
        {
            lock (_lock)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(Logger));

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                _writer.WriteLine($"[{timestamp}] {line}");
            }
        }

        public static void Shutdown()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    _writer.Dispose();
                    _stream.Dispose();
                    _disposed = true;
                }
            }
        }
    }
}
