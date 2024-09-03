using Avalonia.Platform;
using HSDRaw;
using HSDRaw.MEX;
using mexLib;
using MexManager.Views;
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
            // extract ifall from our own resources because it's a mess otherwise
            using (var s = new MemoryStream())
            {
                var stream = AssetLoader.Open(new System.Uri("avares://MexManager/Assets/Data/Stc_icns.dat"));
                stream.CopyTo(s);
                HSDRawFile stc_icns = new (s.ToArray());
                var stc = stc_icns.Roots[0].Data as MEX_Stock;

                Workspace = MexWorkspace.NewWorkspace(
                    filepath,
                    App.Settings.MeleePath,
                    Updater.MexCodePath,
                    Updater.MexAddCodePath,
                    stc);
            }

            // convert all .tex to pngs
            foreach (var f in Directory.GetFiles(Workspace.GetAssetPath(""), "*.*", System.IO.SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(f).ToLower();

                if (ext.Equals(".tex"))
                {
                    var newName = f.Replace(".tex", ".png");
                    var image = new MexImage(f);
                    using var bmp = image.ToBitmap();
                    bmp.Save(newName);
                }
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
