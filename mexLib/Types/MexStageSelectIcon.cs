using HSDRaw.Common.Animation;
using HSDRaw.Common;
using HSDRaw.MEX.Menus;
using HSDRaw.Tools;
using mexLib.Attributes;
using System.ComponentModel;

namespace mexLib.Types
{
    public class MexStageSelectIcon : MexIconBase
    {
        public override float BaseWidth => 2.9760742f;

        public override float BaseHeight => 2.603993f;

        private int _stageID;
        [Category("0 - Stage")]
        [DisplayName("Stage")]
        [MexLink(MexLinkType.Stage)]
        public int StageID
        {
            get => _stageID;
            set
            {
                if (_stageID != value)
                {
                    _stageID = value;
                    OnPropertyChanged();
                }
            }
        }

        private float _width = 3.1f;
        [Category("1 - Collision")]
        [DisplayName("Width")]
        public float Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        private float _height = 2.70f;
        [Category("1 - Collision")]
        [DisplayName("Height")]
        public float Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        private byte _previewID;
        [Category("0 - Stage")]
        [DisplayName("Preview ID")]
        public byte PreviewID
        {
            get => _previewID;
            set
            {
                if (_previewID != value)
                {
                    _previewID = value;
                    OnPropertyChanged();
                }
            }
        }

        private byte _randomSelectID;
        [Category("0 - Stage")]
        [DisplayName("Random ID")]
        public byte RandomSelectID
        {
            get => _randomSelectID;
            set
            {
                if (_randomSelectID != value)
                {
                    _randomSelectID = value;
                    OnPropertyChanged();
                }
            }
        }

        [Browsable(false)]
        public List<FOBJKey> AnimX { get; set; } = new List<FOBJKey>();

        [Browsable(false)]
        public List<FOBJKey> AnimY { get; set; } = new List<FOBJKey>();

        public override int ImageKey => StageID;

        public override (float, float) CollisionOffset => (0, 0);
        public override (float, float) CollisionSize => (Width, Height);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public override MexImage? GetIconImage(MexWorkspace ws)
        {
            var internalId = MexStageIDConverter.ToInternalID(StageID);
            var stage = ws.Project.Stages[internalId];

            return stage.Assets.IconAsset.GetTexFile(ws);
        }
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
                OutlineWidth = ScaleX,
                OutlineHeight = ScaleY,
            };
        }
    }

}
