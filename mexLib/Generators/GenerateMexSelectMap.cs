using HSDRaw.Common.Animation;
using HSDRaw.Common;
using HSDRaw.Melee.Mn;
using HSDRaw;
using HSDRaw.MEX.Stages;
using HSDRaw.Tools;

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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static HSD_JOBJ GenerateJoint(MexWorkspace workspace)
        {
            HSD_JOBJ jobj = new()
            {
                Flags = JOBJ_FLAG.CLASSICAL_SCALING,
                SX = 1,
                SY = 1,
                SZ = 1,
            };

            foreach (var ss in workspace.Project.StageSelects)
            {
                foreach (var icon in ss.StageIcons)
                {
                    var j = icon.ToJoint();
                    if (icon.StageID == 0 && icon.Status != Types.MexStageSelectIcon.StageIconStatus.Locked)
                    {
                        j.SX = 1;
                        j.SY = 1;
                    }
                    jobj.AddChild(j);
                }
            }

            jobj.UpdateFlags();
            return jobj;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static HSD_AnimJoint GenerateDummyAnimJoint()
        {
            var dummyKeys = new HSD_FOBJDesc();

            dummyKeys.SetKeys(new ()
            {
                new ()
                {
                    Frame = 0,
                    Value = 1000,
                    InterpolationType = GXInterpolationType.HSD_A_OP_KEY
                }
            }, (int)JointTrackType.HSD_A_J_TRAX);

            return new HSD_AnimJoint()
            {
                AOBJ = new HSD_AOBJ()
                {
                    FObjDesc = dummyKeys
                }
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static List<HSD_AnimJoint> GenerateAnimJoint(MexWorkspace workspace)
        {
            var anims = new List<HSD_AnimJoint>();
            int total_count = workspace.Project.StageSelects.Sum(e => e.StageIcons.Count);
            int offset = 0;

            // process all stage pages
            foreach (var ss in workspace.Project.StageSelects)
            {
                HSD_AnimJoint root = new ();

                // add dummies before
                for (int i = 0; i < offset; i++)
                    root.AddChild(GenerateDummyAnimJoint());

                // add this page's icon animation
                HSD_AnimJoint anim = ss.Template.GenerateJointAnim(ss.StageIcons);
                root.AddChild(anim.Child);
                offset += ss.StageIcons.Count;

                // add dummies after
                for (int i = offset; i < total_count; i++)
                    root.AddChild(GenerateDummyAnimJoint());

                anims.Add(root);
            }

            return anims;
        }
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

            // merge jobj and generate multiple anims
            var jobj = GenerateJoint(ws);
            var anim = GenerateAnimJoint(ws);

            // generate mat anim joint
            List<HSD_TOBJ> icon_images = new();
            List<HSD_TOBJ> names_images = new();

            var nullIcon = reserved.SSSNullAsset.GetTexFile(ws);
            nullIcon ??= new MexImage(8, 8, HSDRaw.GX.GXTexFmt.CI8, HSDRaw.GX.GXTlutFmt.RGB565);

            var lockedIcon = reserved.SSSLockedNullAsset.GetTexFile(ws);
            lockedIcon ??= new MexImage(8, 8, HSDRaw.GX.GXTexFmt.CI8, HSDRaw.GX.GXTlutFmt.RGB565);

            icon_images.Add(nullIcon.ToTObj());
            icon_images.Add(lockedIcon.ToTObj());

            var keysBanner = new List<FOBJKey>();
            var keysIcon = new List<FOBJKey>();
            int index = 0;
            keysIcon.Add(new FOBJKey()
            {
                Frame = 0,
                Value = 0,
                InterpolationType = GXInterpolationType.HSD_A_OP_CON,
            });
            keysIcon.Add(new FOBJKey()
            {
                Frame = 1,
                Value = 1,
                InterpolationType = GXInterpolationType.HSD_A_OP_CON,
            });
            foreach (var ss in ws.Project.StageSelects)
            {
                for (int i = 0; i < ss.StageIcons.Count; i++)
                {
                    // check for random
                    if (ss.StageIcons[i].Status == Types.MexStageSelectIcon.StageIconStatus.Random)
                    {
                        var randomBanner = reserved.SSSRandomBannerAsset.GetTexFile(ws);
                        //randomBanner ??= new MexImage(8, 8, HSDRaw.GX.GXTexFmt.I4, HSDRaw.GX.GXTlutFmt.RGB565);
                        if (randomBanner != null)
                        {
                            keysBanner.Add(new FOBJKey()
                            {
                                Frame = index,
                                Value = names_images.Count,
                                InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                            });
                            names_images.Add(randomBanner.ToTObj());
                        }
                    }
                    else
                    {
                        var external_id = ss.StageIcons[i].StageID;
                        var internal_id = MexStageIDConverter.ToInternalID(external_id);

                        var stage = ws.Project.Stages[internal_id];

                        var icon = stage.Assets.IconAsset.GetTexFile(ws);
                        //icon ??= new MexImage(8, 8, HSDRaw.GX.GXTexFmt.CI8, HSDRaw.GX.GXTlutFmt.RGB565);

                        var banner = stage.Assets.BannerAsset.GetTexFile(ws);
                        //banner ??= new MexImage(8, 8, HSDRaw.GX.GXTexFmt.I4, HSDRaw.GX.GXTlutFmt.RGB565);

                        if (icon != null)
                        {
                            keysIcon.Add(new FOBJKey()
                            {
                                Frame = index + 2,
                                Value = icon_images.Count,
                                InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                            });
                            icon_images.Add(icon.ToTObj());
                        }
                        if (banner != null)
                        {
                            keysBanner.Add(new FOBJKey()
                            {
                                Frame = index,
                                Value = names_images.Count,
                                InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                            });
                            names_images.Add(banner.ToTObj());
                        }
                    }
                    index++;
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
            var mexMapData =  new MEX_mexMapData()
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
                                TextureAnimation = new HSD_TexAnim().GenerateTextureAnimation(icon_images, keysIcon)
                            }
                        }
                    }
                },
                PositionModel = jobj,
                PositionAnimJoint = anim.Count > 0 ? anim[0] : null,
                StageNameMaterialAnimation = new HSD_MatAnimJoint()
                {
                    Child = new HSD_MatAnimJoint()
                    {
                        Child = new HSD_MatAnimJoint()
                        {
                            MaterialAnimation = new HSD_MatAnim()
                            {
                                TextureAnimation = new HSD_TexAnim().GenerateTextureAnimation(names_images, keysBanner)
                            }
                        }
                    }
                },
                PageData = new HSDRaw.MEX.Menus.MEX_PageStruct()
                {
                    PageCount = project.StageSelects.Count,
                    Anims = new HSDFixedLengthPointerArrayAccessor<HSD_AnimJoint>()
                    {
                        Array = anim.ToArray()
                    }
                }
            };

            return mexMapData;
        }
    }
}
