using mexLib.AssetTypes;
using mexLib.Utilties;
using System.ComponentModel;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public class MexCostume : MexReactiveObject
    {

        [DisplayName("Name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        private string _name = "New Costume";

        [DisplayName("Color Smash Group")]
        public int ColorSmashGroup { get => _colorSmashGroup; set { _colorSmashGroup = value; OnPropertyChanged(); } }
        private int _colorSmashGroup = 0;

        [DisplayName("File")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public MexCostumeVisibilityFile File { get; set; } = new MexCostumeVisibilityFile();

        [Browsable(false)]
        [JsonInclude]
        public string? Icon { get => IconAsset.AssetFileName; internal set => IconAsset.AssetFileName = value; }

        [DisplayName("Stock Icon")]
        [JsonIgnore]
        [Browsable(false)]
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
        [Browsable(false)]
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
        public void DeleteFiles(MexWorkspace workspace)
        {
            File.DeleteFiles(workspace);
        }
        /// <summary>
        /// 
        /// </summary>
        public void DeleteAssets(MexWorkspace workspace)
        {
            CSPAsset.Delete(workspace);
            IconAsset.Delete(workspace);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<MexCostume> FromZip(MexWorkspace workspace, Stream zipstream, StringBuilder log)
        {
            Dictionary<string, MexCostume> costumes = new();

            // first scan file for dat files
            using (ZipArchive zip = new(zipstream, ZipArchiveMode.Read, true))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    using Stream fstream = entry.Open();
                    using MemoryStream stream = new();
                    fstream.CopyTo(stream);
                    fstream.Close();

                    // dat assets
                    if (entry.Name.EndsWith(".dat"))
                    {
                        // file
                        if (entry.Name.StartsWith("PlKb"))
                        {
                            string targetPath = workspace.GetFilePath(entry.Name.Replace(" ", "_"));
                            string path = workspace.FileManager.GetUniqueFilePath(targetPath);

                            string costume_key = Path.GetFileNameWithoutExtension(path)[4..];
                            if (!costumes.ContainsKey(costume_key))
                                costumes.Add(costume_key, new MexCostume());

                            workspace.FileManager.Set(path, stream.ToArray());

                            log.AppendLine($"Imported \"{entry.FullName}\" as kirby costume");
                        }
                        else
                        {
                            string targetPath = workspace.GetFilePath(entry.Name.Replace(" ", "_"));
                            string path = workspace.FileManager.GetUniqueFilePath(targetPath);

                            string costume_key = Path.GetFileNameWithoutExtension(path)[4..6];
                            if (!costumes.ContainsKey(costume_key))
                                costumes.Add(costume_key, new MexCostume());

                            workspace.FileManager.Set(path, stream.ToArray());
                            costumes[costume_key].Name = Path.GetFileNameWithoutExtension(path);
                            costumes[costume_key].File.FileName = Path.GetFileName(path);

                            log.AppendLine($"Imported \"{entry.FullName}\" as costume");
                        }
                    }
                }
            }

            zipstream.Position = 0;

            // search for assets
            using (ZipArchive zip = new(zipstream))
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    using Stream fstream = entry.Open();
                    using MemoryStream stream = new();
                    fstream.CopyTo(stream);
                    fstream.Close();

                    if (costumes.Count == 1)
                    {
                        // assets
                        switch (entry.Name.ToLower())
                        {
                            case "stc.png":
                            case "stock.png":
                            case "icon.png":
                                log.AppendLine($"Imported \"{entry.FullName}\" as stock icon");
                                costumes.Values.ToArray()[0].IconAsset.SetFromImageFile(workspace, stream);
                                break;
                            case "csp.png":
                            case "portrait.png":
                            case "select.png":
                                log.AppendLine($"Imported \"{entry.FullName}\" as portrait");
                                costumes.Values.ToArray()[0].CSPAsset.SetFromImageFile(workspace, stream);
                                break;
                        }
                    }
                    //else
                    //{
                    //    if (Path.GetExtension(entry.Name).ToLower() == ".png")
                    //    {
                    //        var name = Path.GetFileName(entry.Name);
                    //        if (name.Length == 6 && name.StartsWith("Pl"))
                    //        {
                    //            var key = name[4..6];
                    //            if (costumes.ContainsKey(key))
                    //            {
                    //                costumes[key].IconAsset.SetFromImageFile(workspace, stream);
                    //            }
                    //        }
                    //    }
                    //}
                }

            // grab symbols and return 
            foreach (KeyValuePair<string, MexCostume> c in costumes)
            {
                if (!string.IsNullOrEmpty(c.Value.File.FileName))
                {
                    c.Value.File.GetSymbolFromFile(workspace);
                    yield return c.Value;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="datpath"></param>
        /// <returns></returns>
        public static MexCostume? FromDATFile(MexWorkspace workspace, string datpath, out string log)
        {
            string name = Path.GetFileName(datpath);

            // add file to filesystem
            string targetPath = workspace.GetFilePath(name);
            string path = workspace.FileManager.GetUniqueFilePath(targetPath);
            workspace.FileManager.Set(path, workspace.FileManager.Get(datpath));

            // setup costume
            MexCostume costume = new()
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
        public void PackToZip(MexWorkspace workspace, Stream stream)
        {
            // costume pack to zip
            using ZipWriter zip = new(stream);
            zip.TryWriteFile(workspace, File.FileName, File.FileName);
            zip.TryWriteTextureAsset(workspace, IconAsset, "stc.png");
            zip.TryWriteTextureAsset(workspace, CSPAsset, "csp.png");
        }
    }
}
