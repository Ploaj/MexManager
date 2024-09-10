using HSDRaw.Common.Animation;
using HSDRaw.Common;
using HSDRaw.MEX.Menus;
using HSDRaw.Tools;
using mexLib.Attributes;

namespace mexLib.Types
{
    public class MexStageSelectIcon : MexIconBase
    {
        public override float BaseWidth => 3.5f;

        public override float BaseHeight => 3.4f;

        [MexLink(MexLinkType.Stage)]
        public int StageID { get; set; }

        public List<FOBJKey> AnimX { get; set; } = new List<FOBJKey>();

        public List<FOBJKey> AnimY { get; set; } = new List<FOBJKey>();

        public float Width { get; set; }

        public float Height { get; set; }

        public float OutlineWidth { get; set; }

        public float OutlineHeight { get; set; }

        public byte PreviewID { get; set; }

        public byte RandomSelectID { get; set; }

        public override int ImageKey => StageID;

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
                        X = keys[^1].Value;
                        AnimX = keys;
                        break;
                    case JointTrackType.HSD_A_J_TRAY:
                        Y = keys[^1].Value;
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
                SX = ScaleX,
                SY = ScaleY,
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
                HSD_FOBJDesc fobj = new();
                fobj.SetKeys(AnimX, (byte)JointTrackType.HSD_A_J_TRAX);
                if (aobj.FObjDesc == null)
                    aobj.FObjDesc = fobj;
                else
                    aobj.FObjDesc.Add(fobj);

                aobj.EndFrame = Math.Max(aobj.EndFrame, AnimX.Max(e => e.Frame));
            }

            if (AnimY.Count > 0)
            {
                HSD_FOBJDesc fobj = new();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public override MexImage? GetIconImage(MexWorkspace ws)
        {
            return null; // TODO: stage select icon
        }
    }

}
