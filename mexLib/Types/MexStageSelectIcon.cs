using HSDRaw.Common;
using HSDRaw.Common.Animation;
using HSDRaw.MEX.Menus;
using mexLib;
using mexLib.AssetTypes;
using mexLib.Attributes;
using mexLib.Types;
using PropertyModels.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public class MexStageSelectIcon : MexIconBase
    {
        public override float BaseWidth => 2.9760742f;

        public override float BaseHeight => 2.603993f;

        public enum StageIconStatus
        {
            Hidden,
            Locked,
            Unlocked,
            Random,
            Decoration,
        }

        private StageIconStatus _status = StageIconStatus.Unlocked;
        [Category("0 - Stage")]
        [DisplayName("Icon Type")]
        public StageIconStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        private int _stageID;
        [Category("0 - Stage")]
        [DisplayName("Stage")]
        [MexLink(MexLinkType.Stage)]
        [VisibilityPropertyCondition(nameof(Status), StageIconStatus.Unlocked)]
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


        [Browsable(false)]
        [JsonInclude]
        public string? Icon { get => IconAsset.AssetFileName; internal set => IconAsset.AssetFileName = value; }

        [DisplayName("Icon")]
        [JsonIgnore]
        [VisibilityPropertyCondition(nameof(Status), StageIconStatus.Decoration)]
        public MexTextureAsset IconAsset { get; set; } = new MexTextureAsset()
        {
            AssetPath = "sss/page",
            Width = -1,
            Height = -1,
            Format = HSDRaw.GX.GXTexFmt.CI8,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
        };

        public enum IconAnimationKind
        {
            None,
            ScaleX,
        }

        private IconAnimationKind _iconAnimation;
        [DisplayName("Animation")]
        [JsonInclude]
        [VisibilityPropertyCondition(nameof(Status), StageIconStatus.Decoration)]
        public IconAnimationKind IconAnimation
        {
            get => _iconAnimation;
            set
            {
                _iconAnimation = value;
                OnPropertyChanged();
            }
        }

        private int _group;
        [Category("0 - Stage")]
        [DisplayName("Group")]
        [VisibilityPropertyCondition(nameof(Status), StageIconStatus.Decoration, LogicType = ConditionLogicType.Not)]
        public int Group
        {
            get => _group;
            set
            {
                _group = value;
                OnPropertyChanged();
            }
        }

        private float _width = 3.1f;
        [Category("1 - Collision")]
        [DisplayName("Width")]
        [VisibilityPropertyCondition(nameof(Status), StageIconStatus.Decoration, LogicType = ConditionLogicType.Not)]
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
        [VisibilityPropertyCondition(nameof(Status), StageIconStatus.Decoration, LogicType = ConditionLogicType.Not)]
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
        [VisibilityPropertyCondition(nameof(Status), StageIconStatus.Unlocked)]
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
        [VisibilityPropertyCondition(nameof(Status), StageIconStatus.Unlocked)]
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

        public override (float, float) CollisionOffset => (0, 0);
        public override (float, float) CollisionSize => (Width, Height);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private MexTextureAsset GetStageTextureAsset(MexWorkspace ws)
        {
            int internalId = MexStageIDConverter.ToInternalID(StageID);
            MexStage stage = ws.Project.Stages[internalId];
            return stage.Assets.IconAsset;
        }

        /// <summary>
        /// 
        /// </summary>
        public override int GetIconHash(MexWorkspace ws)
        {
            return Status switch
            {
                StageIconStatus.Random => -2,
                StageIconStatus.Locked => -1,
                StageIconStatus.Unlocked => GetStageTextureAsset(ws).GetHashCode(),
                StageIconStatus.Hidden => -3,
                StageIconStatus.Decoration => IconAsset.GetHashCode(),
                _ => -3
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public override MexImage? GetIconImage(MexWorkspace ws)
        {
            return Status switch
            {
                StageIconStatus.Random => ws.Project.ReservedAssets.SSSNullAsset.GetTexFile(ws),
                StageIconStatus.Locked => ws.Project.ReservedAssets.SSSLockedNullAsset.GetTexFile(ws),
                StageIconStatus.Unlocked => GetStageTextureAsset(ws).GetTexFile(ws),
                StageIconStatus.Decoration => IconAsset.GetTexFile(ws),
                StageIconStatus.Hidden => null,
                _ => null
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobj"></param>
        /// <param name="animjoint"></param>
        public void FromJoint(int joint_index, HSD_JOBJ jobj, HSD_AnimJoint animjoint)
        {
            X = jobj.TX;
            Y = jobj.TY;
            Z = jobj.TZ;

            if ((joint_index >= 1 && joint_index <= 6) || (joint_index == 18) || (joint_index == 19))
                Group = 0;

            if ((joint_index >= 7 && joint_index <= 11) || (joint_index == 17) || (joint_index == 0))
                Group = 1;

            if (joint_index >= 12 && joint_index <= 16)
                Group = 2;

            foreach (HSD_FOBJDesc? t in animjoint.AOBJ.FObjDesc.List)
            {
                List<HSDRaw.Tools.FOBJKey> keys = t.GetDecodedKeys();
                switch ((JointTrackType)t.TrackType)
                {
                    case JointTrackType.HSD_A_J_TRAX:
                        X = keys[^1].Value;
                        break;
                    case JointTrackType.HSD_A_J_TRAY:
                        Y = keys[^1].Value;
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
        /// <param name="icon"></param>
        public void FromIcon(MEX_StageIconData icon)
        {
            StageID = icon.ExternalID;
            PreviewID = icon.PreviewModelID;
            // random
            RandomSelectID = icon.RandomStageSelectID;
            Width = icon.CursorWidth;
            Height = icon.CursorHeight;
            ScaleX = icon.OutlineWidth;
            ScaleY = icon.OutlineHeight;

            if (StageID == 0)
            {
                Status = StageIconStatus.Random;
                PreviewID = 255;
                ScaleX = 1;
                ScaleY = 1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MEX_StageIconData? ToIcon()
        {
            if (Status == StageIconStatus.Decoration)
            {
                return null;
            }
            else
            if (Status == StageIconStatus.Random)
            {
                return new MEX_StageIconData()
                {
                    ExternalID = 0,
                    PreviewModelID = 255,
                    RandomEnabled = false,
                    RandomStageSelectID = 0,
                    CursorWidth = Width,
                    CursorHeight = Height,
                    OutlineWidth = 1.2f * ScaleX,
                    OutlineHeight = 1f * ScaleY,
                    IconState = (byte)Status,
                };
            }
            else
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
                    IconState = (byte)Status,
                };
            }
        }
    }

}
