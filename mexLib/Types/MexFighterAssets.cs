using mexLib.AssetTypes;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public partial class MexFighter
    {
        [Browsable(false)]
        public FighterAssets Assets { get; set; } = new FighterAssets();

        public class FighterAssets
        {
            // TODO: result screen small
            // TODO: result screen big

            [Browsable(false)]
            [JsonInclude]
            public string? CSSIcon { get => CSSIconAsset.AssetFileName; internal set => CSSIconAsset.AssetFileName = value; }

            [Category("Character Select")]
            [DisplayName("Icon")]
            [JsonIgnore]
            public MexTextureAsset CSSIconAsset { get; set; } = new MexTextureAsset()
            {
                AssetPath = "css/icon",
                Width = 64,
                Height = 56,
                Format = HSDRaw.GX.GXTexFmt.CI8,
                TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,

            };
        }
    }
}
