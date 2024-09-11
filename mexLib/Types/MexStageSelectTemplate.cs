using HSDRaw.Common.Animation;
using HSDRaw.Tools;
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
    }
}
