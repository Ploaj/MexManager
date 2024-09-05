using mexLib.Attributes;
using mexLib.Utilties;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace mexLib
{
    public class MexCostume : MexAssetContainerBase
    {
        [Browsable(false)]
        public MexCostumeFile File { get; set; } = new MexCostumeFile();

        [Browsable(false)]
        public MexCostumeFile KirbyFile { get; set; } = new MexCostumeFile();

        [DisplayName("Name")]
        public string Name { get; set; } = "New Costume";

        [DisplayName("Filename")]
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
                if (MexWorkspace.LastOpened != null)
                    File.GetSymbolFromFile(MexWorkspace.LastOpened);
                OnPropertyChanged();
                SetCostumeVisibilityFromSymbols();
            }
        }

        [DisplayName("Kirby Filename")]
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
                if (MexWorkspace.LastOpened != null)
                    KirbyFile.GetSymbolFromFile(MexWorkspace.LastOpened);
            }
        }

        [DisplayName("Visibility Table Index")]
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

        [DisplayName("Stock Icon")]
        [MexTextureAsset]
        [MexTextureFormat(HSDRaw.GX.GXTexFmt.CI4, HSDRaw.GX.GXTlutFmt.RGB5A3)]
        [MexTextureSize(24, 24)]
        [MexFilePathValidator(MexFilePathType.Assets)]
        public string Icon { get => _icon; set { _icon = value; OnPropertyChanged(); } }
        private string _icon = "";

        [DisplayName("Character Select Portrait")]
        [MexTextureAsset]
        [MexTextureFormat(HSDRaw.GX.GXTexFmt.CI8, HSDRaw.GX.GXTlutFmt.RGB5A3)]
        [MexTextureSize(136, 188)]
        [MexFilePathValidator(MexFilePathType.Assets)]
        public string CSP { get => _csp; set { _csp = value; OnPropertyChanged(); } }
        private string _csp = "";

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
        public static MexFilePathError? CheckFileName(MexWorkspace workspace, string fullPath)
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

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public string GetIconPath(MexWorkspace workspace)
        {
            return workspace.GetAssetPath(Icon.Replace(".png", ".tex"));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public string GetCSPPath(MexWorkspace workspace)
        {
            return workspace.GetAssetPath(CSP.Replace(".png", ".tex"));
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
        /// <param name="ws"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void GenerateAssetPaths(MexWorkspace ws)
        {
            Icon = GeneratePathIfNotExists(ws, Icon, "icons", "icon", ".png");
            CSP = GeneratePathIfNotExists(ws, CSP, "csp", "csp", ".png");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void RemoveAssets(MexWorkspace ws)
        {
            RemoveTexAsset(ws, Icon);
            RemoveTexAsset(ws, CSP);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static MexCostume? FromZip(MexWorkspace workspace, string zipPath, out string log)
        {
            var l = new StringBuilder();

            var costume = new MexCostume()
            {
                Name = Path.GetFileNameWithoutExtension(zipPath)
            };
            costume.GenerateAssetPaths(workspace);

            using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Read))
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    // assets
                    switch (entry.Name.ToLower())
                    {
                        case "stc.png":
                        case "stock.png":
                        case "icon.png":
                            l.AppendLine($"Imported \"{entry.FullName}\" as stock icon");
                            workspace.FileManager.Set(workspace.GetAssetPath(costume.Icon), entry.Extract());
                            // TODO: convert to tex and add asset
                            //workspace.FileManager.Set(workspace.GetAssetPath(costume.Icon.Replace(".png", ".tex")), entry.Extract());
                            break;
                        case "csp.png":
                        case "portrait.png":
                        case "select.png":
                            l.AppendLine($"Imported \"{entry.FullName}\" as portrait");
                            // TODO: convert to tex and add asset
                            workspace.FileManager.Set(workspace.GetAssetPath(costume.CSP), entry.Extract());
                            break;
                    }
                    // dat assets
                    if (entry.Name.EndsWith(".dat"))
                    {
                        // file
                        if (entry.Name.StartsWith("PlKb"))
                        {
                            var targetPath = workspace.GetFilePath(entry.Name);
                            var path = workspace.FileManager.GetUniqueFilePath(targetPath);

                            workspace.FileManager.Set(path, entry.Extract());
                            costume.KirbyFileName = Path.GetFileName(path);

                            l.AppendLine($"Imported \"{entry.FullName}\" as kirby costume");
                        }
                        else
                        {
                            var targetPath = workspace.GetFilePath(entry.Name);
                            var path = workspace.FileManager.GetUniqueFilePath(targetPath);

                            workspace.FileManager.Set(path, entry.Extract());
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
        public static MexCostume FromDATFile(MexWorkspace workspace, string datpath)
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

            return costume;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipPath"></param>
        /// <param name="workspace"></param>
        public void PackToZip(MexWorkspace workspace, string zipPath)
        {
            using var zip = new ZipWriter(zipPath);

            // normal dat file
            var path = workspace.GetFilePath(FileName);
            if (workspace.FileManager.Exists(path))
            {
                zip.Write(FileName, workspace.FileManager.Get(path));
            }

            // kirby dat file
            var kpath = workspace.GetFilePath(KirbyFileName);
            if (workspace.FileManager.Exists(kpath))
            {
                zip.Write(KirbyFileName, workspace.FileManager.Get(kpath));
            }

            // csp
            var cspPath = workspace.GetAssetPath(CSP);
            if (workspace.FileManager.Exists(cspPath))
            {
                zip.Write("csp.png", workspace.FileManager.Get(cspPath));
            }

            // stock
            var stockPath = workspace.GetAssetPath(Icon);
            if (workspace.FileManager.Exists(stockPath))
            {
                zip.Write("stc.png", workspace.FileManager.Get(stockPath));
            }
        }
    }
}
