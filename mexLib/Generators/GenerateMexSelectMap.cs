﻿using HSDRaw;
using HSDRaw.Common;
using HSDRaw.Common.Animation;
using HSDRaw.GX;
using HSDRaw.Melee.Mn;
using HSDRaw.MEX.Stages;
using HSDRaw.Tools;
using mexLib.Types;
using mexLib.Utilties;

namespace mexLib.Generators
{
    public class GenerateMexSelectMap
    {
        /// <summary>
        /// 
        /// </summary>
        public static bool Compile(MexWorkspace ws)
        {
            string path = ws.GetFilePath("MnSlMap.usd");
            byte[] data = ws.FileManager.Get(path);

            if (data == Array.Empty<byte>())
                return false;

            HSDRawFile file = new(path);
            ClearOldMaterialAnimations(file["MnSelectStageDataTable"].Data as SBM_SelectChrDataTable);
            file.CreateUpdateSymbol("mexMapData", GenerateMexSelect(ws, file));
            file.TrimData(); // trim
            using MemoryStream stream = new();
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
        private static HSD_JOBJ GenerateJoint(IEnumerable<MexStageSelectIcon> icons)
        {
            HSD_JOBJ jobj = new()
            {
                Flags = JOBJ_FLAG.CLASSICAL_SCALING,
                SX = 1,
                SY = 1,
                SZ = 1,
            };

            foreach (MexStageSelectIcon icon in icons)
            {
                HSD_JOBJ j = icon.ToJoint();
                if (icon.StageID == 0 && icon.Status != Types.MexStageSelectIcon.StageIconStatus.Locked)
                {
                    j.SX = 1;
                    j.SY = 1;
                }
                jobj.AddChild(j);
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
            HSD_FOBJDesc dummyKeys = new();

            dummyKeys.SetKeys(new()
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
        /// <param name="type"></param>
        /// <param name="framecount"></param>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <returns></returns>
        private static HSD_FOBJDesc CreateTrack(JointTrackType type, float framecount, float startValue, float endValue)
        {
            HSD_FOBJDesc track = new();
            track.SetKeys(new()
            {
                new ()
                {
                    Frame = 0,
                    Value = startValue,
                    InterpolationType = GXInterpolationType.HSD_A_OP_LIN
                },
                new ()
                {
                    Frame = framecount,
                    Value = endValue,
                    InterpolationType = GXInterpolationType.HSD_A_OP_LIN
                }
            }, (byte)type);
            return track;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static HSD_AnimJoint GenerateDecorativeAnimJoint(bool visible, MexStageSelectIcon.IconAnimationKind kind)
        {
            HSD_FOBJDesc visTrack = new();
            visTrack.SetKeys(new()
            {
                new ()
                {
                    Frame = 0,
                    Value = visible ? 1 : 0,
                    InterpolationType = GXInterpolationType.HSD_A_OP_KEY
                }
            }, (int)JointTrackType.HSD_A_J_BRANCH);

            switch (kind)
            {
                case MexStageSelectIcon.IconAnimationKind.ScaleX:
                    visTrack.Add(CreateTrack(JointTrackType.HSD_A_J_SCAX, 10, 0, 1));
                    break;
            }

            return new HSD_AnimJoint()
            {
                AOBJ = new HSD_AOBJ()
                {
                    EndFrame = 10,
                    FObjDesc = visTrack
                }
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static List<HSD_AnimJoint> GenerateAnimJoint(MexWorkspace workspace)
        {
            List<HSD_AnimJoint> anims = new();
            int total_count = workspace.Project.StageSelects.Sum(e => e.StageIcons.Count(e => e.Status != MexStageSelectIcon.StageIconStatus.Decoration));
            int offset = 0;

            // process all stage pages
            foreach (MexStageSelect ss in workspace.Project.StageSelects)
            {
                HSD_AnimJoint root = new();

                // add dummies before
                for (int i = 0; i < offset; i++)
                    root.AddChild(GenerateDummyAnimJoint());

                // add this page's icon animation
                IEnumerable<MexStageSelectIcon> icons = ss.StageIcons.Where(e => e.Status != MexStageSelectIcon.StageIconStatus.Decoration);
                HSD_AnimJoint anim = ss.Template.GenerateJointAnim(icons);
                root.AddChild(anim.Child);
                offset += icons.Count();

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
            SBM_MnSelectStageDataTable? dataTable = file["MnSelectStageDataTable"].Data as SBM_MnSelectStageDataTable;

            MexProject project = ws.Project;
            MexReservedAssets reserved = project.ReservedAssets;

            IEnumerable<MexStageSelectIcon> icons = ws.Project.StageSelects.SelectMany(e => e.StageIcons).Where(e => e.Status != MexStageSelectIcon.StageIconStatus.Decoration);

            // merge jobj and generate multiple anims
            HSD_JOBJ jobj = GenerateJoint(icons);
            List<HSD_AnimJoint> anim = GenerateAnimJoint(ws);

            // generate mat anim joint
            List<HSD_TOBJ> icon_images = new();
            List<HSD_TOBJ> names_images = new();

            MexImage? nullIcon = reserved.SSSNullAsset.GetTexFile(ws);
            nullIcon ??= new MexImage(8, 8, GXTexFmt.CI8, HSDRaw.GX.GXTlutFmt.RGB565);

            MexImage? lockedIcon = reserved.SSSLockedNullAsset.GetTexFile(ws);
            lockedIcon ??= new MexImage(8, 8, GXTexFmt.CI8, HSDRaw.GX.GXTlutFmt.RGB565);

            icon_images.Add(nullIcon.ToTObj());
            icon_images.Add(lockedIcon.ToTObj());

            List<FOBJKey> keysBanner = new();
            List<FOBJKey> keysIcon = new();
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
            foreach (MexStageSelectIcon? ss in icons)
            {
                // check for random
                if (ss.Status == MexStageSelectIcon.StageIconStatus.Random)
                {
                    MexImage? randomBanner = reserved.SSSRandomBannerAsset.GetTexFile(ws);
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
                    int external_id = ss.StageID;
                    int internal_id = MexStageIDConverter.ToInternalID(external_id);

                    MexStage stage = ws.Project.Stages[internal_id];

                    MexImage? icon = stage.Assets.IconAsset.GetTexFile(ws);
                    //icon ??= new MexImage(8, 8, HSDRaw.GX.GXTexFmt.CI8, HSDRaw.GX.GXTlutFmt.RGB565);

                    MexImage? banner = stage.Assets.BannerAsset.GetTexFile(ws);
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

            // sss icon could be generated on save
            HSD_JOBJ model;
            if (dataTable != null)
            {
                model = HSDAccessor.DeepClone<HSD_JOBJ>(dataTable.IconDoubleModel);
                HSD_JOBJ icon_joint = model.Child.Next;
                model.Child = icon_joint;
                model.Optimize();
            }
            else
            {
                model = new HSD_JOBJ()
                {
                };
            }

            // generate decoration objects
            int page_index = 0;
            foreach (MexStageSelect page in ws.Project.StageSelects)
            {
                foreach (MexStageSelectIcon icon in page.StageIcons)
                {
                    if (icon.Status != MexStageSelectIcon.StageIconStatus.Decoration)
                        continue;

                    float width = icon.BaseWidth * icon.ScaleX;
                    float height = icon.BaseHeight * icon.ScaleY;

                    // create joint
                    HSD_JOBJ joint = new()
                    {
                        TX = icon.X,
                        TY = icon.Y,
                        SX = 1,
                        SY = 1,
                        SZ = 1,
                        // create dobj
                        Dobj = new HSD_DOBJ()
                        {
                            Mobj = new HSD_MOBJ()
                            {
                                RenderFlags = RENDER_MODE.CONSTANT | RENDER_MODE.XLU | RENDER_MODE.TEX0 | RENDER_MODE.NO_ZUPDATE,
                                Material = new HSD_Material()
                                {
                                    DIF_A = 255,
                                    DIF_R = 255,
                                    DIF_G = 255,
                                    DIF_B = 255,
                                    SPC_A = 255,
                                    SPC_R = 255,
                                    SPC_G = 255,
                                    SPC_B = 255,
                                    AMB_A = 255,
                                    AMB_R = 128,
                                    AMB_G = 128,
                                    AMB_B = 128,
                                    Alpha = 1,
                                    Shininess = 50,
                                },
                                Textures = icon.IconAsset.GetTexFile(ws)?.ToTObj()
                            }
                        }
                    };

                    // pobj only has pos
                    POBJ_Generator gen = new();

                    GX_Vertex v1 = new() { POS = new GXVector3(-width, -height, 0), TEX0 = new GXVector2(0, 1) };
                    GX_Vertex v2 = new() { POS = new GXVector3(width, -height, 0), TEX0 = new GXVector2(1, 1) };
                    GX_Vertex v3 = new() { POS = new GXVector3(width, height, 0), TEX0 = new GXVector2(1, 0) };
                    GX_Vertex v4 = new() { POS = new GXVector3(-width, height, 0), TEX0 = new GXVector2(0, 0) };

                    List<GX_Vertex> verts = new()
                    {
                        v3, v2, v1,
                        v1, v4, v3,
                    };

                    joint.Dobj.Pobj = gen.CreatePOBJsFromTriangleList(
                        verts,
                        new GXAttribName[]
                        {
                            GXAttribName.GX_VA_POS,
                            GXAttribName.GX_VA_TEX0,
                            GXAttribName.GX_VA_NULL,
                        },
                        new List<HSD_Envelope>());

                    gen.SaveChanges();

                    jobj.AddChild(joint);

                    int anim_index = 0;
                    foreach (HSD_AnimJoint a in anim)
                    {
                        a.AddChild(GenerateDecorativeAnimJoint(page_index == anim_index, icon.IconAnimation));
                        anim_index++;
                    }
                }
                page_index++;
            }
            jobj.UpdateFlags();

            // generate structure
            MEX_mexMapData mexMapData = new()
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
