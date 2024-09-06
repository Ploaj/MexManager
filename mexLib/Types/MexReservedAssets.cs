using mexLib.AssetTypes;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public class MexReservedAssets
    {
        // order empty, smash, master hand, crazy hand, target, giga bowser, sandbag, single player
        public string?[] Icons
        {
            get
            {
                return IconsAssets.Select(e => e.AssetFileName).ToArray();
            }
            set
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
        public string? CSSBack { get => CSSBackAsset.AssetFileName; set => CSSBackAsset.AssetFileName = value; }
        public MexTextureAsset CSSBackAsset = new MexTextureAsset()
        {
            AssetPath = "css/back",
            Width = 64,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.I4,
        };

        public string? CSSNull { get => CSSNullAsset.AssetFileName; set => CSSNullAsset.AssetFileName = value; }
        public MexTextureAsset CSSNullAsset = new MexTextureAsset()
        {
            AssetPath = "css/null",
            Width = 64,
            Height = 56,
            Format = HSDRaw.GX.GXTexFmt.CI8,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,

        };

        // TODO: reserved sss null, locked, random, random tag
    }
}
