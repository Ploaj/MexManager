﻿using mexLib.Attributes;
using mexLib.Installer;
using mexLib.Utilties;
using System.ComponentModel;
using System.IO.Compression;

namespace mexLib.Types
{
    public partial class MexFighter
    {
        [Browsable(false)]
        public FighterFiles Files { get; set; } = new FighterFiles();

        public class FighterFiles
        {
            [Category("0 - Fighter Data"), DisplayName("FighterData FileName"), Description("File containing fighter's data")]
            [MexFilePathValidator(MexFilePathType.Files, nullable: false)]
            public string FighterDataPath { get; set; } = "";

            [Category("0 - Fighter Data"), DisplayName("FighterData Symbol"), Description("Symbol used inside of Fighter Data file")]
            public string FighterDataSymbol { get; set; } = "";

            [Category("0 - Fighter Data"), DisplayName("Animation FileName"), Description("File containing the fighter animations")]
            [MexFilePathValidator(MexFilePathType.Files, nullable: false)]
            public string AnimFile { get; set; } = "";

            [Category("0 - Fighter Data"), DisplayName("Animation Count"), Description("Number of animations the fighter has")]
            public uint AnimCount { get; set; } = 0;

            [Category("1 - Demo Screens"), DisplayName("VI Wait FileName"), Description("")]
            [MexFilePathValidator(MexFilePathType.Files)]
            public string DemoFile { get; set; } = "";

            [Category("1 - Demo Screens"), DisplayName("Vi Wait"), Description("")]
            public string DemoWait { get; set; } = "";

            [Category("1 - Demo Screens"), DisplayName("Result"), Description("")]
            public string DemoResult { get; set; } = "";

            [Category("1 - Demo Screens"), DisplayName("Intro"), Description("")]
            public string DemoIntro { get; set; } = "";

            [Category("1 - Demo Screens"), DisplayName("Ending"), Description("")]
            public string DemoEnding { get; set; } = "";

            [Category("2 - Result Screen"), DisplayName("Result Animation FileName"), Description("File Containing the Result Fighter Animations")]
            [MexFilePathValidator(MexFilePathType.Files)]
            public string RstAnimFile { get; set; } = "";

            [Category("2 - Result Screen"), DisplayName("Result Animation Count"), Description("Number of Result Animations")]
            public uint RstAnimCount { get; set; } = 0;

            [Category("3 - Effects"), DisplayName("Effect FileName"), Description("Effect file to load for fighter")]
            [MexFilePathValidator(MexFilePathType.Files)]
            public string EffectFile { get; set; } = "";

            [Category("3 - Effects"), DisplayName("Effect Symbol"), Description("Symbol in effect file to load")]
            public string EffectSymbol { get; set; } = "";

            [Category("4 - Kirby Data"), DisplayName("Kirby Cap FileName"), Description("Kirby cap file associated with this fighter")]
            [MexFilePathValidator(MexFilePathType.Files)]
            public string KirbyCapFileName { get; set; } = "";

            [Category("4 - Kirby Data"), DisplayName("Kirby Cap Symbol"), Description("Symbol name in cap file")]
            public string KirbyCapSymbol { get; set; } = "";

            [Category("4 - Kirby Data"), DisplayName("Kirby Effect FileName"), Description("Effect file to load for Kirby")]
            [MexFilePathValidator(MexFilePathType.Files)]
            public string KirbyEffectFile { get; set; } = "";

            [Category("4 - Kirby Data"), DisplayName("Kirby Effect Symbol"), Description("Symbol in Kirby effect file to load")]
            public string KirbyEffectSymbol { get; set; } = "";

            /// <summary>
            /// 
            /// </summary>
            /// <param name="workspace"></param>
            public void Delete(MexWorkspace workspace)
            {
                // delete fighter files
                workspace.FileManager.Remove(workspace.GetFilePath(FighterDataPath));
                workspace.FileManager.Remove(workspace.GetFilePath(AnimFile));
                workspace.FileManager.Remove(workspace.GetFilePath(DemoFile));
                workspace.FileManager.Remove(workspace.GetFilePath(DemoWait));
                workspace.FileManager.Remove(workspace.GetFilePath(RstAnimFile));
                workspace.FileManager.Remove(workspace.GetFilePath(EffectFile));
                workspace.FileManager.Remove(workspace.GetFilePath(KirbyEffectFile));
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="workspace"></param>
            /// <param name="zip"></param>
            public MexInstallerError? ToPackage(MexWorkspace workspace, ZipWriter zip)
            {
                // fighter files to package
                zip.TryWriteFile(workspace, FighterDataPath, FighterDataPath);
                zip.TryWriteFile(workspace, AnimFile, AnimFile);
                zip.TryWriteFile(workspace, DemoFile, DemoFile);
                zip.TryWriteFile(workspace, DemoWait, DemoWait);
                zip.TryWriteFile(workspace, RstAnimFile, RstAnimFile);
                zip.TryWriteFile(workspace, EffectFile, EffectFile);
                zip.TryWriteFile(workspace, KirbyEffectFile, KirbyEffectFile);
                return null;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="workspace"></param>
            /// <param name="zip"></param>
            public MexInstallerError? FromPackage(MexWorkspace workspace, ZipArchive zip)
            {
                // fighter files from package
                FighterDataPath = zip.TryReadFile(workspace, FighterDataPath);
                AnimFile = zip.TryReadFile(workspace, AnimFile);
                DemoFile = zip.TryReadFile(workspace, DemoFile);
                DemoWait = zip.TryReadFile(workspace, DemoWait);
                RstAnimFile = zip.TryReadFile(workspace, RstAnimFile);
                EffectFile = zip.TryReadFile(workspace, EffectFile);
                KirbyEffectFile = zip.TryReadFile(workspace, KirbyEffectFile);
                return null;
            }
        }
    }
}
