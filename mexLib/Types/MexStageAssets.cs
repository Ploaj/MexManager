using mexLib.AssetTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mexLib.Types
{
    public partial class MexStage
    {
        public StageAssets Assets { get; set; } = new StageAssets();

        public class StageAssets
        {
            [Browsable(false)]
            [JsonInclude]
            public string? Icon { get => IconAsset.AssetFileName; internal set => IconAsset.AssetFileName = value; }

            [Category("Stage Select")]
            [DisplayName("Icon")]
            [JsonIgnore]
            public MexTextureAsset IconAsset { get; set; } = new MexTextureAsset()
            {
                AssetPath = "sss/icon",
                Width = 64,
                Height = 56,
                Format = HSDRaw.GX.GXTexFmt.CI8,
                TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
            };

            [Browsable(false)]
            [JsonInclude]
            public string? Banner { get => BannerAsset.AssetFileName; internal set => BannerAsset.AssetFileName = value; }

            [Category("Stage Select")]
            [DisplayName("Banner")]
            [JsonIgnore]
            public MexTextureAsset BannerAsset { get; set; } = new MexTextureAsset()
            {
                AssetPath = "sss/icon",
                Width = 224,
                Height = 56,
                Format = HSDRaw.GX.GXTexFmt.I4,
            };
        }
    }
}
