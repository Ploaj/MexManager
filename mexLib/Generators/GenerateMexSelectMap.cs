using HSDRaw.Common.Animation;
using HSDRaw.Common;
using HSDRaw.Melee.Mn;
using HSDRaw;
using HSDRaw.MEX.Stages;

namespace mexLib.Generators
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
            file.CreateUpdateSymbol("mexMapData", GenerateMexSelect(ws, file));
            using MemoryStream stream = new ();
            file.Save(stream);
            ws.FileManager.Set(path, stream.ToArray());

            //GenerateStageSelect(ws, file);

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
        /// <returns></returns>
        //private static void GenerateStageSelect(MexWorkspace ws, HSDRawFile file)
        //{
        //    var ss = new MEX_StageSelect()
        //    {
        //        PageCount = 2,
        //        Pages = new HSDFixedLengthPointerArrayAccessor<MEX_mexMapData>()
        //        {
        //            Array = new MEX_mexMapData[]
        //            {
        //                GenerateMexSelect(ws, file),
        //                GenerateMexSelect(ws, file),
        //            }
        //        }
        //    };

        //    var f = new HSDRawFile();
        //    f.Roots.Add(new HSDRootNode()
        //    {
        //        Name = "mexMapSelect",
        //        Data = ss,
        //    });
        //    using var stream = new MemoryStream();
        //    f.Save(stream);

        //    ws.FileManager.Set(ws.GetFilePath("MxSlMap.dat"), stream.ToArray());
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        private static MEX_mexMapData GenerateMexSelect(MexWorkspace ws, HSDRawFile file)
        {
            var dataTable = file["MnSelectStageDataTable"].Data as SBM_MnSelectStageDataTable;

            var project = ws.Project;
            var reserved = project.ReservedAssets;

            var jobj = ws.Project.StageSelects[0].GenerateJoint();
            var anim = ws.Project.StageSelects[0].GenerateAnimJoint();

            // generate mat anim joint
            List<HSD_TOBJ> icon_images = new();
            List<HSD_TOBJ> names_images = new();

            var nullIcon = reserved.SSSNullAsset.GetTexFile(ws);
            nullIcon ??= new MexImage(8, 8, HSDRaw.GX.GXTexFmt.CI8, HSDRaw.GX.GXTlutFmt.RGB565);

            var lockedIcon = reserved.SSSLockedNullAsset.GetTexFile(ws);
            lockedIcon ??= new MexImage(8, 8, HSDRaw.GX.GXTexFmt.CI8, HSDRaw.GX.GXTlutFmt.RGB565);

            icon_images.Add(nullIcon.ToTObj());
            icon_images.Add(lockedIcon.ToTObj());

            for (int i = 0; i < ws.Project.StageSelects[0].StageIcons.Count; i++)
            {
                var external_id = ws.Project.StageSelects[0].StageIcons[i].StageID;

                // check for random
                if (external_id == 0)
                {
                    var randomBanner = reserved.SSSRandomBannerAsset.GetTexFile(ws);
                    randomBanner ??= new MexImage(8, 8, HSDRaw.GX.GXTexFmt.I4, HSDRaw.GX.GXTlutFmt.RGB565);
                    names_images.Add(randomBanner.ToTObj());
                }
                else
                {
                    var internal_id = MexStageIDConverter.ToInternalID(external_id);
                    var stage = ws.Project.Stages[internal_id];

                    var icon = stage.Assets.IconAsset.GetTexFile(ws);
                    icon ??= new MexImage(8, 8, HSDRaw.GX.GXTexFmt.CI8, HSDRaw.GX.GXTlutFmt.RGB565);

                    var banner = stage.Assets.BannerAsset.GetTexFile(ws);
                    banner ??= new MexImage(8, 8, HSDRaw.GX.GXTexFmt.I4, HSDRaw.GX.GXTlutFmt.RGB565);

                    icon_images.Add(icon.ToTObj());
                    names_images.Add(banner.ToTObj());
                }
            }

            // sss icon could be generated on save
            HSD_JOBJ model;
            if (dataTable != null)
            {
                model = HSDAccessor.DeepClone<HSD_JOBJ>(dataTable.IconDoubleModel);
                var icon_joint = model.Child.Next;
                model.Child = icon_joint;
            }
            else
            {
                model = new HSD_JOBJ()
                {
                };
            }

            // generate structure
            return new MEX_mexMapData()
            {
                IconModel = model,
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
                },
            };
        }
    }
}
