using mexLib.Utilties;

namespace mexLib
{
    public class MexCostumeFile
    {
        public string FileName { get; set; } = "";

        public string JointSymbol { get; set; } = "";

        public string MaterialSymbol { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public bool GetSymbolFromFile(MexWorkspace workspace)
        {
            var fullPath = workspace.GetFilePath(FileName);

            if (!workspace.FileManager.Exists(fullPath))
                return false;

            using Stream? s = workspace.FileManager.GetStream(fullPath);

            if (s != null && !ArchiveTools.IsValidHSDFile(s))
                return false;

            JointSymbol = "";
            MaterialSymbol = "";
            bool passing = false;
            foreach (var symbol in ArchiveTools.GetSymbols(s))
            {
                if (symbol.EndsWith("matanim_joint"))
                    MaterialSymbol = symbol;
                else
                if (symbol.EndsWith("_joint"))
                {
                    passing = true;
                    JointSymbol = symbol;
                }
            }

            return passing;
        }
    }
}
