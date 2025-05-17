using System.ComponentModel;
using System.Drawing;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public partial class MexFighter
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class GAWColor
        {
            [Browsable(false)]
            public uint Fill { get; set; }

            [Browsable(false)]
            public uint Outline { get; set; }

            [DisplayName("Fill Color")]
            [JsonIgnore]
            public Color FillColor { get => Color.FromArgb((int)Fill); set => Fill = (uint)value.ToArgb(); }

            [DisplayName("Outline Color")]
            [JsonIgnore]
            public Color OutlineColor { get => Color.FromArgb((int)Outline); set => Outline = (uint)value.ToArgb(); }
        }

        public class FighterGaWExt
        {
            public BindingList<GAWColor> Colors { get; set; } = new();
        }

        [Browsable(false)]
        public FighterGaWExt? GaWData { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dol"></param>
        /// <param name="index"></param>
        private void LoadExtFromDol(uint index)
        {
            // game and watch colors
            if (index == 24)
            {
                GaWData = new FighterGaWExt()
                {
                    Colors = new BindingList<GAWColor>()
                    {
                        new () { Fill = 0xFF000000, Outline = 0x80FFFFFF },
                        new () { Fill = 0xFF6E0000, Outline = 0x80FFFFFF },
                        new () { Fill = 0xFF00006E, Outline = 0x80FFFFFF },
                        new () { Fill = 0xFF006E00, Outline = 0x80FFFFFF },
                    }
                };
            }
        }
    }
}
