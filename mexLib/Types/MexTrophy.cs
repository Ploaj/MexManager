using mexLib.Attributes;
using mexLib.HsdObjects;
using mexLib.Installer;
using PropertyModels.ComponentModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
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

        [JsonIgnore]
        [Browsable(false)]
        public short SortAlphabeticalJ { get; set; } = 0;

        [JsonIgnore]
        [Browsable(false)]
        public short SortAlphabetical { get; set; } = 0;

        [JsonIgnore]
        [Browsable(false)]
        public short SortAlphabeticalJUS { get; set; } = 0;

        [JsonIgnore]
        [Browsable(false)]
        public short SortAlphabeticalUS { get; set; } = 0;

        public class TrophyFileEntry : MexReactiveObject
        {
            [Category("File")]
            [DisplayName("File")]
            [MexFilePathValidator(MexFilePathType.Files)]
            public string File { get => _file; set { _file = value; OnPropertyChanged(); } }
            private string _file = "";

            [Category("File")]
            [DisplayName("Symbol")]
            public string Symbol { get; set; } = "";
        }

        public class TrophyTextEntry : MexReactiveObject
        {
            [Category("Text")]
            [DisplayName("Name")]
            public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
            private string _name = "New Trophy";

            [Category("Text")]
            [DisplayName("Description Color")]
            public Color DescriptionColor
            {
                get => Color.FromArgb(_descriptionColor);
                set => _descriptionColor = value.ToArgb();
            }

            [Category("Text")]
            [DisplayName("Description Text")]
            [MultilineText()]
            public string Description { get; set; } = "";
            private int _descriptionColor = unchecked((int)0xFFFFFFFF);

            [Category("Text")]
            [DisplayName("Misc Top Color")]
            public Color Source1Color
            {
                get => Color.FromArgb(_source1Color);
                set => _source1Color = value.ToArgb();
            }

            [Category("Text")]
            [DisplayName("Misc Top Text")]
            public string Source1 { get; set; } = "";
            private int _source1Color = unchecked((int)0xFFFFFFFF);

            [Category("Text")]
            [DisplayName("Misc Bottom Color")]
            public Color Source2Color
            {
                get => Color.FromArgb(_source2Color);
                set => _source2Color = value.ToArgb();
            }

            [Category("Text")]
            [DisplayName("Misc Bottom Text")]
            public string Source2 { get; set; } = "";
            private int _source2Color = unchecked((int)0xFFFFFFFF);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="str"></param>
            /// <param name="reset"></param>
            /// <param name="color"></param>
            /// <returns></returns>
            private string DecodeString(string str, bool reset, out int color)
            {
                var decode = SdSanitizer.Decode(str, out color);
                var encode = SdSanitizer.Encode(decode, color, reset);

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
                Name = DecodeString(Name, false, out int color);
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

            public float OffsetY { get; set; } = 0;

            [JsonIgnore]
            public string FileName
            {
                get => MexDefaultData.TrophyIconsFiles[FileIndex];
            }
        }
    }
}
