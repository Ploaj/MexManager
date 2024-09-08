using mexLib;
using mexLib.Types;
using mexLib.Utilties;
using MexManager.Views;
using System;
using System.IO;

namespace MexManager
{
    public static class Global
    {
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
            // TODO: check update for codes and tool
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
            using (var s = new MemoryStream())
            {
                // load codes
                var mainCode = CodeLoader.FromGCT(File.ReadAllBytes(MexCodePath));
                var defaultCodes = CodeLoader.FromINI(File.ReadAllBytes(MexAddCodePath));

                Workspace = MexWorkspace.NewWorkspace(
                    filepath,
                    App.Settings.MeleePath,
                    mainCode,
                    defaultCodes);
            }

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
    }
}
