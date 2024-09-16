using mexLib.AssetTypes;
using mexLib.Utilties;
using System.ComponentModel;
using System.IO.Compression;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public partial class MexStage
    {
        [Browsable(false)]
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

            public void ToPackage(MexWorkspace workspace, ZipWriter zip)
            {
                zip.TryWriteTextureAsset(workspace, IconAsset, "icon.png");
                zip.TryWriteTextureAsset(workspace, BannerAsset, "banner.png");
            }

            public void FromPackage(MexWorkspace workspace, ZipArchive zip)
            {
                IconAsset.SetFromPackage(workspace, zip, "icon.png");
                BannerAsset.SetFromPackage(workspace, zip, "banner.png");
            }

            public void Delete(MexWorkspace workspace)
            {
                IconAsset.Delete(workspace);
                BannerAsset.Delete(workspace);
            }
        }
    }
}
