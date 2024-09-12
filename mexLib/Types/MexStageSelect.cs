using HSDRaw;
using HSDRaw.Common;
using HSDRaw.Common.Animation;
using HSDRaw.MEX.Menus;
using mexLib.MexScubber;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace mexLib.Types
{
    public class MexStageSelect
    {
        [Browsable(false)]
        public ObservableCollection<MexStageSelectIcon> StageIcons { get; set; } = new ObservableCollection<MexStageSelectIcon>();

        [Browsable(false)]
        public MexStageSelectIcon RandomIcon { get; set; } = new MexStageSelectIcon();

        [DisplayName("Cursor Start X")]
        public float StageSelectCursorStartX { get; set; } = 0;

        [DisplayName("Cursor Start Y")]
        public float StageSelectCursorStartY { get; set; } = -17;

        [DisplayName("Cursor Start Z")]
        public float StageSelectCursorStartZ { get; set; } = 0;

        [Browsable(false)]
        public MexStageSelectTemplate Template { get; set; } = new MexStageSelectTemplate();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public HSD_JOBJ GenerateJoint()
        {
            HSD_JOBJ jobj = new()
            {
                Flags = JOBJ_FLAG.CLASSICAL_SCALING,
                SX = 1,
                SY = 1,
                SZ = 1,
            };

            foreach (var icon in StageIcons)
                jobj.AddChild(icon.ToJoint());

            var random = RandomIcon.ToJoint();
            random.SX = 1;
            random.SY = 1;
            jobj.AddChild(random);

            jobj.UpdateFlags();

            return jobj;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public HSD_AnimJoint GenerateAnimJoint()
        {
            HSD_AnimJoint anim = Template.GenerateJointAnim(StageIcons);
            anim.AddChild(Template.GenerateJointAnim(RandomIcon));
            return anim;
        }
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

                if (i == 29)
                    RandomIcon = ico;
                else
                    StageIcons.Add(ico);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        public void ToMxDt(MexGenerator gen)
        {
            var tb = gen.Data.MenuTable;
            tb.Parameters.StageSelectCursorStartX = StageSelectCursorStartX;
            tb.Parameters.StageSelectCursorStartY = StageSelectCursorStartY;
            tb.Parameters.StageSelectCursorStartZ = StageSelectCursorStartZ;

            var icons = StageIcons.Select(e => e.ToIcon()).ToList();
            icons.Add(RandomIcon.ToIcon());

            // generate icons
            tb.SSSIconData = new HSDArrayAccessor<MEX_StageIconData>()
            {
                Array = icons.ToArray(),
            };

            // generate random bitfield
            var bitfield = new byte[StageIcons.Count / 8 + 1];
            for (int i = 0; i < bitfield.Length; i++)
                bitfield[i] = 0xFF;
            tb.SSSBitField = new SSSBitfield() { Array = bitfield };

            gen.Data.MenuTable = tb;
        }
    }
}
