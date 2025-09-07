﻿using mexLib.Attributes;
using mexLib.Utilties;
using System.ComponentModel;

namespace mexLib.Types
{
    public class MexCostumeVisibilityFile : MexCostumeFile
    {
        [DisplayName("Original Costume Index")]
        public int VisibilityIndex
        {
            get => _visibilityIndex;
            set
            {
                _visibilityIndex = value;
                OnPropertyChanged();
            }
        }
        public int _visibilityIndex = 0;

        public MexCostumeVisibilityFile()
        {
            //PropertyChanged += (s, e) =>
            //{
            //    if (e.PropertyName == nameof(JointSymbol))
            //        SetCostumeVisibilityFromSymbols();
            //};
        }

        /// <summary>
        /// 
        /// </summary>
        //private void SetCostumeVisibilityFromSymbols()
        //{
        //    //switch (JointSymbol)
        //    //{
        //    //    case "PlyPeach5KYe_Share_joint": VisibilityIndex = 1; return;

        //    //    case "PlyPikachu5KNr_Share_joint": VisibilityIndex = 0; return;
        //    //    case "PlyPikachu5KRd_Share_joint": VisibilityIndex = 1; return;
        //    //    case "PlyPikachu5KBu_Share_joint": VisibilityIndex = 2; return;
        //    //    case "PlyPikachu5KGr_Share_joint": VisibilityIndex = 3; return;

        //    //    case "PlyPichu5KNr_Share_joint": VisibilityIndex = 0; return;
        //    //    case "PlyPichu5KRd_Share_joint": VisibilityIndex = 1; return;
        //    //    case "PlyPichu5KBu_Share_joint": VisibilityIndex = 2; return;
        //    //    case "PlyPichu5KGr_Share_joint": VisibilityIndex = 3; return;
        //    //}

        //    //if (JointSymbol.Contains("PlyPeach5K") ||
        //    //    JointSymbol.Contains("PlyPikachu5K") ||
        //    //    JointSymbol.Contains("PlyPichu5K"))
        //    //{
        //    //    VisibilityIndex = 0;
        //    //}
        //}
    }

    public class MexCostumeFile : MexReactiveObject
    {
        [DisplayName("File")]
        [MexFilePathValidator(MexFilePathType.Files)]
        [MexFilePathValidatorCallback("CheckFileName")]
        public string FileName { get => _fileName; set { _fileName = value; OnPropertyChanged(); } }

        private string _fileName = "";

        [DisplayName("Joint Symbol")]
        public string JointSymbol { get => _jointSymbol; set { _jointSymbol = value; OnPropertyChanged(); } }

        private string _jointSymbol = "";

        [DisplayName("Material Symbol")]
        public string MaterialSymbol { get => _materialSymbol; set { _materialSymbol = value; OnPropertyChanged(); } }

        private string _materialSymbol = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public MexFilePathError? CheckFileName(MexWorkspace workspace, string fullPath)
        {
            using Stream? stream = workspace.FileManager.GetStream(fullPath);

            if (stream == null)
                return new MexFilePathError("Unable to read file");

            if (!ArchiveTools.IsValidHSDFile(stream))
                return new MexFilePathError("Not a valid HSD file");

            bool passing = false;
            foreach (string s in ArchiveTools.GetSymbols(stream))
            {
                if (s.EndsWith("_joint"))
                    passing = true;
            }

            if (!passing)
                return new MexFilePathError("joint not found in dat");

            GetSymbolFromFile(workspace, fullPath);
            OnPropertyChanged(propertyName: nameof(JointSymbol));
            OnPropertyChanged(propertyName: nameof(MaterialSymbol));

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public bool GetSymbolFromFile(MexWorkspace workspace, string? fullPath = null)
        {
            fullPath ??= workspace.GetFilePath(FileName);

            using Stream? s = workspace.FileManager.GetStream(fullPath);

            if (s == null)
                return false;

            if (!ArchiveTools.IsValidHSDFile(s))
                return false;

            JointSymbol = "";
            MaterialSymbol = "";
            bool passing = false;
            foreach (string symbol in ArchiveTools.GetSymbols(s))
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
        /// <summary>
        /// 
        /// </summary>
        public void DeleteFiles(MexWorkspace workspace)
        {
            string path = workspace.GetFilePath(FileName);

            if (workspace.FileManager.Exists(path))
                workspace.FileManager.Remove(path);
        }
    }
}
