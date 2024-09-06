using mexLib.AssetTypes;
using mexLib.Attributes;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public class MexSeries
    {
        [Category("General"), DisplayName("Name"), Description("Name of the series")]
        public string Name { get => _name; set { _name = value; } }
        private string _name = "";

        [Browsable(false)]
        public MexPlaylist Playlist { get; set; } = new MexPlaylist();

        //[Category("General"), DisplayName("Series Icon")]
        //[MexTextureAsset]
        //[MexTextureFormat(HSDRaw.GX.GXTexFmt.I4)]
        //[MexTextureSize(80, 64)]
        //[MexFilePathValidator(MexFilePathType.Assets)]
        //public string Icon { get => _icon; set { _icon = value; } }
        //private string _icon = "";

        [Browsable(false)]
        public string? Icon { get => IconAsset.AssetFileName; set => IconAsset.AssetFileName = value; }

        [JsonIgnore]
        public MexTextureAsset IconAsset { get; set; } = new MexTextureAsset()
        {
            AssetPath = "series/icon",
            Width = 80,
            Height = 64,
            Format = HSDRaw.GX.GXTexFmt.I4,
        };

        // TODO: 3d emblem

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ws"></param>
        //public override void GenerateAssetPaths(MexWorkspace ws)
        //{
        //    Icon = GeneratePathIfNotExists(ws, Icon, "series", "icon", ".png");
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ws"></param>
        //public override void RemoveAssets(MexWorkspace ws)
        //{
        //    RemoveTexAsset(ws, Icon);
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
