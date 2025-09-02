using MeleeMedia.Audio;
using mexLib;
using mexLib.Installer;
using mexLib.Types;
using mexLib.Utilties;
using MexManager.Tools;
using MexManager.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MexManager
{
    public static class Global
    {
        public static string[] LaunchArgs { get; set; } = [];

        public static MexWorkspace? Workspace { get; internal set; }

        public static FileManager Files
        {
            get
            {
                if (Workspace != null)
                    return Workspace.FileManager;

                return new();
            }
        }

        public static MexCode? MEXCode { get; internal set; }

        public static readonly string MexCodePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "codes.gct");
        public static readonly string MexAddCodePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "codes.ini");

        public static void Initialize()
        {
        }

        public static void PlayMusic(MexMusic music)
        {
            if (Workspace != null)
            {
                string hps = Workspace.GetFilePath($"audio/{music.FileName}");

                if (Files.Exists(hps))
                {
                    MainView.GlobalAudio?.LoadHPS(Files.Get(hps));
                    MainView.GlobalAudio?.Play();
                }
                else
                {
                    MessageBox.Show($"Could not find \"{music.FileName}\"", "File not found", MessageBox.MessageBoxButtons.Ok);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsp"></param>
        public static void PlaySound(DSP dsp)
        {
            MainView.GlobalAudio?.LoadDSP(dsp);
            MainView.GlobalAudio?.Play();
        }

        public static void StopMusic()
        {
            MainView.GlobalAudio?.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static MexWorkspace? CreateWorkspace(string filepath)
        {
            // load codes
            MexCode? mainCode = CodeLoader.FromGCT(File.ReadAllBytes(MexCodePath));
            System.Collections.Generic.IEnumerable<MexCode> defaultCodes = CodeLoader.FromINI(File.ReadAllBytes(MexAddCodePath));

            if (mainCode == null)
                return null;

            Workspace = MexWorkspace.NewWorkspace(
                filepath,
                App.Settings.MeleePath,
                mainCode,
                defaultCodes);

            return Workspace;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static MexWorkspace? CreateWorkspaceFromMex(string mexdolPath)
        {
            // load codes
            MexCode? mainCode = CodeLoader.FromGCT(File.ReadAllBytes(MexCodePath));
            System.Collections.Generic.IEnumerable<MexCode> defaultCodes = CodeLoader.FromINI(File.ReadAllBytes(MexAddCodePath));
            string? path = Path.GetDirectoryName(Path.GetDirectoryName(mexdolPath));

            if (path == null || mainCode == null)
                return null;

            string projectPath = Path.Combine(path, "project.mexproj");

            Workspace = MexWorkspace.CreateFromMexFileSystem(
                projectPath,
                path,
                mainCode,
                defaultCodes);

            return Workspace;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private static bool TryOpenWorkspace(string filepath, out string error, out bool isomissing)
        {
            if (MexWorkspace.TryOpenWorkspace(filepath, out MexWorkspace? workspace, out error, out isomissing))
            {
                // set working workspace
                Workspace = workspace;
                Global.ReloadCodes();
                return true;
            }

            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        private static async Task<bool> TryExtractISO(string iso, string filepath)
        {
            if (!File.Exists(iso))
                return false;

            var output = Path.GetDirectoryName(filepath) + "/";

            if (!Directory.Exists(output))
                return false;

            await ProgressWindow.DisplayProgress((w) =>
            {
                ISOTool.ExtractToFileSystem(iso, output, (r, t) =>
                {
                    w.ReportProgress(t.ProgressPercentage, t.UserState);
                });
            });

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static async Task<bool> LoadWorkspace(string filepath)
        {
            if (Workspace != null)
            {
                CloseWorkspace();
            }

            if (!TryOpenWorkspace(filepath, out string error, out bool isomissing))
            {
                await MessageBox.Show(error, "Open Project Error", MessageBox.MessageBoxButtons.Ok);

                if (isomissing)
                {
                    if (!File.Exists(filepath))
                        return false;
                    
                    MexProject? proj = MexJsonSerializer.Deserialize<MexProject>(filepath);
                    if (proj == null)
                        return false;

                    var res = await MessageBox.Show($"Please select the \"{proj.Build.Name}\" v{proj.Build.MajorVersion}.{proj.Build.MinorVersion}.{proj.Build.PatchVersion} ISO.", "Extract ISO", MessageBox.MessageBoxButtons.OkCancel);

                    if (res == MessageBox.MessageBoxResult.Ok)
                    {
                        var iso = await FileIO.TryOpenFile("Source ISO", "", FileIO.FilterISO);
                        if (iso == null)
                            return false;

                        // extract files
                        if (!await TryExtractISO(iso, filepath))
                            return false;

                        // try to open after extract files
                        if (TryOpenWorkspace(filepath, out error, out _))
                        {
                            return true;
                        }
                        else
                        {
                            await MessageBox.Show(error, "Open Project Error", MessageBox.MessageBoxButtons.Ok);
                            return false;
                        }
                            
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        public static void SaveWorkspace()
        {
            if (Workspace == null)
                return;

            Workspace.Save(Logger.GetWriter());
        }
        /// <summary>
        /// 
        /// </summary>
        public static void CloseWorkspace()
        {
            if (Workspace == null)
                return;

            Workspace = null;
        }
        /// <summary>
        /// 
        /// </summary>
        public static void LaunchGameInDolphin()
        {
            if (Workspace == null)
                return;

            // Define the path to the exe and the parameters
            string exePath = App.Settings.DolphinPath;
            string parameters = $"--exec=\"{Workspace.GetSystemPath("main.dol")}\"";

            // Start a new process
            ProcessStartInfo processStartInfo = new()
            {
                FileName = exePath,
                Arguments = parameters,
                RedirectStandardOutput = true, // Optional: to capture the output
                RedirectStandardError = true,  // Optional: to capture errors
                UseShellExecute = false,       // Needed to redirect output
                CreateNoWindow = true          // Optional: hide the window
            };

            using Process process = new();
            {
                process.StartInfo = processStartInfo;
                process.Start();

                // Optionally, read the output
                //string output = process.StandardOutput.ReadToEnd();
                //string error = process.StandardError.ReadToEnd();

                //process.WaitForExit();  // Wait for the process to exit
                //int exitCode = process.ExitCode;  // Get the exit code if needed

                // Optional: handle the output or errors
                //if (string.IsNullOrEmpty(error))
                //{
                //    System.Console.WriteLine("Output: " + output);
                //}
                //else
                //{
                //    System.Console.WriteLine("Error: " + error);
                //}
            }
        }
        /// <summary>
        /// 
        /// </summary>
        internal static void ReloadCodes()
        {
            // load most recent codes patch
            MexCode? mainCode = CodeLoader.FromGCT(File.ReadAllBytes(MexCodePath));
            if (Workspace != null && mainCode != null)
            {
                // update codes
                Workspace.Project.MainCode = mainCode;
            }
        }
    }
}
