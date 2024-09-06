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

        // TODO: reserved css icon back, null

        // TODO: reserved sss null, locked, random, random tag
    }
}
