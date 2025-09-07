using MexManager.Tools;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace MexManager
{
    public class Updater
    {
        static Release[]? releases;

        public static Release? LatestRelease { get; internal set; }

        public static string? DownloadURL { get; internal set; }

        public static string? Version { get; internal set; }

        public static bool UpdateReady => UpdateManager | UpdateCodes;

        public static bool UpdateManager { get; internal set; } = false;

        public static bool UpdateCodes { get; internal set; } = false;

        /// <summary>
        /// 
        /// </summary>
        //public static bool UpdateCodes()
        //{
        //    // https://github.com/akaneia/m-ex/raw/master/asm/codes.gct
        //    // https://github.com/akaneia/m-ex/raw/master/asm/codes.ini

        //    UpdateCodesFromURL(Global.MexCodePath, @"https://github.com/akaneia/m-ex/raw/master/asm/codes.gct");
        //    UpdateCodesFromURL(Global.MexAddCodePath, @"https://github.com/akaneia/m-ex/raw/master/asm/codes.ini");

        //    return false;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mexPath"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        //private async static void UpdateCodesFromURL(string filePath, string url)
        //{
        //    //string? hash = null;
        //    //if (File.Exists(filePath))
        //    //{
        //    //    hash = HashGen.ComputeSHA256Hash(File.ReadAllBytes(filePath));
        //    //}

        //    using HttpClient client = new();
        //    {
        //        Uri uri = new(url);
        //        await client.DownloadFileTaskAsync(uri, filePath);
        //    }

        //    //var newhash = HashGen.ComputeSHA256Hash(File.ReadAllBytes(filePath));

        //    //if (!string.IsNullOrEmpty(hash) &&
        //    //    !hash.Equals(newhash))
        //    //    return true;

        //    //return false;
        //}

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

            if (await CheckUpdateLocalFile(
                "akaneia", 
                "m-ex", 
                "master", 
                "asm/codes.ini", 
                Global.MexAddCodePath,
                false))
            {
                UpdateCodes = true;
                onready?.Invoke();
            }
            if (await CheckUpdateLocalFile(
                "akaneia",
                "m-ex",
                "master",
                "asm/codes.gct",
                Global.MexCodePath,
                false))
            {
                UpdateCodes = true;
                onready?.Invoke();
            }

            try
            {
                var client = new GitHubClient(new Octokit.ProductHeaderValue("mex-updater"));
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
                        UpdateManager = true;
                        onready?.Invoke();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine($"Failed to get latest update\n{e}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        static async Task GetReleases(GitHubClient client)
        {
            releases = (await client.Repository.Release.GetAll("Ploaj", "MexManager")).ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CheckUpdateLocalFile(
            string owner, 
            string repo, 
            string branch, 
            string filePath, 
            string localFile,
            bool download)
        {
            using HttpClient http = new ();

            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("mex-updater", "1.0"));

            // --- Step 1: Get last commit for that file
            string commitsUrl = $"https://api.github.com/repos/{owner}/{repo}/commits?path={filePath}&sha={branch}";
            string commitJson = await http.GetStringAsync(commitsUrl);

            var commits = JsonDocument.Parse(commitJson).RootElement;
            if (commits.GetArrayLength() == 0)
            {
                Logger.WriteLine($"{Path.GetFileNameWithoutExtension(localFile)} not found in repo");
                return false;
            }

            var latestCommit = commits[0];
            var commitDate = latestCommit
                .GetProperty("commit")
                .GetProperty("committer")
                .GetProperty("date")
                .GetDateTime();

            // --- Step 2: Compare with local file
            if (File.Exists(localFile))
            {
                var localTime = File.GetLastWriteTimeUtc(localFile);
                Logger.WriteLine($"{Path.GetFileName(localFile)} {commitDate} {localTime}");
                if (localTime >= commitDate)
                {
                    Logger.WriteLine($"\"{Path.GetFileName(localFile)}\" is up to date");
                    return false;
                }
            }

            // --- Step 3: Download raw file
            if (download)
            {
                string rawUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{filePath}";
                var fileBytes = await http.GetByteArrayAsync(rawUrl);
                Directory.CreateDirectory(Path.GetDirectoryName(localFile) ?? ".");
                await File.WriteAllBytesAsync(localFile, fileBytes);

                File.SetLastWriteTimeUtc(localFile, commitDate); // keep timestamps in sync
                Logger.WriteLine($"Updated {Path.GetFileNameWithoutExtension(localFile)} ");
            }

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async static Task<bool> UpdateCodesOnly()
        {
            if (await CheckUpdateLocalFile(
                "akaneia",
                "m-ex",
                "master",
                "asm/codes.ini",
                Global.MexAddCodePath,
                true) &&
                await CheckUpdateLocalFile(
                "akaneia",
                "m-ex",
                "master",
                "asm/codes.gct",
                Global.MexCodePath,
                true))
            {
                UpdateCodes = false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        public async static Task<bool> Update()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var updatePath = Path.Combine(baseDir, "MexManagerUpdater.exe");
            if (!File.Exists(updatePath))
            {
                await CheckUpdateLocalFile(
                    "Ploaj", 
                    "MexManager", 
                    "master", 
                    "MexManager.Desktop/MexManagerUpdater.exe", 
                    updatePath, 
                    true);
            }

            if (!File.Exists(updatePath))
                return false;

            //var assembly = Assembly.GetExecutingAssembly();
            //using var stream = assembly.GetManifestResourceStream("MexManager.MexManagerUpdater.exe");
            //{
            //    if (stream == null)
            //        throw new Exception($"Resource MexManagerUpdater not found.");

            //    using var fileStream = new FileStream(updatePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            //    {
            //        stream.CopyTo(fileStream);
            //    }
            //}

            Process p = new ();
            p.StartInfo.FileName = updatePath;
            p.StartInfo.Arguments = $"\"{Updater.DownloadURL}\" \"{Updater.Version}\" -r";
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.Verb = "runas";
            try
            {
                p.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Failed to start updater: " + ex.Message);
                return false;
            }

            // Exit the current Avalonia application
            Environment.Exit(0);
            return true;
        }
    }
}
