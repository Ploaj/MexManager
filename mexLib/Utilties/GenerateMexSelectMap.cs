using HSDRaw.Common.Animation;
using HSDRaw.Common;
using HSDRaw.Melee.Mn;
using HSDRaw;
using HSDRaw.MEX.Stages;

namespace mexLib.Utilties
{
    public class GenerateMexSelectMap
    {
        /// <summary>
        /// 
        /// </summary>
        public static bool Compile(MexWorkspace ws)
        {
            var path = ws.GetFilePath("MnSlMap.usd");
            var data = ws.FileManager.Get(path);

            if (data == Array.Empty<byte>())
                return false;

            HSDRawFile file = new(path);
            ClearOldMaterialAnimations(file["MnSelectStageDataTable"].Data as SBM_SelectChrDataTable);
            file.CreateUpdateSymbol("mexMapData", GenerateMexSelect(ws));
            using MemoryStream stream = new MemoryStream();
            file.Save(stream);
            ws.FileManager.Set(path, stream.ToArray());
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        private static void ClearOldMaterialAnimations(SBM_SelectChrDataTable? tb)
        {
            if (tb == null) return;

            // TODO: remove old name tags
            // TODO: remove old icons animation
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        private static MEX_mexMapData GenerateMexSelect(MexWorkspace ws)
        {
            HSD_JOBJ jobj = new ()
            {
                SX = 1,
                SY = 1,
                SZ = 1,
            };

            HSD_AnimJoint anim = new ();

            foreach (var icon in ws.Project.StageSelects[0].StageIcons)
            {
                jobj.AddChild(icon.ToJoint());
                anim.AddChild(icon.ToJointAnim());
            }

            jobj.AddChild(ws.Project.StageSelects[0].RandomIcon.ToJoint());
            anim.AddChild(ws.Project.StageSelects[0].RandomIcon.ToJointAnim());

            jobj.UpdateFlags();

            var icon_joint = new HSDRawFile(ws.GetAssetPath("sss//icon_joint.dat")).Roots[0].Data as HSD_JOBJ;

            // generate mat anim joint
            List<HSD_TOBJ> icon_images = new ();
            List<HSD_TOBJ> names_images = new ();

            icon_images.Add(new MexImage(ws.GetAssetPath("sss\\Null_icon.tex")).ToTObj());
            icon_images.Add(new MexImage(ws.GetAssetPath("sss\\Locked_icon.tex")).ToTObj());

            for (int i = 0; i < ws.Project.StageSelects[0].StageIcons.Count; i++)
            {
                var external_id = ws.Project.StageSelects[0].StageIcons[i].StageID;
                var internal_id = MexStageIDConverter.ToInternalID(external_id);
                var stage = ws.Project.Stages[internal_id];
                var name = stage.Name;

                icon_images.Add(new MexImage(ws.GetAssetPath($"sss\\{name}_icon.tex")).ToTObj());
                names_images.Add(new MexImage(ws.GetAssetPath($"sss\\{name}_tag.tex")).ToTObj());
            }

            names_images.Add(new MexImage(ws.GetAssetPath("sss\\Random_tag.tex")).ToTObj());

            // generate structure
            return new MEX_mexMapData()
            {
                IconModel = icon_joint,
                IconAnimJoint = new HSD_AnimJoint()
                {
                    Child = new HSD_AnimJoint()
                },
                IconMatAnimJoint = new HSD_MatAnimJoint()
                {
                    Child = new HSD_MatAnimJoint()
                    {
                        MaterialAnimation = new HSD_MatAnim()
                        {
                            Next = new HSD_MatAnim()
                            {
                                TextureAnimation = new HSD_TexAnim().GenerateTextureAnimation(icon_images, null)
                            }
                        }
                    }
                },
                PositionModel = jobj,
                PositionAnimJoint = anim,
                StageNameMaterialAnimation = new HSD_MatAnimJoint()
                {
                    Child = new HSD_MatAnimJoint()
                    {
                        Child = new HSD_MatAnimJoint()
                        {
                            MaterialAnimation = new HSD_MatAnim()
                            {
                                TextureAnimation = new HSD_TexAnim().GenerateTextureAnimation(names_images, null)
                            }
                        }
                    }
                }
            };
        }
    }
}
