using GCILib;
using HSDRaw;
using HSDRaw.Melee;
using MeleeMedia.Audio;
using mexLib.Generators;
using mexLib.Installer;
using mexLib.MexScubber;
using mexLib.Types;
using mexLib.Utilties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace mexLib
{
    public class MexWorkspace
    {
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
            return $"{FilePath}data\\{fileName}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetFilePath(string fileName) 
        { 
            return $"{FilePath}files\\{fileName}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetAssetPath(string fileName)
        {
            return $"{FilePath}assets\\{fileName}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetSystemPath(string fileName)
        {
            return $"{FilePath}sys\\{fileName}";
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
        /// <param name="projectFile"></param>
        /// <param name="mexPath"></param>
        /// <param name="mainCode"></param>
        /// <returns></returns>
        public static MexWorkspace CreateFromMexFileSystem(
            string projectFile,
            string mexPath,
            MexCode mainCode)
        {
            // get project path
            var projectPath = Path.GetDirectoryName(projectFile) + "\\";

            // create workspace
            var workspace = new MexWorkspace()
            {
                FilePath = projectPath,
                ProjectFilePath = projectFile,
            };

            // TODO: copy files from source
            File.Copy(Path.Combine(mexPath, "files/MxDt.dat"), workspace.GetFilePath("MxDt.dat"), overwrite: true);
            File.Copy(Path.Combine(mexPath, "files/GmRst.usd"), workspace.GetFilePath("GmRst.usd"), overwrite:true);
            File.Copy(Path.Combine(mexPath, "files/MnSlChr.usd"), workspace.GetFilePath("MnSlChr.usd"), overwrite: true);
            File.Copy(Path.Combine(mexPath, "files/MnSlMap.usd"), workspace.GetFilePath("MnSlMap.usd"), overwrite: true);
            File.Copy(Path.Combine(mexPath, "files/SmSt.dat"), workspace.GetFilePath("SmSt.dat"), overwrite: true);
            File.Copy(Path.Combine(mexPath, "files/IfAll.usd"), workspace.GetFilePath("IfAll.usd"), overwrite: true);
            File.Copy(Path.Combine(mexPath, "files/PlCo.dat"), workspace.GetFilePath("PlCo.dat"), overwrite: true);
            File.Copy(Path.Combine(mexPath, "files/audio/us/smash2.sem"), workspace.GetFilePath("audio/us/smash2.sem"), overwrite: true);

            // install mex system
            var error = MxDtImporter.Install(workspace);
            if (error != null)
            {
                throw new Exception(error.Message);
            }

            // save workspace
            workspace.Project.MainCode = mainCode;
            workspace.LoadMiscData();
            workspace.Save();

            return workspace;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isoPath"></param>
        public static MexWorkspace NewWorkspace(
            string projectFile,
            string isoPath,
            MexCode mainCode,
            IEnumerable<MexCode> defaultCodes)
        {
            if (!File.Exists(isoPath))
                throw new FileNotFoundException("Melee ISO not found");

            var projectPath = Path.GetDirectoryName(projectFile) + "\\";

            //Directory.Delete(projectPath + "\\assets", recursive: true);

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

                //File.WriteAllBytes(workspace.GetFilePath("GmRst.usd"), iso.GetFileData("GmRst.usd"));
                //File.WriteAllBytes(workspace.GetFilePath("MnSlChr.usd"), iso.GetFileData("MnSlChr.usd"));
                //File.WriteAllBytes(workspace.GetFilePath("MnSlMap.usd"), iso.GetFileData("MnSlMap.usd"));
                //File.WriteAllBytes(workspace.GetFilePath("SmSt.dat"), iso.GetFileData("SmSt.dat"));
                //File.WriteAllBytes(workspace.GetFilePath("audio/us/smash2.sem"), iso.GetFileData("audio/us/smash2.sem"));

                // extract iso files
                int index = 0;
                foreach (var file in iso.GetAllFilePaths())
                {
                    var output = files + "/" + file;
                    var dir = Path.GetDirectoryName(output);

                    if (dir != null && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.WriteAllBytes(output, iso.GetFileData(file));

                    index++;
                    //ReportProgress(null, new ProgressChangedEventArgs((int)((index / (float)files.Length) * 99), null));
                }

                //ReportProgress(null, new ProgressChangedEventArgs(100, null));
            }

            // create data and asset directories
            Directory.CreateDirectory(workspace.GetDataPath(""));
            Directory.CreateDirectory(workspace.GetAssetPath(""));

            // install mex system
            var dol = new MexDOL(workspace.GetDOL());
            var error = MexInstaller.Install(workspace, dol);
            if (error != null)
            {
                throw new Exception(error.Message);
            }

            // save dol just this once
            dol.Save(workspace.GetSystemPath("main.dol"));

            // save workspace
            workspace.Project.MainCode = mainCode;
            foreach (var c in defaultCodes)
                workspace.Project.Codes.Add(c);

            workspace.LoadMiscData();

            workspace.Save();

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
                FilePath = Path.GetDirectoryName(projectFilePath) + "\\",
                ProjectFilePath = projectFilePath,
            };

            workspace.Project = MexProject.LoadFromFile(workspace);

            //var plco = new HSDRawFile(workspace.FileManager.Get(workspace.GetFilePath("PlCo.dat"))).Roots[0].Data as SBM_ftLoadCommonData;
            //for (uint i = 0; i < workspace.Project.Fighters.Count; i++)
            //{
            //    workspace.Project.Fighters[(int)i].LoadFromPlCo(plco, i);
            //}

            workspace.LoadMiscData();

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            var sw = new Stopwatch();
            var total = new TimeSpan();

            sw.Start();
            MxDtCompiler.Compile(this);
            sw.Stop();
            total += sw.Elapsed;

            Debug.WriteLine($"Compiled MxDt {sw.Elapsed}");

            sw.Start();
            GeneratePlCo.Compile(this);
            sw.Stop();
            total += sw.Elapsed;

            Debug.WriteLine($"Compiled PlCo {sw.Elapsed}");

            sw.Restart();
            GenerateIfAll.Compile(this);
            sw.Stop();
            total += sw.Elapsed;

            Debug.WriteLine($"Generate IfAll {sw.Elapsed}");

            // generate mexSelectChr
            sw.Restart();
            GenerateMexSelectChr.Compile(this);
            sw.Stop();
            total += sw.Elapsed;

            Debug.WriteLine($"Generate MnSlChr {sw.Elapsed}");

            // generate mexSelectStage
            sw.Restart();
            GenerateMexSelectMap.Compile(this);
            sw.Stop();
            total += sw.Elapsed;

            Debug.WriteLine($"Generate MnSlMap {sw.Elapsed}");

            // generate result screen
            sw.Restart();
            GenerateGmRst.Compile(this);
            sw.Stop();
            total += sw.Elapsed;

            Debug.WriteLine($"Generate GmRst {sw.Elapsed}");

            sw.Start();
            GenerateTrophy.Compile(this);
            sw.Stop();
            total += sw.Elapsed;

            Debug.WriteLine($"Compiled Trophies {sw.Elapsed}");

            // generate sem/smst/ssm
            sw.Restart();
            SaveSoundData();
            sw.Stop();
            total += sw.Elapsed;

            Debug.WriteLine($"Compiled sounds {sw.Elapsed}");

            // compile codes
            sw.Restart();
            FileManager.Set(GetFilePath("codes.gct"), CodeLoader.ToGCT(Project.GetAllCodes()));
            sw.Stop();
            total += sw.Elapsed;

            Debug.WriteLine($"Compiled codes {sw.Elapsed}");

            // save data
            sw.Restart();
            Project.Save(this);
            sw.Stop();
            total += sw.Elapsed;

            Debug.WriteLine($"Save Project Data {sw.Elapsed}");

            // save files
            sw.Start();
            FileManager.Save();
            sw.Stop();
            total += sw.Elapsed;

            Debug.WriteLine($"Save files {sw.Elapsed}");

            Debug.WriteLine($"Total Save Time {total}");
        }
        /// <summary>
        /// 
        /// </summary>
        private void SaveSoundData()
        {
            // check if sounds are loaded
            if (!Project.SoundGroups.Any(e => e.Scripts != null))
                return;

            // generate sem
            var sem = new List<SemScript[]>();

            // generate ssm
            List<int> soundIds = new ();
            List<string> soundNames = new ();
            int soundIndex = 0;
            int groupOffset = 0;
            foreach (var group in Project.SoundGroups)
            {
                if (group.Sounds == null || group.Scripts == null)
                {
                    groupOffset += 10000;
                    continue;
                }

                // save ssm
                var ssm = new SSM()
                {
                    Name = group.Name,
                    StartIndex = soundIndex,
                    Sounds = group.Sounds.Select(e => e.DSP).ToArray()
                };

                using MemoryStream stream = new();
                ssm.WriteToStream(stream, out int bufferSize);
                FileManager.Set(GetFilePath($"audio//us//{group.FileName}"), stream.ToArray());

                // generate sem
                var scripts = group.Scripts.Select((e, i) =>
                {
                    var script = new SemScript(e);
                    script.AdjustSoundOffset(soundIndex);
                    soundNames.Add(e.Name);
                    soundIds.Add(i + groupOffset);
                    return script;
                }).ToArray();
                sem.Add(scripts);

                // advance indices
                groupOffset += 10000;
                soundIndex += group.Sounds.Count;
            }

            // save smst
            {
                // generate smst
                var f = new HSDRawFile();
                f.Roots.Add(new HSDRootNode()
                {
                    Name = "smSoundTestLoadData",
                    Data = new smSoundTestLoadData()
                    {
                        SoundModeCount = 2,
                        SoundModes = new string[] { "<<STEREO>>  MONAURAL  ", "  STEREO  <<MONAURAL>>" },

                        SoundBankNames = Project.SoundGroups.Select(e => $"GRPSFX_{Path.GetFileNameWithoutExtension(e.FileName)}").ToArray(),
                        SoundBankCount = Project.SoundGroups.Select(e =>
                        {
                            return e.Scripts == null ? 0 : e.Scripts.Count;
                        }).ToArray(),

                        ScriptCount = Project.SoundGroups.Sum(e => e.Sounds != null ? e.Sounds.Count : 0),
                        SoundNames = soundNames.ToArray(),
                        SoundIDs = soundIds.ToArray(),

                        MusicBanks = Project.Music.Select(e => e.FileName).ToArray(),
                    },
                });
                using MemoryStream stream = new ();
                f.Save(stream);
                FileManager.Set(GetFilePath("SmSt.dat"), stream.ToArray());
            }
            {
                using MemoryStream stream = new();
                SemFile.Compile(stream, sem);
                FileManager.Set(GetFilePath($"audio//us//smash2.sem"), stream.ToArray());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void LoadMiscData()
        {
            var sw = new Stopwatch();

            sw.Start();
            LoadSoundData();
            sw.Stop();

            Debug.WriteLine("Loaded sound data in " + sw.Elapsed.ToString());

            sw.Start();
            TrophyLoader.LoadFromFileSystem(this);
            sw.Stop();

            Debug.WriteLine("Loaded trophy data in " + sw.Elapsed.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        private string? LoadSoundData()
        {
            var semPath = GetFilePath(@"audio//us//smash2.sem");
            if (!FileManager.Exists(semPath))
                return "smash2.sem not found";

            var smstPath = GetFilePath(@"SmSt.dat");
            if (!FileManager.Exists(smstPath))
                return "SmSt.dat not found";

            HSDRawFile smstFile = new(smstPath);
            var smst = smstFile["smSoundTestLoadData"].Data as smSoundTestLoadData;
            if (smst == null)
                return "Error reading SmSt.dat";

            using var semStream = FileManager.GetStream(semPath);
            if (semStream == null)
                return "Error reading smash2.sem";
            var sem = SemFile.Decompile(semStream).ToArray();

            var soundNames = smst.SoundNames;
            var soundids = smst.SoundIDs.ToList();

            int index = 0;
            foreach (var sound in Project.SoundGroups)
            {
                // load ssm
                var ssmPath = GetFilePath($"audio//us//{sound.FileName}");
                int start_index = 0;

                // extract ssm
                if (FileManager.Exists(ssmPath))
                {
                    // open ssm
                    var ssm = new SSM();
                    ssm.Open(sound.FileName, FileManager.GetStream(ssmPath));

                    start_index = ssm.StartIndex;

                    // load sounds from ssm
                    for (int i = 0; i < ssm.Sounds.Length; i++)
                    {
                        while (i >= sound.Sounds.Count)
                            sound.Sounds.Add(new MexSound());

                        sound.Sounds[i].DSP = ssm.Sounds[i];
                    }
                }

                // extract sem
                sound.Scripts = new ObservableCollection<SemScript>();
                if (index < sem.Length)
                {
                    // load script meta data
                    var scripts = sem[index];

                    // get name and adjust sfx id to be relative to bank
                    for (int j = 0; j < scripts.Length; j++)
                    {
                        // load script name
                        var sindex = soundids.IndexOf(index * 10000 + j);
                        if (sindex != -1 && sindex < soundNames.Length)
                        {
                            scripts[j].Name = soundNames[sindex];

                            // adjust sound id to relative
                            scripts[j].AdjustSoundOffset(-start_index);
                        }

                        // give sound name if it's null
                        if (scripts[j].Script.FirstOrDefault(e => e.SemCode == SemCode.Sound)?.Value is int sfxid &&
                            sfxid < sound.Sounds.Count && 
                            string.IsNullOrEmpty(sound.Sounds[sfxid].Name))
                        {
                            var sound_name = soundNames[sindex];
                            sound_name = sound_name.Replace("SFX", "").Trim();
                            if (sound_name.StartsWith("_"))
                                sound_name = sound_name[1..];
                            sound.Sounds[sfxid].Name = sound_name;
                        }

                        // clean script
                        scripts[j].CleanScripts();

                        // add script to sound group
                        sound.Scripts.Add(scripts[j]);
                    }
                }

                index++;
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void ExportISO(string file, ProgressChangedEventHandler args)
        {
            using var iso = new GCISO(
                FileManager.Get(GetSystemPath("boot.bin")),
                FileManager.Get(GetSystemPath("bi2.bin")),
                FileManager.Get(GetSystemPath("apploader.img")),
                FileManager.Get(GetSystemPath("main.dol")));

            var root = GetFilePath("");
            foreach (var f in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
            {
                iso.AddFile(f[root.Length..], f);
            }

            iso.SetAddressTable(MeleeFilelist.FileList);
            iso.Rebuild(file, false, args);
        }
    }
}
