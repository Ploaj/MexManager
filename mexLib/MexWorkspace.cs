using GCILib;
using HSDRaw.Common;
using HSDRaw.MEX;
using mexLib.Installer;
using mexLib.MexScubber;
using mexLib.Utilties;
using System.Diagnostics;

namespace mexLib
{
    public class MexWorkspace
    {
        public static MexWorkspace? LastOpened { get; internal set; }

        public byte VersionMajor { get; } = 1;
        public byte VersionMinor { get; } = 1;

        private string FilePath { get; set; } = "";

        public string ProjectFilePath { get; internal set; } = "";

        public MexProject Project { get; internal set; } = new MexProject();

        public FileManager FileManager { get; internal set; } = new FileManager();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetDataPath(string fileName)
        {
            return $"{FilePath}\\data\\{fileName}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetFilePath(string fileName) 
        { 
            return $"{FilePath}\\files\\{fileName}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetAssetPath(string fileName)
        {
            return $"{FilePath}\\assets\\{fileName}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetSystemPath(string fileName)
        {
            return $"{FilePath}\\sys\\{fileName}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long GetFileSize(string fileName)
        {
            var path = GetFilePath(fileName);
            if (FileManager.Exists(path))
                return new FileInfo(path).Length;
            return 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBannerRGBA()
        {
            GCBanner Banner = new (GetFilePath("opening.bnr"));
            return Banner.GetBannerImageRGBA8();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isoPath"></param>
        public static MexWorkspace NewWorkspace(
            string projectFile,
            string isoPath,
            string gctPath,
            string codesPath,
            MEX_Stock? stock)
        {
            if (!File.Exists(isoPath))
                throw new FileNotFoundException("Melee ISO not found");

            var projectPath = Path.GetDirectoryName(projectFile) + "\\";

            var sys = projectPath + "\\sys";
            if (!Directory.Exists(sys))
                Directory.CreateDirectory(sys);

            var files = projectPath + "\\files";
            if (!Directory.Exists(files))
                Directory.CreateDirectory(files);

            // create workspace
            var workspace = new MexWorkspace()
            {
                FilePath = projectPath,
                ProjectFilePath = projectFile,
            };

            using (GCISO iso = new(isoPath))
            {
                File.WriteAllBytes(Path.Combine(sys, "main.dol"), iso.DOLData);
                File.WriteAllBytes(Path.Combine(sys, "apploader.img"), iso.AppLoader);
                File.WriteAllBytes(Path.Combine(sys, "boot.bin"), iso.Boot);
                File.WriteAllBytes(Path.Combine(sys, "bi2.bin"), iso.Boot2);

                File.WriteAllBytes(workspace.GetFilePath("MnSlChr.usd"), iso.GetFileData("MnSlChr.usd"));
                File.WriteAllBytes(workspace.GetFilePath("MnSlMap.usd"), iso.GetFileData("MnSlMap.usd"));

                // write files
                //int index = 0;
                //foreach (var file in iso.GetAllFilePaths())
                //{
                //    var output = files + "/" + file;
                //    var dir = Path.GetDirectoryName(output);

                //    if (dir != null && !Directory.Exists(dir))
                //        Directory.CreateDirectory(dir);

                //    File.WriteAllBytes(output, iso.GetFileData(file));

                //    index++;
                //    //ReportProgress(null, new ProgressChangedEventArgs((int)((index / (float)files.Length) * 99), null));
                //}

                //ReportProgress(null, new ProgressChangedEventArgs(100, null));
            }

            // copy codes
            Directory.CreateDirectory(workspace.GetDataPath(""));
            Directory.CreateDirectory(workspace.GetAssetPath(""));
            File.Copy(gctPath, workspace.GetDataPath("codes.gct"), true);
            File.Copy(codesPath, workspace.GetDataPath("codes.ini"), true);

            // install mex system
            var dol = new MexDOL(workspace.GetDOL());
            var error = MexInstaller.Install(workspace, dol);
            if (error != null)
            {
                throw new Exception(error.Message);
            }

            // extract stock icons
            if (stock != null)
            {
                stock.GetFighterIcons(out List<List<HSD_TOBJ>> fighters, out List<HSD_TOBJ> reserved);
                var path = workspace.GetAssetPath("icons\\");
                Directory.CreateDirectory(path);
                int index = 0;
                foreach (var icon in reserved)
                {
                    new MexImage(icon).Save(Path.Combine(path, $"{index++:D3}.tex"));
                }
                for (int f = 0; f < fighters.Count; f++)
                {
                    for (int c = 0; c < fighters[f].Count; c++)
                    {
                        var costumeName = Path.GetFileNameWithoutExtension(workspace.Project.Fighters[f].Costumes.Costumes[c].FileName);
                        new MexImage(fighters[f][c]).Save(Path.Combine(path, $"{costumeName}.tex"));
                    }
                }
            }

            // save dol just this once
            dol.Save(workspace.GetSystemPath("main.dol"));

            // save workspace
            workspace.Save();

            LastOpened = workspace;

            return workspace;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetDOL()
        {
            var sys = FilePath + "\\sys";
            return File.ReadAllBytes($"{sys}\\main.dol");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool TryOpenWorkspace(string projectFilePath, out MexWorkspace? workspace)
        {
            workspace = null;

            // validate
            if (!File.Exists(projectFilePath))
                return false;

            // create workspace
            workspace = new MexWorkspace()
            {
                FilePath = Path.GetDirectoryName(projectFilePath) + "//",
                ProjectFilePath = projectFilePath,
            };

            workspace.Project = MexProject.LoadFromFile(workspace);

            LastOpened = workspace;

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            var sw = new Stopwatch();

            sw.Start();
            MxDtCompiler.Compile(this);
            sw.Stop();

            Debug.WriteLine($"Compile MxDt {sw.Elapsed}");

            sw.Start();
            GeneratePlCo.Compile(this);
            sw.Stop();

            Debug.WriteLine($"Compile PlCo {sw.Elapsed}");

            sw.Restart();
            GenerateIfAll.Compile(this);
            sw.Stop();

            Debug.WriteLine($"Generate IfAll {sw.Elapsed}");

            // generate mexSelectChr
            sw.Restart();
            GenerateMexSelectChr.Compile(this);
            sw.Stop();

            Debug.WriteLine($"Generate MnSlChr {sw.Elapsed}");

            // generate mexSelectStage
            sw.Restart();
            GenerateMexSelectMap.Compile(this);
            sw.Stop();

            Debug.WriteLine($"Generate MnSlMap {sw.Elapsed}");

            // TODO: generate sem/smst/ssm

            // TODO: compile codes
            sw.Restart();
            FileManager.Set(GetFilePath("codes.gct"), File.ReadAllBytes(GetDataPath("codes.gct")));
            sw.Stop();

            Debug.WriteLine($"Compile codes {sw.Elapsed}");

            sw.Restart();
            Project.Save(this);
            sw.Stop();

            Debug.WriteLine($"Save Project Data {sw.Elapsed}");

            sw.Start();
            FileManager.Save();
            sw.Stop();

            Debug.WriteLine($"Save files {sw.Elapsed}");
        }
    }
}
