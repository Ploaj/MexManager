using MexManager.Tools;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MexManager
{
    public class Updater
    {
        static Release[]? releases;

        public static Release? LatestRelease;

        public static string? DownloadURL;
        public static string? Version;

        public static bool UpdateReady;

        /// <summary>
        /// 
        /// </summary>
        public static bool UpdateCodes()
        {
            // https://github.com/akaneia/m-ex/raw/master/asm/codes.gct
            // https://github.com/akaneia/m-ex/raw/master/asm/codes.ini

            UpdateCodesFromURL(Global.MexCodePath, @"https://github.com/akaneia/m-ex/raw/master/asm/codes.gct");
            UpdateCodesFromURL(Global.MexAddCodePath, @"https://github.com/akaneia/m-ex/raw/master/asm/codes.ini");

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mexPath"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private async static void UpdateCodesFromURL(string filePath, string url)
        {
            //string? hash = null;
            //if (File.Exists(filePath))
            //{
            //    hash = HashGen.ComputeSHA256Hash(File.ReadAllBytes(filePath));
            //}

            using HttpClient client = new();
            {
                Uri uri = new(url);
                await client.DownloadFileTaskAsync(uri, filePath);
            }

            //var newhash = HashGen.ComputeSHA256Hash(File.ReadAllBytes(filePath));

            //if (!string.IsNullOrEmpty(hash) &&
            //    !hash.Equals(newhash))
            //    return true;

            //return false;
        }

        public delegate void OnUpdateReader();

        /// <summary>
        /// 
        /// </summary>
        public static async Task CheckLatest(OnUpdateReader onready)
        {
            string currentVersion = "";
            var versionText = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt");
            if (File.Exists(versionText))
                currentVersion = File.ReadAllText(versionText);

            try
            {
                var client = new GitHubClient(new ProductHeaderValue("mex-updater"));
                await GetReleases(client);

                if (releases == null)
                    return;

                foreach (Release latest in releases)
                {
                    if (latest.Prerelease &&
                        latest.Assets.Count > 0 &&
                        !latest.Assets[0].UpdatedAt.ToString().Equals(currentVersion))
                    {
                        Logger.WriteLine($"Check Update");
                        Logger.WriteLine($"Name: {latest.Name}");
                        Logger.WriteLine($"URL: {latest.Assets[0].BrowserDownloadUrl}");
                        Logger.WriteLine($"Upload Date: {latest.Assets[0].UpdatedAt}");

                        LatestRelease = latest;
                        DownloadURL = latest.Assets[0].BrowserDownloadUrl;
                        Version = latest.Assets[0].UpdatedAt.ToString();
                        UpdateReady = true;
                        onready?.Invoke();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine($"Failed to get latest update\n{e.ToString()}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        static async Task GetReleases(GitHubClient client)
        {
            List<Release> Releases = [];
            foreach (Release r in await client.Repository.Release.GetAll("Ploaj", "MexManager"))
                Releases.Add(r);
            releases = Releases.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        public static void Update()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            File.Delete(Path.Combine(baseDir, "MexManagerUpdater_.exe"));
            File.Copy(Path.Combine(baseDir, "MexManagerUpdater.exe"), Path.Combine(baseDir, "MexManagerUpdater_.exe"));

            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MexManagerUpdater_.exe");
            p.StartInfo.Arguments = $"{Updater.DownloadURL} \"{Updater.Version}\" -r";
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.Verb = "runas";
            try
            {
                p.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Failed to start updater: " + ex.Message);
                return;
            }

            // Exit the current Avalonia application
            Environment.Exit(0);
        }
    }
}
