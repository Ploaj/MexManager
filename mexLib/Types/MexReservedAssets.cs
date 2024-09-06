using mexLib.AssetTypes;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public class MexReservedAssets
    {
        // order empty, smash, master hand, crazy hand, target, giga bowser, sandbag, single player
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
        [JsonInclude]
        public string? CSSBack { get => CSSBackAsset.AssetFileName; internal set => CSSBackAsset.AssetFileName = value; }
        public MexTextureAsset CSSBackAsset = new ()
        {
            AssetPath = "css/back",
            Width = 64,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.I4,
        };

        [JsonInclude]
        public string? CSSNull { get => CSSNullAsset.AssetFileName; internal set => CSSNullAsset.AssetFileName = value; }
        public MexTextureAsset CSSNullAsset = new ()
        {
            AssetPath = "css/null",
            Width = 64,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.CI8,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
        };

        // reserved sss null, locked, random, random tag

        [JsonInclude]
        public string? SSSNull { get => SSSNullAsset.AssetFileName; internal set => SSSNullAsset.AssetFileName = value; }
        public MexTextureAsset SSSNullAsset = new()
        {
            AssetPath = "sss/null",
            Width = 64,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.CI8,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB565,
        };
        [JsonInclude]
        public string? SSSLockedNull { get => SSSLockedNullAsset.AssetFileName; internal set => SSSLockedNullAsset.AssetFileName = value; }
        public MexTextureAsset SSSLockedNullAsset = new()
        {
            AssetPath = "sss/locked",
            Width = 64,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.CI8,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB565,
        };
        [JsonInclude]
        public string? SSSRandomBanner { get => SSSRandomBannerAsset.AssetFileName; internal set => SSSRandomBannerAsset.AssetFileName = value; }
        public MexTextureAsset SSSRandomBannerAsset = new()
        {
            AssetPath = "sss/null",
            Width = 224,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.I4,
        };

    }
}
