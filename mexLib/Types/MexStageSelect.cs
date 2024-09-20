using HSDRaw;
using HSDRaw.Common;
using HSDRaw.Common.Animation;
using HSDRaw.MEX;
using HSDRaw.MEX.Menus;
using mexLib.MexScubber;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace mexLib.Types
{
    public class MexStageSelect : MexReactiveObject
    {
        [DisplayName("Name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        private string _name = "New Page";

        private MexStageSelectTemplate _template = new ();
        [Browsable(false)]
        public MexStageSelectTemplate Template { get => _template; set { _template = value; OnPropertyChanged(); } }

        [Browsable(false)]
        public ObservableCollection<MexStageSelectIcon> StageIcons { get; set; } = new ObservableCollection<MexStageSelectIcon>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dol"></param>
        public void FromDOL(MexDOL dol)
        {
            // SSSIconData - 0x803F06D0 30
            for (uint i = 0; i < 30; i++)
            {
                var stage_icon = new MEX_StageIconData()
                {
                    _s = new HSDStruct(dol.GetData(0x803F06D0 + i * 0x1C, 0x1C))
                };
                stage_icon._s.Resize(0x20);
                stage_icon.ExternalID = stage_icon._s.GetByte(0x0B); // move external id

                var ico = new MexStageSelectIcon();
                ico.FromIcon(stage_icon);

                StageIcons.Add(ico);
            }
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
