using HSDRaw.GX;
using mexLib.Utilties;

namespace mexLib.AssetTypes
{
    public class MexTextureAsset
    {
        public string? AssetFileName { get; internal set; }

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
            Uri baseUri = new (basePath);
            Uri subUri = new (subPath);
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
            var assetPath = workspace.GetAssetPath("");
            var sourcePath = workspace.FileManager.GetUniqueFilePath(workspace.GetAssetPath(AssetPath) + ".png");
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
        public void SetFromMexImage(MexWorkspace workspace, MexImage image)
        {
            var path = GetFullPath(workspace);

            // set png
            workspace.FileManager.Set(path + ".tex", image.ToByteArray());

            // compile and set tex
            workspace.FileManager.Set(path + ".png", image.ToPNG());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="filePath"></param>
        public void SetFromImageFile(MexWorkspace workspace, string filePath)
        {
            var path = GetFullPath(workspace);

            // set png
            workspace.FileManager.Set(path + ".png", File.ReadAllBytes(filePath));

            // compile and set tex
            using var stream = new FileStream(filePath, FileMode.Open);
            MexImage tex = ImageConverter.FromPNG(stream, Width, Height, Format, TlutFormat);
            workspace.FileManager.Set(path + ".tex", tex.ToByteArray());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public MexImage? GetSourceImage(MexWorkspace workspace)
        {
            var path = GetFullPath(workspace);

            var stream = workspace.FileManager.GetStream(path + ".png");

            if (stream == null)
                return null;

            var tex = ImageConverter.FromPNG(stream, Format, TlutFormat);
            stream.Close();

            return tex;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public MexImage? GetTexFile(MexWorkspace workspace)
        {
            var texPath = GetFullPath(workspace) + ".tex";

            if (!workspace.FileManager.Exists(texPath))
                return null;

            var stream = workspace.FileManager.Get(texPath);
            return MexImage.FromByteArray(stream);
        }
    }
}
