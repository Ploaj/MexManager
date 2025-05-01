using HSDRaw.Common.Animation;
using HSDRaw.Tools;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace mexLib.Types
{
    public class MexStageSelectTemplate : MexReactiveObject
    {
        private float _appearTime = 10;
        [DisplayName("Appear Time")]
        [Description("Time it takes for Group to reach their final position")]
        public float AppearTime { get => _appearTime; set { _appearTime = value; OnPropertyChanged(); } }

        private float _appearSpacing = 5;
        [DisplayName("Appear Spacing")]
        [Description("Time it takes for Group to begin moving")]
        public float AppearSpacing { get => _appearSpacing; set { _appearSpacing = value; OnPropertyChanged(); } }

        private float _startX = 36;
        [DisplayName("Start X")]
        [Description("The starting X position of the icons")]
        public float StartX { get => _startX; set { _startX = value; OnPropertyChanged(); } }

        [Browsable(false)]
        public ObservableCollection<MexStageSelectIconPlacementTemplate> IconPlacements { get; set; } = new ObservableCollection<MexStageSelectIconPlacementTemplate>();

        public class MexStageSelectIconPlacementTemplate
        {
            public int Group { get; set; }
            public float Width { get; set; }
            public float Height { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public float ScaleX { get; set; }
            public float ScaleY { get; set; }

            public override string ToString()
            {
                return $"({X}, {Y}) ({Width},{Height})";
            }

            public MexStageSelectIconPlacementTemplate Clone()
            {
                return new MexStageSelectIconPlacementTemplate()
                {
                    Group = Group,
                    Width = Width,
                    Height = Height,
                    X = X,
                    Y = Y,
                    Z = Z,
                    ScaleX = ScaleX,
                    ScaleY = ScaleY,
                };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public MexStageSelectTemplate()
        {
            if (IconPlacements.Count == 0)
                foreach (var i in VanillaPlacements)
                    IconPlacements.Add(i.Clone());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="icons"></param>
        public void MakeTemplate(IEnumerable<MexStageSelectIcon> icons)
        {
            IconPlacements.Clear();
            foreach (var i in icons)
            {
                IconPlacements.Add(new MexStageSelectIconPlacementTemplate()
                {
                    Group = i.Group,
                    Width = i.Width,
                    Height = i.Height,
                    X = i.X,
                    Y = i.Y,
                    Z = i.Z,
                    ScaleX = i.ScaleX,
                    ScaleY = i.ScaleY,
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="icons"></param>
        public void ApplyTemplate(IEnumerable<MexStageSelectIcon> icons)
        {
            int i = 0;
            foreach (var icon in icons)
            {
                if (i >= IconPlacements.Count)
                    break;

                var tem = IconPlacements[i];

                icon.Group = tem.Group;
                icon.Width = tem.Width;
                icon.Height = tem.Height;
                icon.X = tem.X;
                icon.Y = tem.Y;
                icon.Z = tem.Z;
                icon.ScaleX = tem.ScaleX;
                icon.ScaleY = tem.ScaleY;

                i++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public HSD_AnimJoint GenerateJointAnim(MexStageSelectIcon icon)
        {
            float start = AppearSpacing * icon.Group;
            float end = AppearTime + AppearSpacing * icon.Group;

            var keys = new List<FOBJKey>();

            if (start != 0)
            {
                keys.Add(new FOBJKey()
                {
                    Frame = 0,
                    Value = StartX,
                    InterpolationType = GXInterpolationType.HSD_A_OP_LIN,
                });
            }

            keys.Add(new FOBJKey()
            {
                Frame = start,
                Value = StartX,
                InterpolationType = GXInterpolationType.HSD_A_OP_LIN,
            });

            keys.Add(new FOBJKey()
            {
                Frame = end,
                Value = icon.X,
                InterpolationType = GXInterpolationType.HSD_A_OP_LIN,
            });

            var aobj = new HSD_AOBJ()
            {
            };

            HSD_FOBJDesc fobj = new();
            fobj.SetKeys(keys, (byte)JointTrackType.HSD_A_J_TRAX);
            if (aobj.FObjDesc == null)
                aobj.FObjDesc = fobj;
            else
                aobj.FObjDesc.Add(fobj);

            aobj.EndFrame = Math.Max(aobj.EndFrame, keys.Max(e => e.Frame));

            return new HSD_AnimJoint()
            {
                AOBJ = aobj
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="icons"></param>
        /// <returns></returns>
        public HSD_AnimJoint GenerateJointAnim(IEnumerable<MexStageSelectIcon> icons)
        {
            var root = new HSD_AnimJoint();

            foreach (var i in icons)
                root.AddChild(GenerateJointAnim(i));

            return root;
        }

        private readonly static MexStageSelectIconPlacementTemplate[] VanillaPlacements =
        {
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = -16.498047f,
                Y = 15.699995f,
                Z = 1f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = -16.498047f,
                Y = 10.099995f,
                Z = 0.5f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = -9.8984375f,
                Y = 15.699995f,
                Z = 1f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = -9.8984375f,
                Y = 10.099995f,
                Z = 0.5f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = -3.2988281f,
                Y = 15.699995f,
                Z = 1f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = -3.2988281f,
                Y = 10.099995f,
                Z = 0.5f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = 3.2998047f,
                Y = 15.699995f,
                Z = 1f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = 3.2998047f,
                Y = 10.099995f,
                Z = 0.5f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = 9.899414f,
                Y = 15.699995f,
                Z = 1f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = 9.899414f,
                Y = 10.099995f,
                Z = 0.5f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = 16.499023f,
                Y = 15.700005f,
                Z = 1f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.7f,
                X = 16.499023f,
                Y = 10.100004f,
                Z = 0.5f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 1,
                Width = 3.1f,
                Height = 2.7f,
                X = -4.5996094f,
                Y = 3.700001f,
                Z = 0.8f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 1,
                Width = 3.1f,
                Height = 2.7f,
                X = -4.5996094f,
                Y = -1.8999989f,
                Z = 0.3f,
                ScaleX = 1f,
                ScaleY = 1f,
            },

            new ()
            {
                Group = 1,
                Width = 3.1f,
                Height = 2.7f,
                X = 2f,
                Y = 3.700001f,
                Z = 0.8f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 1,
                Width = 3.1f,
                Height = 2.7f,
                X = 2f,
                Y = -1.8999989f,
                Z = 0.3f,
                ScaleX = 1f,
                ScaleY = 1f,
            },


            new ()
            {
                Group = 1,
                Width = 3.1f,
                Height = 2.7f,
                X = 8.599609f,
                Y = 3.700001f,
                Z = 0.8f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 1,
                Width = 3.1f,
                Height = 2.7f,
                X = 8.599609f,
                Y = -1.8999989f,
                Z = 0.3f,
                ScaleX = 1f,
                ScaleY = 1f,
            },


            new ()
            {
                Group = 1,
                Width = 3.1f,
                Height = 2.7f,
                X = 15.199219f,
                Y = 3.700001f,
                Z = 0.8f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 1,
                Width = 3.1f,
                Height = 2.7f,
                X = 15.199219f,
                Y = -1.8999989f,
                Z = 0.3f,
                ScaleX = 1f,
                ScaleY = 1f,
            },


            new ()
            {
                Group = 1,
                Width = 3.1f,
                Height = 2.7f,
                X = 21.799805f,
                Y = 3.699994f,
                Z = 0.8f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
            new ()
            {
                Group = 1,
                Width = 3.1f,
                Height = 2.7f,
                X = 21.799805f,
                Y = -1.9000058f,
                Z = 0.3f,
                ScaleX = 1f,
                ScaleY = 1f,
            },


            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.9f,
                X = -23.09961f,
                Y = 13.699999f,
                Z = 0.4f,
                ScaleX = 1f,
                ScaleY = 1.1f,
            },
            new ()
            {
                Group = 0,
                Width = 3.1f,
                Height = 2.9f,
                X = 23.09961f,
                Y = 14f,
                Z = 0.4f,
                ScaleX = 1f,
                ScaleY = 1.1f,
            },
            new ()
            {
                Group = 2,
                Width = 2.9f,
                Height = 2.1f,
                X = 1.2998047f,
                Y = -9.100002f,
                Z = 0f,
                ScaleX = 0.8f,
                ScaleY = 0.8f,
            },
            new ()
            {
                Group = 2,
                Width = 2.9f,
                Height = 2.1f,
                X = 6.5996094f,
                Y = -9.100002f,
                Z = 0f,
                ScaleX = 0.8f,
                ScaleY = 0.8f,
            },
            new ()
            {
                Group = 2,
                Width = 2.9f,
                Height = 2.1f,
                X = 12.299805f,
                Y = -9.100002f,
                Z = 0f,
                ScaleX = 0.8f,
                ScaleY = 0.8f,
            },
            new ()
            {
                Group = 2,
                Width = 2.9f,
                Height = 2.1f,
                X = 17.59961f,
                Y = -9.100002f,
                Z = 0f,
                ScaleX = 0.8f,
                ScaleY = 0.8f,
            },
            new ()
            {
                Group = 2,
                Width = 2.9f,
                Height = 2.1f,
                X = 22.899414f,
                Y = -9.100002f,
                Z = 0f,
                ScaleX = 0.8f,
                ScaleY = 0.8f,
            },
            new ()
            {
                Group = 1,
                Width = 3.6f,
                Height = 2.7f,
                X = -14.099609f,
                Y = 3.599984f,
                Z = 0.4f,
                ScaleX = 1f,
                ScaleY = 1f,
            },
        };
    }
}