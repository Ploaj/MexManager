using mexLib.AssetTypes;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public class MexReservedAssets
    {
        // order empty, smash, master hand, crazy hand, target, giga bowser, sandbag, single player
        [Browsable(false)]
        [JsonInclude]
        public string?[] Icons
        {
            get
            {
                return IconsAssets.Select(e => e.AssetFileName).ToArray();
            }
            internal set
            {
                for (int i = 0; i < Math.Min(value.Length, IconsAssets.Length); i++)
                {
                    IconsAssets[i].AssetFileName = value[i];
                }
            }
        }

        [Browsable(false)]
        [JsonIgnore]
        public MexTextureAsset[] IconsAssets { get; set; } =
        {
            new MexTextureAsset()
            {
                AssetPath = "icons/rs",
                Width = 24,
                Height = 24,
                Format = HSDRaw.GX.GXTexFmt.CI4,
                TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
            },
            new MexTextureAsset()
            {
                AssetPath = "icons/rs",
                Width = 24,
                Height = 24,
                Format = HSDRaw.GX.GXTexFmt.CI4,
                TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
            },
            new MexTextureAsset()
            {
                AssetPath = "icons/rs",
                Width = 24,
                Height = 24,
                Format = HSDRaw.GX.GXTexFmt.CI4,
                TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
            },
            new MexTextureAsset()
            {
                AssetPath = "icons/rs",
                Width = 24,
                Height = 24,
                Format = HSDRaw.GX.GXTexFmt.CI4,
                TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
            },
            new MexTextureAsset()
            {
                AssetPath = "icons/rs",
                Width = 24,
                Height = 24,
                Format = HSDRaw.GX.GXTexFmt.CI4,
                TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
            },
            new MexTextureAsset()
            {
                AssetPath = "icons/rs",
                Width = 24,
                Height = 24,
                Format = HSDRaw.GX.GXTexFmt.CI4,
                TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
            },
            new MexTextureAsset()
            {
                AssetPath = "icons/rs",
                Width = 24,
                Height = 24,
                Format = HSDRaw.GX.GXTexFmt.CI4,
                TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
            },
            new MexTextureAsset()
            {
                AssetPath = "icons/rs",
                Width = 24,
                Height = 24,
                Format = HSDRaw.GX.GXTexFmt.CI4,
                TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
            },
        };

        // reserved css icon back, null
        [Browsable(false)]
        [JsonInclude]
        public string? CSSBack { get => CSSBackAsset.AssetFileName; internal set => CSSBackAsset.AssetFileName = value; }
        [DisplayName("Icon Background")]
        [JsonIgnore]
        public MexTextureAsset CSSBackAsset { get; set; } = new()
        {
            AssetPath = "css/back",
            Width = 64,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.I4,
        };

        [Browsable(false)]
        [JsonInclude]
        public string? CSSNull { get => CSSNullAsset.AssetFileName; internal set => CSSNullAsset.AssetFileName = value; }
        [DisplayName("Blank Fighter")]
        [JsonIgnore]
        public MexTextureAsset CSSNullAsset { get; set; } = new()
        {
            AssetPath = "css/null",
            Width = 64,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.CI8,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
        };

        // reserved sss null, locked, random, random tag

        [Browsable(false)]
        [JsonInclude]
        public string? SSSNull { get => SSSNullAsset.AssetFileName; internal set => SSSNullAsset.AssetFileName = value; }
        [DisplayName("Blank Stage")]
        [JsonIgnore]
        public MexTextureAsset SSSNullAsset { get; set; } = new()
        {
            AssetPath = "sss/null",
            Width = 64,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.CI8,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB565,
        };

        [Browsable(false)]
        [JsonInclude]
        public string? SSSLockedNull { get => SSSLockedNullAsset.AssetFileName; internal set => SSSLockedNullAsset.AssetFileName = value; }
        [DisplayName("Locked Stage")]
        [JsonIgnore]
        public MexTextureAsset SSSLockedNullAsset { get; set; } = new()
        {
            AssetPath = "sss/locked",
            Width = 64,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.CI8,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB565,
        };

        [Browsable(false)]
        [JsonInclude]
        public string? SSSRandomBanner { get => SSSRandomBannerAsset.AssetFileName; internal set => SSSRandomBannerAsset.AssetFileName = value; }
        [DisplayName("Random Banner")]
        [JsonIgnore]
        public MexTextureAsset SSSRandomBannerAsset { get; set; } = new()
        {
            AssetPath = "sss/random",
            Width = 224,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.I4,
        };

        // result

        [Browsable(false)]
        [JsonInclude]
        public string? RstRedTeam { get => RstRedTeamAsset.AssetFileName; internal set => RstRedTeamAsset.AssetFileName = value; }
        [DisplayName("Red Team")]
        [JsonIgnore]
        public MexTextureAsset RstRedTeamAsset { get; set; } = new()
        {
            AssetPath = "rst/team_red",
            Width = 256,
            Height = 28,
            Format = HSDRaw.GX.GXTexFmt.I4,
            TlutFormat = HSDRaw.GX.GXTlutFmt.IA8,
        };

        [Browsable(false)]
        [JsonInclude]
        public string? RstGreenTeam { get => RstGreenTeamAsset.AssetFileName; internal set => RstGreenTeamAsset.AssetFileName = value; }
        [DisplayName("Green Team")]
        [JsonIgnore]
        public MexTextureAsset RstGreenTeamAsset { get; set; } = new()
        {
            AssetPath = "rst/team_green",
            Width = 256,
            Height = 28,
            Format = HSDRaw.GX.GXTexFmt.I4,
            TlutFormat = HSDRaw.GX.GXTlutFmt.IA8,
        };

        [Browsable(false)]
        [JsonInclude]
        public string? RstBlueTeam { get => RstBlueTeamAsset.AssetFileName; internal set => RstBlueTeamAsset.AssetFileName = value; }
        [DisplayName("Blue Team")]
        [JsonIgnore]
        public MexTextureAsset RstBlueTeamAsset { get; set; } = new()
        {
            AssetPath = "rst/team_blue",
            Width = 256,
            Height = 28,
            Format = HSDRaw.GX.GXTexFmt.I4,
            TlutFormat = HSDRaw.GX.GXTlutFmt.IA8,
        };

        [Browsable(false)]
        [JsonInclude]
        public string? RstNoContest { get => RstNoContestAsset.AssetFileName; internal set => RstNoContestAsset.AssetFileName = value; }
        [DisplayName("No Contest")]
        [JsonIgnore]
        public MexTextureAsset RstNoContestAsset { get; set; } = new()
        {
            AssetPath = "rst/no_contest",
            Width = 256,
            Height = 28,
            Format = HSDRaw.GX.GXTexFmt.I4,
            TlutFormat = HSDRaw.GX.GXTlutFmt.IA8,
        };
    }
}
