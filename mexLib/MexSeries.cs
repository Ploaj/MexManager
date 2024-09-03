using System.ComponentModel;

namespace mexLib
{
    public class MexSeries
    {
        [Category("General"), DisplayName("Name"), Description("Name of the series")]
        public string Name { get; set; } = "";

        [Browsable(false)]
        public MexPlaylist Playlist { get; set; } = new MexPlaylist();

        // TODO: 3d emblem
        // TODO: 2d emblem

        public override string ToString()
        {
            return Name;
        }
    }
}
