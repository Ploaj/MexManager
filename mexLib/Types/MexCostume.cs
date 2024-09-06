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
        [Browsable(false)]
        public MexCostumeFile File { get; set; } = new MexCostumeFile();

        [Browsable(false)]
        public MexCostumeFile KirbyFile { get; set; } = new MexCostumeFile();

        [DisplayName("Name")]
        public string Name { get; set; } = "New Costume";

        [DisplayName("File")]
        [MexFilePathValidator(MexFilePathType.Files)]
        [MexFilePathValidatorCallback("CheckFileName")]
        public string FileName
        {
            get
            {
                return File.FileName;
            }
            set
            {
                File.FileName = value;
                //if (MexWorkspace.LastOpened != null)
                //    File.GetSymbolFromFile(MexWorkspace.LastOpened);
                SetCostumeVisibilityFromSymbols();
            }
        }

        [DisplayName("Kirby File")]
        [MexFilePathValidator(MexFilePathType.Files)]
        public string KirbyFileName
        {
            get
            {
                return KirbyFile.FileName;
            }
            set
            {
                KirbyFile.FileName = value;
                //if (MexWorkspace.LastOpened != null)
                //    KirbyFile.GetSymbolFromFile(MexWorkspace.LastOpened);
            }
        }

        [DisplayName("Original Costume Index")]
        public int VisibilityIndex
        {
            get => _visibilityIndex;
            set
            {
                _visibilityIndex = value;
                OnPropertyChanged();
            }
        }
        public int _visibilityIndex = 0;

        [Browsable(false)]
        [JsonInclude]
        public string? Icon { get => IconAsset.AssetFileName; internal set => IconAsset.AssetFileName = value; }

        [DisplayName("Character Select Portrait")]
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

        [DisplayName("Character Select Portrait")]
        [JsonIgnore]
        public MexTextureAsset CSPAsset { get; set; } = new MexTextureAsset()
        {
            AssetPath = "csp/csp",
            Width = 136,
            Height = 188,
            Format = HSDRaw.GX.GXTexFmt.CI8,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
        };

        public void SetCostumeVisibilityFromSymbols()
        {
            switch (File.JointSymbol)
            {
                case "PlyPeach5KYe_Share_joint": VisibilityIndex = 1; return;

                case "PlyPikachu5KNr_Share_joint": VisibilityIndex = 0; return;
                case "PlyPikachu5KRd_Share_joint": VisibilityIndex = 1; return;
                case "PlyPikachu5KBu_Share_joint": VisibilityIndex = 2; return;
                case "PlyPikachu5KGr_Share_joint": VisibilityIndex = 3; return;

                case "PlyPichu5KNr_Share_joint": VisibilityIndex = 0; return;
                case "PlyPichu5KRd_Share_joint": VisibilityIndex = 1; return;
                case "PlyPichu5KBu_Share_joint": VisibilityIndex = 2; return;
                case "PlyPichu5KGr_Share_joint": VisibilityIndex = 3; return;
            }

            if (File.JointSymbol.Contains("PlyPeach5K") ||
                File.JointSymbol.Contains("PlyPikachu5K") ||
                File.JointSymbol.Contains("PlyPichu5K"))
            {
                VisibilityIndex = 0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public MexFilePathError? CheckFileName(MexWorkspace workspace, string fullPath)
        {
            using Stream? stream = workspace.FileManager.GetStream(fullPath);

            if (stream == null)
                return new MexFilePathError("Unable to read file");

            if (!ArchiveTools.IsValidHSDFile(stream))
                return new MexFilePathError("Not a valid HSD file");

            bool passing = false;
            foreach (var s in ArchiveTools.GetSymbols(stream))
            {
                if (s.EndsWith("_joint"))
                    passing = true;
            }

            if (!passing)
                return new MexFilePathError("joint not found in dat");

            File.GetSymbolFromFile(workspace);

            return null;
        }
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
        public static MexCostume? FromZip(MexWorkspace workspace, string zipPath, out string log)
        {
            // TODO: multiple costumes in one file

            var l = new StringBuilder();

            var costume = new MexCostume()
            {
                Name = Path.GetFileNameWithoutExtension(zipPath)
            };

            using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Read))
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    using var fstream = entry.Open();
                    using var stream = new MemoryStream();
                    fstream.CopyTo(stream);
                    fstream.Close();

                    // assets
                    switch (entry.Name.ToLower())
                    {
                        case "stc.png":
                        case "stock.png":
                        case "icon.png":
                            l.AppendLine($"Imported \"{entry.FullName}\" as stock icon");
                            costume.IconAsset.SetFromImageFile(workspace, stream);
                            break;
                        case "csp.png":
                        case "portrait.png":
                        case "select.png":
                            l.AppendLine($"Imported \"{entry.FullName}\" as portrait");
                            costume.CSPAsset.SetFromImageFile(workspace, stream);
                            break;
                    }
                    // dat assets
                    if (entry.Name.EndsWith(".dat"))
                    {
                        // file
                        if (entry.Name.StartsWith("PlKb"))
                        {
                            var targetPath = workspace.GetFilePath(entry.Name.Replace(" ", "_"));
                            var path = workspace.FileManager.GetUniqueFilePath(targetPath);

                            workspace.FileManager.Set(path, stream.ToArray());
                            costume.KirbyFileName = Path.GetFileName(path);

                            l.AppendLine($"Imported \"{entry.FullName}\" as kirby costume");
                        }
                        else
                        {
                            var targetPath = workspace.GetFilePath(entry.Name.Replace(" ", "_"));
                            var path = workspace.FileManager.GetUniqueFilePath(targetPath);

                            workspace.FileManager.Set(path, stream.ToArray());
                            costume.FileName = Path.GetFileName(path);

                            l.AppendLine($"Imported \"{entry.FullName}\" as costume");
                        }
                    }
                }

            costume.File.GetSymbolFromFile(workspace);
            costume.KirbyFile.GetSymbolFromFile(workspace);

            log = l.ToString();
            if (!string.IsNullOrEmpty(costume.FileName))
                return costume;
            else
                return null;
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
                FileName = Path.GetFileName(path),
            };
            costume.File.GetSymbolFromFile(workspace);

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
            zip.TryWriteFile(workspace, FileName, FileName);
            zip.TryWriteFile(workspace, KirbyFileName, KirbyFileName);
            zip.TryWriteTextureAsset(workspace, IconAsset, "stc.png");
            zip.TryWriteTextureAsset(workspace, CSPAsset, "csp.png");
        }
    }
}
