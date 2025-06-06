﻿using mexLib.Attributes;
using mexLib.HsdObjects;
using mexLib.Installer;
using mexLib.Utilties;
using PropertyModels.ComponentModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public class MexTrophy : MexReactiveObject
    {
        public class TrophyData
        {
            public TrophyFileEntry File { get; set; } = new TrophyFileEntry();

            public TrophyTextEntry Text { get; set; } = new TrophyTextEntry();

            public TrophyParams Param3D { get; set; } = new TrophyParams();

            public Trophy2DParams Param2D { get; set; } = new Trophy2DParams();
        }

        [Browsable(false)]
        public string Name { get => Data.Text.Name; }

        [Browsable(false)]
        public TrophyData Data { get; set; } = new TrophyData();

        [Browsable(false)]
        public bool HasUSData { get => _hasUSData; set { _hasUSData = value; OnPropertyChanged(); } }
        private bool _hasUSData;

        [Browsable(false)]
        public TrophyData USData { get; set; } = new TrophyData();

        public bool JapanOnly { get; set; } = false;

        [JsonIgnore]
        public short SortSeries { get => _sortSeries; set { _sortSeries = value; OnPropertyChanged(); } }
        private short _sortSeries = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="zip"></param>
        public MexInstallerError? ToPackage(MexWorkspace workspace, Stream stream)
        {
            // create zip
            using ZipWriter zip = new(stream);

            // fighter to package
            zip.WriteAsJson("trophy.json", this);

            // write files
            zip.TryWriteFile(workspace, Data.File.File, Data.File.File);
            if (HasUSData)
                zip.TryWriteFile(workspace, USData.File.File, USData.File.File);

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="zip"></param>
        public static MexInstallerError? FromPackage(MexWorkspace workspace, Stream stream, out MexTrophy? trophy)
        {
            trophy = null;

            // load zip
            using ZipArchive zip = new(stream);

            // import trophy from package
            ZipArchiveEntry? entry = zip.GetEntry("trophy.json");
            if (entry == null)
                return new MexInstallerError("\"trophy.json\" was not found in zip");

            // parse group entry
            trophy = MexJsonSerializer.Deserialize<MexTrophy>(entry.Extract());
            if (trophy == null)
                return new MexInstallerError("Error parsing \"trophy.json\"");

            // add files
            trophy.Data.File.File = zip.TryReadFile(workspace, trophy.Data.File.File);
            if (trophy.HasUSData)
                trophy.USData.File.File = zip.TryReadFile(workspace, trophy.USData.File.File);

            return null;
        }

        public class TrophyFileEntry : MexReactiveObject
        {
            [Category("File")]
            [DisplayName("File")]
            [MexFilePathValidator(MexFilePathType.Files)]
            [MexFilePathValidatorCallback("ExtractSymbol")]
            public string File { get => _file; set { _file = value; OnPropertyChanged(); } }
            private string _file = "";

            [Category("File")]
            [DisplayName("Symbol")]
            public string Symbol { get => _symbol; set { _symbol = value; OnPropertyChanged(); } }
            private string _symbol = "";

            public MexFilePathError? ExtractSymbol(MexWorkspace workspace, string fullPath)
            {
                using Stream? stream = workspace.FileManager.GetStream(fullPath);

                if (stream == null)
                    return new MexFilePathError("Unable to read file");

                if (!ArchiveTools.IsValidHSDFile(stream))
                    return new MexFilePathError("Not a valid HSD file");

                foreach (string s in ArchiveTools.GetSymbols(stream))
                {
                    if (s.EndsWith("_joint"))
                    {
                        Symbol = s;
                        return null;
                    }
                }

                return new MexFilePathError("joint symbol not found in dat");
            }
        }

        public class TrophyTextEntry : MexReactiveObject
        {
            [Category("Text")]
            [DisplayName("Name")]
            public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
            private string _name = "New Trophy";

            [Category("Text")]
            [DisplayName("Description Color")]
            [JsonIgnore]
            public Color DescriptionColor
            {
                get => Color.FromArgb((int)_descriptionColor);
                set => _descriptionColor = (uint)value.ToArgb() | 0xFF000000;
            }
            [JsonInclude]
            public uint _descriptionColor = 0xFFFFFFFF;

            [Category("Text")]
            [DisplayName("Description Text")]
            [MultilineText()]
            public string Description { get; set; } = "";

            [Category("Text")]
            [DisplayName("Misc Top Color")]
            [JsonIgnore]
            public Color Source1Color
            {
                get => Color.FromArgb((int)_source1Color);
                set => _source1Color = (uint)value.ToArgb() | 0xFF000000;
            }
            [JsonInclude]
            public uint _source1Color = 0xFFFFFFFF;

            [Category("Text")]
            [DisplayName("Misc Top Text")]
            public string Source1 { get; set; } = "";

            [Category("Text")]
            [DisplayName("Misc Bottom Color")]
            [JsonIgnore]
            public Color Source2Color
            {
                get => Color.FromArgb((int)_source2Color);
                set => _source2Color = (uint)value.ToArgb() | 0xFF000000;
            }
            [JsonInclude]
            public uint _source2Color = 0xFFFFFFFF;

            [Category("Text")]
            [DisplayName("Misc Bottom Text")]
            public string Source2 { get; set; } = "";

            /// <summary>
            /// 
            /// </summary>
            /// <param name="str"></param>
            /// <param name="reset"></param>
            /// <param name="color"></param>
            /// <returns></returns>
            private static string DecodeString(string str, bool reset, out uint color)
            {
                string decode = SdSanitizer.Decode(str, out color);
                string encode = SdSanitizer.Encode(decode, color, reset);

                if (!str.Equals(encode))
                {
                    Debug.WriteLine($"Error encodeing: {str}");
                    Debug.WriteLine($"\t{decode}");
                    Debug.WriteLine($"\t{encode}");
                }

                return decode;
            }
            /// <summary>
            /// 
            /// </summary>
            public void DecodeAllStrings()
            {
                Name = DecodeString(Name, false, out uint _);
                Description = DecodeString(Description, true, out _descriptionColor);
                Source1 = DecodeString(Source1, true, out _source1Color);
                Source2 = DecodeString(Source2, true, out _source2Color);
            }
        }

        public enum TrophyType
        {
            Normal,
            SmashRed,
            SmashBlue,
        }

        public enum TrophyUnlockKind
        {
            SinglePlayer,
            Special,
            LotteryAndSinglePlayer,
            LotteryOnly,
            LotteryAfterSinglePlayerClear,
            LotteryAndSinglePlayerAfterSinglePlayerClear,
            LotteryAllCharactersUnlocked,
            Lottery250TrophiesCollected,
            EventOnly,
        }

        public class TrophyParams
        {
            public TrophyType TrophyType { get; set; } = TrophyType.Normal;

            public float OffsetX { get; set; } = 0;

            public float OffsetY { get; set; } = 0;

            public float OffsetZ { get; set; } = 0;

            public float TrophyScale { get; set; } = 1;

            public float StandScale { get; set; } = 1;

            public float Rotation { get; set; } = 0;

            public TrophyUnlockKind UnlockCondition { get; set; } = TrophyUnlockKind.Special;

            public byte UnlockParam { get; set; } = 1; // has to do with unlock?
        }

        public class Trophy2DParams
        {
            public byte FileIndex { get; set; } = 0;

            public byte ImageIndex { get; set; } = 0;

            public float OffsetX { get; set; } = 0;

            public float OffsetY { get; set; } = 1.0f;

            [JsonIgnore]
            public string FileName
            {
                get => MexDefaultData.TrophyIconsFiles[FileIndex];
            }
        }
    }
}
