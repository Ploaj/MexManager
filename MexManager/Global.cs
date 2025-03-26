using MeleeMedia.Audio;
using mexLib;
using mexLib.Types;
using mexLib.Utilties;
using MexManager.Views;
using System;
using System.Diagnostics;
using System.IO;

namespace MexManager
{
    public static class Global
    {
        public static string[] LaunchArgs { get; set; }

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
            // TODO: check update for tool
            //Updater.UpdateCodes();
        }

        public static void PlayMusic(MexMusic music)
        {
            if (Workspace != null)
            {
                var hps = Workspace.GetFilePath($"audio\\{music.FileName}");

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
            var mainCode = CodeLoader.FromGCT(File.ReadAllBytes(MexCodePath));
            var defaultCodes = CodeLoader.FromINI(File.ReadAllBytes(MexAddCodePath));

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
            var mainCode = CodeLoader.FromGCT(File.ReadAllBytes(MexCodePath));
            var defaultCodes = CodeLoader.FromINI(File.ReadAllBytes(MexAddCodePath));
            var path = Path.GetDirectoryName(Path.GetDirectoryName(mexdolPath));

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
        public static bool LoadWorkspace(string filepath)
        {
            if (Workspace != null)
            {
                CloseWorkspace();
            }

            if (MexWorkspace.TryOpenWorkspace(filepath, out MexWorkspace? workspace))
            {
                var mainCode = CodeLoader.FromGCT(File.ReadAllBytes(MexCodePath));
                if (workspace != null && mainCode != null)
                    workspace.Project.MainCode = mainCode;
                Workspace = workspace;
                return true;
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

            Workspace.Save();
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
            ProcessStartInfo processStartInfo = new ()
            {
                FileName = exePath,
                Arguments = parameters,
                RedirectStandardOutput = true, // Optional: to capture the output
                RedirectStandardError = true,  // Optional: to capture errors
                UseShellExecute = false,       // Needed to redirect output
                CreateNoWindow = true          // Optional: hide the window
            };

            using Process process = new ();
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
    }
}
