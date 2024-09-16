using mexLib.AssetTypes;
using mexLib.Utilties;
using System.ComponentModel;
using System.IO.Compression;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public partial class MexFighter
    {
        [Browsable(false)]
        public FighterAssets Assets { get; set; } = new FighterAssets();

        public class FighterAssets
        {
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

            [Browsable(false)]
            [JsonInclude]
            public string? ResultBannerBig { get => ResultBannerBigAsset.AssetFileName; internal set => ResultBannerBigAsset.AssetFileName = value; }

            [Category("Result Screen")]
            [DisplayName("Large Result Banner")]
            [JsonIgnore]
            public MexTextureAsset ResultBannerBigAsset { get; set; } = new MexTextureAsset()
            {
                AssetPath = "rst/big_banner",
                Width = 256,
                Height = 28,
                Format = HSDRaw.GX.GXTexFmt.I4,
                TlutFormat = HSDRaw.GX.GXTlutFmt.IA8,
            };

            [Browsable(false)]
            [JsonInclude]
            public string? ResultSmallBig { get => ResultBannerSmallAsset.AssetFileName; internal set => ResultBannerSmallAsset.AssetFileName = value; }

            [Category("Result Screen")]
            [DisplayName("Small Result Banner")]
            [JsonIgnore]
            public MexTextureAsset ResultBannerSmallAsset { get; set; } = new MexTextureAsset()
            {
                AssetPath = "rst/small_banner",
                Width = 120,
                Height = 24,
                Format = HSDRaw.GX.GXTexFmt.I4,
                TlutFormat = HSDRaw.GX.GXTlutFmt.IA8,
            };

            public void ToPackage(MexWorkspace workspace, ZipWriter zip)
            {
                zip.TryWriteTextureAsset(workspace, CSSIconAsset, "icon.png");
                zip.TryWriteTextureAsset(workspace, ResultBannerBigAsset, "big_banner.png");
                zip.TryWriteTextureAsset(workspace, ResultBannerSmallAsset, "small_banner.png");
            }

            public void FromPackage(MexWorkspace workspace, ZipArchive zip)
            {
                CSSIconAsset.SetFromPackage(workspace, zip, "icon.png");
                ResultBannerBigAsset.SetFromPackage(workspace, zip, "big_banner.png");
                ResultBannerSmallAsset.SetFromPackage(workspace, zip, "small_banner.png");
            }

            public void Delete(MexWorkspace workspace)
            {
                CSSIconAsset.Delete(workspace);
                ResultBannerBigAsset.Delete(workspace);
                ResultBannerSmallAsset.Delete(workspace);
            }
        }
    }
}
