using GCILib;
using System.ComponentModel;

namespace mexLib.Utilties
{
    public class ISOTool
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isoPath"></param>
        /// <param name="outputDirectory"></param>
        public static void ExtractToFileSystem(string isoPath, string outputDirectory, ProgressChangedEventHandler? progress = null)
        {
            string sys = outputDirectory + "/sys";
            if (!Directory.Exists(sys))
                Directory.CreateDirectory(sys);

            string files = outputDirectory + "/files";
            if (!Directory.Exists(files))
                Directory.CreateDirectory(files);

            using (GCISO iso = new(isoPath))
            {
                File.WriteAllBytes(Path.Combine(sys, "main.dol"), iso.DOLData);
                File.WriteAllBytes(Path.Combine(sys, "apploader.img"), iso.AppLoader);
                File.WriteAllBytes(Path.Combine(sys, "boot.bin"), iso.Boot);
                File.WriteAllBytes(Path.Combine(sys, "bi2.bin"), iso.Boot2);

                // extract iso files
                var allPaths = iso.GetAllFilePaths().ToArray();
                var fileNum = allPaths.Length;
                for (int i = 0; i < fileNum; i++)
                {
                    var file = allPaths[i];

                    var percent = (int)((i / (double)fileNum) * 100);
                    System.Diagnostics.Debug.WriteLine(percent);
                    progress?.Invoke(null, new ProgressChangedEventArgs(percent, file));

                    string output = files + "/" + file;
                    string? dir = Path.GetDirectoryName(output);

                    if (dir != null && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    // TODO: stream write instead of full copy
                    File.WriteAllBytes(output, iso.GetFileData(file));
                }
            }

            progress?.Invoke(null, new ProgressChangedEventArgs(100, "Done"));
        }
    }
}
