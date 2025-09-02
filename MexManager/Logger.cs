using System;
using System.Diagnostics;
using System.IO;

namespace MexManager
{
    public static class Logger
    {
        private static readonly FileStream _stream = new(@"log.txt", FileMode.Create);
        private static readonly StreamWriter _writer = new(_stream) { AutoFlush = true };
        private static readonly object _lock = new();
        private static bool _disposed = false;

        public static StreamWriter GetWriter()
        {
            return _writer;
        }

        public static void WriteLine(string line)
        {
            lock (_lock)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(Logger));

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                _writer.WriteLine($"[{timestamp}] {line}");
#if DEBUG
                Debug.WriteLine($"[{timestamp}] {line}");
#endif
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
