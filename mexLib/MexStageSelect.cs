using HSDRaw;
using HSDRaw.Common;
using HSDRaw.Common.Animation;
using HSDRaw.MEX.Menus;
using HSDRaw.Tools;
using mexLib.Attributes;
using mexLib.MexScubber;
using System.Collections.ObjectModel;

namespace mexLib
{
    public class MexStageSelectIcon
    {
        [MexLink(MexLinkType.Stage)]
        public int StageID { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public float ScaleX { get; set; } = 1;

        public float ScaleY { get; set; } = 1;

        public List<FOBJKey> AnimX { get; set; } = new List<FOBJKey>();

        public List<FOBJKey> AnimY { get; set; } = new List<FOBJKey>();

        public float Width { get; set; }

        public float Height { get; set; }

        public float OutlineWidth { get; set; }

        public float OutlineHeight { get; set; }

        public byte PreviewID { get; set; }

        public byte RandomSelectID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobj"></param>
        /// <param name="animjoint"></param>
        public void FromJoint(HSD_JOBJ jobj, HSD_AnimJoint animjoint)
        {
            X = jobj.TX;
            Y = jobj.TY;
            Z = jobj.TZ;

            foreach (var t in animjoint.AOBJ.FObjDesc.List)
            {
                var keys = t.GetDecodedKeys();
                switch ((JointTrackType)t.TrackType)
                {
                    case JointTrackType.HSD_A_J_TRAX:
                        X = keys[keys.Count - 1].Value;
                        AnimX = keys;
                        break;
                    case JointTrackType.HSD_A_J_TRAY:
                        Y = keys[keys.Count - 1].Value;
                        AnimY = keys;
                        break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public HSD_JOBJ ToJoint()
        {
            return new HSD_JOBJ()
            {
                Flags = JOBJ_FLAG.CLASSICAL_SCALING,
                TX = X,
                TY = Y,
                TZ = Z,
                SX = 1,
                SY = 1,
                SZ = 1,
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public HSD_AnimJoint ToJointAnim()
        {
            var aobj = new HSD_AOBJ()
            {
            };

            if (AnimX.Count > 0)
            {
                HSD_FOBJDesc fobj = new ();
                fobj.SetKeys(AnimX, (byte)JointTrackType.HSD_A_J_TRAX);
                if (aobj.FObjDesc == null)
                    aobj.FObjDesc = fobj;
                else
                    aobj.FObjDesc.Add(fobj);

                aobj.EndFrame = Math.Max(aobj.EndFrame, AnimX.Max(e => e.Frame));
            }

            if (AnimY.Count > 0)
            {
                HSD_FOBJDesc fobj = new ();
                fobj.SetKeys(AnimY, (byte)JointTrackType.HSD_A_J_TRAY);
                if (aobj.FObjDesc == null)
                    aobj.FObjDesc = fobj;
                else
                    aobj.FObjDesc.Add(fobj);

                aobj.EndFrame = Math.Max(aobj.EndFrame, AnimY.Max(e => e.Frame));
            }

            return new HSD_AnimJoint()
            {
                AOBJ = aobj
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="icon"></param>
        public void FromIcon(MEX_StageIconData icon)
        {
            StageID = icon.ExternalID;
            PreviewID = icon.PreviewModelID;
            // random
            RandomSelectID = icon.RandomStageSelectID;
            Width = icon.CursorWidth;
            Height = icon.CursorHeight;
            OutlineWidth = icon.OutlineWidth;
            OutlineHeight = icon.OutlineHeight;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MEX_StageIconData ToIcon()
        {
            return new MEX_StageIconData()
            {
                ExternalID = StageID,
                PreviewModelID = PreviewID,
                RandomEnabled = false,
                RandomStageSelectID = RandomSelectID,
                CursorWidth = Width,
                CursorHeight = Height,
                OutlineWidth = OutlineWidth,
                OutlineHeight = OutlineHeight,
            };
        }
    }

    public class MexStageSelect
    {
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<MexStageSelectIcon> StageIcons { get; set; } = new ObservableCollection<MexStageSelectIcon>();

        public MexStageSelectIcon RandomIcon { get; set; } = new MexStageSelectIcon();

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
