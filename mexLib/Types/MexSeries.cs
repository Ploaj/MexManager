using mexLib.AssetTypes;
using mexLib.Installer;
using mexLib.Utilties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Compression;
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

        [Browsable(false)]
        [JsonInclude]
        public string? Icon { get => IconAsset.AssetFileName; internal set => IconAsset.AssetFileName = value; }

        [JsonIgnore]
        public MexTextureAsset IconAsset { get; set; } = new MexTextureAsset()
        {
            AssetPath = "series/icon",
            Width = 80,
            Height = 64,
            Format = HSDRaw.GX.GXTexFmt.I4,
        };

        [Browsable(false)]
        [JsonInclude]
        public string? Model { get => ModelAsset.AssetFileName; internal set => ModelAsset.AssetFileName = value; }

        [JsonIgnore]
        public MexOBJAsset ModelAsset { get; set; } = new MexOBJAsset()
        {
            AssetPath = "series/emblem",
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="workspace"></param>
        /// <param name="stage"></param>
        public static void ToPackage(Stream s, MexWorkspace workspace, MexSeries stage)
        {
            using var zip = new ZipWriter(s);

            zip.WriteAsJson("stage.json", stage);
            zip.TryWriteTextureAsset(workspace, stage.IconAsset, "icon.png");
            var obj = stage.ModelAsset.GetOBJFile(workspace);
            if (obj != null)
            {
                using var objStream = new MemoryStream();
                obj.Write(objStream);
                zip.Write("icon.obj", objStream.ToArray());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public static MexInstallerError? FromPackage(Stream s, MexWorkspace workspace, out MexSeries? series)
        {
            series = null;
            using ZipArchive zip = new(s);

            {
                var entry = zip.GetEntry("series.json");
                if (entry == null)
                    return new MexInstallerError("\"series.json\" was not found in zip");

                // parse group entry
                series = MexJsonSerializer.Deserialize<MexSeries>(entry.Extract());
                if (series == null)
                    return new MexInstallerError("Error parsing \"series.json\"");

                // init playlist
                series.Playlist = new MexPlaylist()
                {
                    Entries = new ObservableCollection<MexPlaylistEntry>()
                };

                //
                series.IconAsset.SetFromPackage(workspace, zip, "icon.png");
                series.ModelAsset.SetFromPackage(workspace, zip, "icon.obj");
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        public void Delete(MexWorkspace workspace)
        {
            IconAsset.Delete(workspace);
            ModelAsset.Delete(workspace);
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
