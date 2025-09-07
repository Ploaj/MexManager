namespace mexLib.Utilties
{
    public class FileManager
    {
        private readonly Dictionary<string, byte[]> ToAdd = new();

        private readonly List<string> ToRemove = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string GetUniqueFilePath(string filePath)
        {
            // sanitize
            filePath = Path.GetFullPath(filePath);

            // Get the directory, filename without extension, and extension
            string? directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            // Set the initial unique file path
            string uniqueFilePath = filePath;
            int count = 1;

            // Check if the file exists, if so append a number until a unique path is found
            while (Exists(uniqueFilePath))
            {
                uniqueFilePath = Path.Combine(directory ?? "", $"{fileName}_{count:D3}{extension}");
                count++;
            }

            return uniqueFilePath;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Exists(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            path = Path.GetFullPath(path);

            if (ToRemove.Contains(path))
                return false;

            if (ToAdd.ContainsKey(path))
                return true;

            return File.Exists(path);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public long GetFileSize(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return 0;

            path = Path.GetFullPath(path);

            if (ToRemove.Contains(path))
                return 0;

            if (ToAdd.ContainsKey(path))
                return ToAdd[path].Length;

            return new FileInfo(path).Length;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Stream? GetStream(string path)
        {
            path = Path.GetFullPath(path);

            if (ToAdd.ContainsKey(path))
                return new MemoryStream(ToAdd[path]);

            if (File.Exists(path))
                return new FileStream(path, FileMode.Open);

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public byte[] Get(string path)
        {
            path = Path.GetFullPath(path);

            if (ToAdd.ContainsKey(path))
                return ToAdd[path];

            if (File.Exists(path) && !ToRemove.Contains(path))
                return File.ReadAllBytes(path);

            return Array.Empty<byte>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public void Set(string path, byte[] data)
        {
            path = Path.GetFullPath(path);

            if (ToAdd.ContainsKey(path))
            {
                ToAdd[path] = data;
            }
            else
            {
                ToAdd.Add(path, data);
            }
            ToRemove.Remove(path);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void Remove(string path)
        {
            path = Path.GetFullPath(path);

            ToRemove.Add(path);
            ToAdd.Remove(path);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            foreach (KeyValuePair<string, byte[]> v in ToAdd)
            {
                string? dir = Path.GetDirectoryName(v.Key);
                if (dir != null)
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(v.Key, v.Value);
            }

            foreach (string v in ToRemove)
            {
                if (File.Exists(v))
                    File.Delete(v);
            }

            Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            ToAdd.Clear();
            ToRemove.Clear();
        }
    }
}
