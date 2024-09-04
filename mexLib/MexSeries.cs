using mexLib.Attributes;
using System.ComponentModel;

namespace mexLib
{
    public class MexSeries : MexAssetContainerBase
    {
        [Category("General"), DisplayName("Name"), Description("Name of the series")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        private string _name = "";

        [Browsable(false)]
        public MexPlaylist Playlist { get; set; } = new MexPlaylist();

        [Category("General"), DisplayName("Series Icon")]
        [MexTextureAsset]
        [MexTextureFormat(HSDRaw.GX.GXTexFmt.I4)]
        [MexTextureSize(80, 64)]
        [MexFilePathValidator(MexFilePathType.Assets)]
        public string Icon { get => _icon; set { _icon = value; OnPropertyChanged(); } }
        private string _icon = "";

        // TODO: 3d emblem

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        public override void GenerateAssetPaths(MexWorkspace ws)
        {
            Icon = GeneratePathIfNotExists(ws, Icon, "series", "icon", ".png");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        public override void RemoveAssets(MexWorkspace ws)
        {
            RemoveTexAsset(ws, Icon);
        }
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
