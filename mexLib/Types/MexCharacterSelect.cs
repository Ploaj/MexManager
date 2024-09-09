using HSDRaw;
using HSDRaw.MEX.Menus;
using mexLib.Attributes;
using mexLib.MexScubber;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace mexLib.Types
{
    public class MexCharacterSelect
    {
        [DisplayName("Cursor Scale")]
        public float CharacterSelectHandScale { get; set; } = 1.0f;

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Browsable(false)]
        public MexCharacterSelectTemplate Template { get; set; } = new();

        [Browsable(false)]
        public ObservableCollection<MexCharacterSelectIcon> FighterIcons { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dol"></param>
        public void FromDOL(MexDOL dol)
        {
            // CSSIconData - 0x803F0A48 0x398
            MEX_IconData css = new()
            {
                _s = new HSDStruct(dol.GetData(0x803F0A48, 0x398))
            };
            // extract icon data
            foreach (var i in css.Icons)
            {
                FighterIcons.Add(new MexCharacterSelectIcon()
                {
                    Fighter = i.ExternalCharID,
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        public void ToMxDt(MexGenerator gen)
        {
            var tb = gen.Data.MenuTable;
            tb.Parameters.CSSHandScale = CharacterSelectHandScale;

            tb.CSSIconData = new MEX_IconData()
            {
                Icons = FighterIcons.Select((e, i) => e.ToIcon(i)).ToArray()
            };
        }
    }
}
