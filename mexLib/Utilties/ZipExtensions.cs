﻿using System.IO.Compression;

namespace mexLib.Utilties
{
    public static class ZipExtensions
    {
        /// <summary>
        /// Reads file from zip and adds it to workspace and returns the new filename if it changed
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="workspace"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string TryReadFile(this ZipArchive zip, MexWorkspace workspace, string filename)
        {
            ZipArchiveEntry? e = zip.GetEntry(filename);
            if (e == null)
                return "";

            string path = workspace.GetFilePath(filename);
            path = workspace.FileManager.GetUniqueFilePath(path);

            workspace.FileManager.Set(path, e.Extract());

            return Path.GetFileName(path);
        }
    }
}
