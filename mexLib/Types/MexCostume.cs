using mexLib.AssetTypes;
using mexLib.Attributes;
using mexLib.Utilties;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace mexLib
{
    public class MexCostume : MexReactiveObject
    {

        [DisplayName("Name")]
        public string Name { get; set; } = "New Costume";

        [DisplayName("Main File")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public MexCostumeVisibilityFile File { get; set; } = new MexCostumeVisibilityFile();

        [DisplayName("Kirby File")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public MexCostumeFile KirbyFile { get; set; } = new MexCostumeFile();

        [Browsable(false)]
        [JsonInclude]
        public string? Icon { get => IconAsset.AssetFileName; internal set => IconAsset.AssetFileName = value; }

        [DisplayName("Stock Icon")]
        [JsonIgnore]
        public MexTextureAsset IconAsset { get; set; } = new MexTextureAsset()
        {
            AssetPath = "icons/ft",
            Width = 24,
            Height = 24,
            Format = HSDRaw.GX.GXTexFmt.CI4,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
        };

        [Browsable(false)]
        [JsonInclude]
        public string? CSP { get => CSPAsset.AssetFileName; internal set => CSPAsset.AssetFileName = value; }

        [DisplayName("Portrait")]
        [JsonIgnore]
        public MexTextureAsset CSPAsset { get; set; } = new MexTextureAsset()
        {
            AssetPath = "csp/csp",
            Width = 136,
            Height = 188,
            Format = HSDRaw.GX.GXTexFmt.CI8,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
        };
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<MexCostume> FromZip(MexWorkspace workspace, string zipPath, StringBuilder log)
        {
            Dictionary<string, MexCostume> costumes = new ();

            //using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Read))
            //    foreach (ZipArchiveEntry entry in zip.Entries)
            //    {
            //        using var fstream = entry.Open();
            //        using var stream = new MemoryStream();
            //        fstream.CopyTo(stream);
            //        fstream.Close();

            //        // assets
            //        switch (entry.Name.ToLower())
            //        {
            //            case "stc.png":
            //            case "stock.png":
            //            case "icon.png":
            //                l.AppendLine($"Imported \"{entry.FullName}\" as stock icon");
            //                costume.IconAsset.SetFromImageFile(workspace, stream);
            //                break;
            //            case "csp.png":
            //            case "portrait.png":
            //            case "select.png":
            //                l.AppendLine($"Imported \"{entry.FullName}\" as portrait");
            //                costume.CSPAsset.SetFromImageFile(workspace, stream);
            //                break;
            //        }
            //        // dat assets
            //        if (entry.Name.EndsWith(".dat"))
            //        {
            //            // file
            //            if (entry.Name.StartsWith("PlKb"))
            //            {
            //                var targetPath = workspace.GetFilePath(entry.Name.Replace(" ", "_"));
            //                var path = workspace.FileManager.GetUniqueFilePath(targetPath);

            //                workspace.FileManager.Set(path, stream.ToArray());
            //                costume.KirbyFileName = Path.GetFileName(path);

            //                l.AppendLine($"Imported \"{entry.FullName}\" as kirby costume");
            //            }
            //            else
            //            {
            //                var targetPath = workspace.GetFilePath(entry.Name.Replace(" ", "_"));
            //                var path = workspace.FileManager.GetUniqueFilePath(targetPath);

            //                workspace.FileManager.Set(path, stream.ToArray());
            //                costume.FileName = Path.GetFileName(path);

            //                l.AppendLine($"Imported \"{entry.FullName}\" as costume");
            //            }
            //        }
            //    }

            //costume.File.GetSymbolFromFile(workspace);
            //costume.KirbyFile.GetSymbolFromFile(workspace);

            foreach (var c in costumes)
            {
                if (!string.IsNullOrEmpty(c.Value.File.FileName))
                yield return c.Value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="datpath"></param>
        /// <returns></returns>
        public static MexCostume? FromDATFile(MexWorkspace workspace, string datpath, out string log)
        {
            var name = Path.GetFileName(datpath);

            // add file to filesystem
            var targetPath = workspace.GetFilePath(name);
            var path = workspace.FileManager.GetUniqueFilePath(targetPath);
            workspace.FileManager.Set(path, workspace.FileManager.Get(datpath));

            // setup costume
            var costume = new MexCostume()
            {
                Name = Path.GetFileNameWithoutExtension(datpath),
            };
            costume.File.FileName = Path.GetFileName(path);
            costume.File.GetSymbolFromFile(workspace, datpath);

            if (string.IsNullOrEmpty(costume.File.JointSymbol))
            {
                log = "Joint Symbol not found";
                return null;
            }
            log = "";
            return costume;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipPath"></param>
        /// <param name="workspace"></param>
        public void PackToZip(MexWorkspace workspace, string zipPath)
        {
            // costume pack to zip
            using var zip = new ZipWriter(zipPath);
            zip.TryWriteFile(workspace, File.FileName, File.FileName);
            zip.TryWriteFile(workspace, KirbyFile.FileName, KirbyFile.FileName);
            zip.TryWriteTextureAsset(workspace, IconAsset, "stc.png");
            zip.TryWriteTextureAsset(workspace, CSPAsset, "csp.png");
        }
    }
}
