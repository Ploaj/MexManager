using HSDRaw.GX;
using mexLib.Utilties;
using System.IO.Compression;

namespace mexLib.AssetTypes
{
    public class MexTextureAsset
    {
        public string? AssetFileName { get; set; }

        public string AssetPath { get; internal set; } = "";

        public int Width { get; internal set; }

        public int Height { get; internal set; }

        public GXTexFmt Format { get; internal set; }

        public GXTlutFmt TlutFormat { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public MexTextureAsset()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="subPath"></param>
        /// <returns></returns>
        private static string GetRelativePath(string basePath, string subPath)
        {
            Uri baseUri = new(basePath);
            Uri subUri = new(subPath);
            Uri relativeUri = baseUri.MakeRelativeUri(subUri);

            // Get the relative path string
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));

            // Remove the file extension, if present
            string relativePathWithoutExtension = Path.Combine(Path.GetDirectoryName(relativePath) ?? string.Empty,
                                                               Path.GetFileNameWithoutExtension(relativePath));

            return relativePathWithoutExtension;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        private string GetUniqueAssetPath(MexWorkspace workspace)
        {
            string assetPath = workspace.GetAssetPath("");
            string sourcePath = workspace.FileManager.GetUniqueFilePath(workspace.GetAssetPath(AssetPath) + ".png");
            return GetRelativePath(assetPath, sourcePath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public string GetFullPath(MexWorkspace workspace)
        {
            AssetFileName ??= GetUniqueAssetPath(workspace);
            return workspace.GetAssetPath(AssetFileName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="image"></param>
        public void SetFromMexImage(MexWorkspace workspace, MexImage image, bool updateSource = true)
        {
            string path = GetFullPath(workspace);

            // set png
            workspace.FileManager.Set(path + ".tex", image.ToByteArray());

            // compile and set tex
            if (updateSource)
                workspace.FileManager.Set(path + ".png", image.ToPNG());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="filePath"></param>
        public void SetFromImageFile(MexWorkspace workspace, Stream imageStream)
        {
            string path = GetFullPath(workspace);

            // set png
            // i perform an encoding before saving to apply limitations of the texture format for more accurate preview
            imageStream.Position = 0;
            MexImage source_png = ImageConverter.FromPNG(imageStream, Format, TlutFormat);
            workspace.FileManager.Set(path + ".png", source_png.ToPNG());

            // compile and set tex
            imageStream.Position = 0;
            MexImage tex = ImageConverter.FromPNG(imageStream,
                Width == -1 ? source_png.Width : Width,
                Height == -1 ? source_png.Height : Height,
                Format,
                TlutFormat);
            workspace.FileManager.Set(path + ".tex", tex.ToByteArray());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        private Stream? GetSourceImageStream(MexWorkspace workspace)
        {
            if (AssetFileName == null)
                return null;

            string path = GetFullPath(workspace);

            return workspace.FileManager.GetStream(path + ".png");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public MexImage? GetSourceImage(MexWorkspace workspace)
        {
            using Stream? stream = GetSourceImageStream(workspace);

            if (stream == null)
                return null;

            MexImage tex = ImageConverter.FromPNG(stream, Format, TlutFormat);

            return tex;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public MexImage? GetTexFile(MexWorkspace workspace)
        {
            if (AssetFileName == null)
                return null;

            string texPath = GetFullPath(workspace) + ".tex";

            if (!workspace.FileManager.Exists(texPath))
                return null;

            byte[] stream = workspace.FileManager.Get(texPath);
            return MexImage.FromByteArray(stream);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        public void Delete(MexWorkspace workspace)
        {
            if (AssetFileName == null)
                return;

            string path = GetFullPath(workspace);

            workspace.FileManager.Remove(path + ".png");
            workspace.FileManager.Remove(path + ".tex");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="csp_width"></param>
        /// <param name="csp_height"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal MexImage? Resize(MexWorkspace workspace, int csp_width, int csp_height)
        {
            MexImage? source = GetSourceImage(workspace);

            if (source != null)
            {
                MexImage resized = ImageConverter.Resize(source, csp_width, csp_height);

                string path = GetFullPath(workspace);
                workspace.FileManager.Set(path + ".tex", resized.ToByteArray());

                return resized;
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="zip"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool SetFromPackage(MexWorkspace workspace, ZipArchive zip, string filename)
        {
            ZipArchiveEntry? entry = zip.GetEntry(filename);

            if (entry == null)
                return false;

            AssetFileName = null;

            using MemoryStream img = new(entry.Extract());
            SetFromImageFile(workspace, img);

            return true;
        }
    }
}
