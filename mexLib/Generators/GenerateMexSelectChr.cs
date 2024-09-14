using HSDRaw.Common.Animation;
using HSDRaw.Common;
using HSDRaw.MEX.Menus;
using HSDRaw.Tools;
using HSDRaw;
using HSDRaw.GX;
using HSDRaw.Melee.Mn;
using mexLib.Types;
using System.Diagnostics;

namespace mexLib.Generators
{
    public static class GenerateMexSelectChr
    {
        /// <summary>
        /// 
        /// </summary>
        public static bool Compile(MexWorkspace ws)
        {
            var path = ws.GetFilePath("MnSlChr.usd");
            var data = ws.FileManager.Get(path);

            if (data == Array.Empty<byte>())
                return false;

            HSDRawFile file = new(path);
            ClearOldMaterialAnimations(file["MnSelectChrDataTable"].Data as SBM_SelectChrDataTable);
            file.CreateUpdateSymbol("mexSelectChr", GenerateMexSelect(ws));

            using MemoryStream stream = new ();
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

            // menu joint 51 - 54
            var menu = tb.MenuMaterialAnimation.TreeList;
            menu[51].MaterialAnimation.TextureAnimation = null;
            menu[52].MaterialAnimation.TextureAnimation = null;
            menu[53].MaterialAnimation.TextureAnimation = null;
            menu[54].MaterialAnimation.TextureAnimation = null;

            // single 45
            var single = tb.SingleMenuMaterialAnimation.TreeList;
            single[45].MaterialAnimation.TextureAnimation = null;

            // portrait 6
            var potrait = tb.PortraitMaterialAnimation.TreeList;
            potrait[6].MaterialAnimation.TextureAnimation = null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        private static MEX_mexSelectChr GenerateMexSelect(MexWorkspace ws)
        {
            // generate csps
            int stride = ws.Project.Fighters.Count;
            List<FOBJKey> keys = new();
            List<HSD_TOBJ> icons = new();

            // compression info
            ws.Project.CharacterSelect.ApplyCompression(ws);

            // gather reserved icons
            for (int internalId = 0; internalId < stride; internalId++)
            {
                int externalId = MexFighterIDConverter.ToExternalID(internalId, stride);
                var f = ws.Project.Fighters[internalId];
                int costume_index = 0;
                foreach (var c in f.Costumes)
                {
                    var textureAsset = c.CSPAsset.GetTexFile(ws);

                    if (textureAsset != null)
                    {
                        keys.Add(new FOBJKey()
                        {
                            Frame = externalId + stride * costume_index,
                            Value = icons.Count,
                            InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                        });
                        icons.Add(textureAsset.ToTObj());
                    }
                    else
                    {
                        keys.Add(new FOBJKey()
                        {
                            Frame = externalId + stride * costume_index,
                            Value = 0,
                            InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                        });
                    }
                    costume_index++;
                }
            }

            // generate icon model
            HSD_JOBJ root = new()
            {
                Flags = JOBJ_FLAG.CLASSICAL_SCALING,
                SX = 1,
                SY = 1,
                SZ = 1,
            };
            HSD_AnimJoint root_anim = new()
            {

            };
            HSD_MatAnimJoint root_matanim_joint = new()
            {

            };

            // generate model
            foreach (var i in ws.Project.CharacterSelect.FighterIcons)
            {
                root.AddChild(GenerateIconModel(i, ws));
                root_anim.AddChild(new HSD_AnimJoint()
                {

                });
                root_matanim_joint.AddChild(GenerateIconMatAnim());
            }

            root.UpdateFlags();

            return new MEX_mexSelectChr()
            {
                IconModel = root,
                IconAnimJoint = root_anim,
                IconMatAnimJoint = root_matanim_joint,
                CSPStride = stride,
                CSPMatAnim = new HSD_MatAnim()
                {
                    TextureAnimation = new HSD_TexAnim().GenerateTextureAnimation(icons, keys)
                },

            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static HSD_MatAnimJoint GenerateIconMatAnim()
        {
            var r = new HSD_FOBJDesc();
            r.SetKeys(
                new List<FOBJKey>()
                {
                    new (){ Frame = 0, Value = 0, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 10, Value = 0, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 13, Value = 0.5999756f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 16, Value = 0, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 19, Value = 0.5999756f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 22, Value = 0, InterpolationType = GXInterpolationType.HSD_A_OP_CON},
                    new (){ Frame = 600, Value = 0, InterpolationType = GXInterpolationType.HSD_A_OP_CON},
                },
                (byte)TexTrackType.HSD_A_T_TEV0_R);
            var g = new HSD_FOBJDesc();
            g.SetKeys(
                new List<FOBJKey>()
                {
                    new (){ Frame = 0, Value = 0.099975586f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 10, Value = 0.099975586f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 13, Value = 0.5999756f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 16, Value = 0.099975586f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 19, Value = 0.5999756f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 22, Value = 0.099975586f, InterpolationType = GXInterpolationType.HSD_A_OP_CON},
                    new (){ Frame = 600, Value = 0.099975586f, InterpolationType = GXInterpolationType.HSD_A_OP_CON},
                },
                (byte)TexTrackType.HSD_A_T_TEV0_G);
            var b = new HSD_FOBJDesc();
            b.SetKeys(
                new List<FOBJKey>()
                {
                    new (){ Frame = 0, Value = 0.19998169f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 10, Value =  0.19998169f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 13, Value = 0.5999756f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 16, Value =  0.19998169f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 19, Value = 0.5999756f, InterpolationType = GXInterpolationType.HSD_A_OP_LIN},
                    new (){ Frame = 22, Value =  0.19998169f, InterpolationType = GXInterpolationType.HSD_A_OP_CON},
                    new (){ Frame = 600, Value = 0.19998169f, InterpolationType = GXInterpolationType.HSD_A_OP_CON},
                },
                (byte)TexTrackType.HSD_A_T_TEV0_B);

            r.Add(g);
            r.Add(b);

            return new HSD_MatAnimJoint()
            {
                MaterialAnimation = new HSD_MatAnim()
                {
                    TextureAnimation = new HSD_TexAnim()
                    {
                        AnimationObject = new HSD_AOBJ()
                        {
                            EndFrame = 600,
                            FObjDesc = r,
                        }
                    }
                }
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static HSD_DOBJ GenerateQuadDobj(POBJ_Generator gen, float w, float h, float z)
        {
            HSD_DOBJ dobj = new()
            {
                Mobj = new HSD_MOBJ()
                {
                    RenderFlags = RENDER_MODE.CONSTANT |
                                RENDER_MODE.TEX0 |
                                RENDER_MODE.NO_ZUPDATE |
                                RENDER_MODE.XLU,
                    Material = new HSD_Material()
                    {
                        AMB_A = 255,
                        AMB_R = 128,
                        AMB_G = 128,
                        AMB_B = 128,
                        DIF_A = 255,
                        DIF_R = 255,
                        DIF_G = 255,
                        DIF_B = 255,
                        SPC_A = 255,
                        SPC_R = 255,
                        SPC_G = 255,
                        SPC_B = 255,
                        Shininess = 50,
                        Alpha = 0.999f
                    }
                }
            };

            List<GX_Vertex> verts = new()
                {
                    // First Triangle (Bottom-right -> Top-right -> Bottom-left)
                    new GX_Vertex() { POS = new GXVector3(-w, -h, z), TEX0 = new GXVector2(0, 1) }, // Bottom-left
                    new GX_Vertex() { POS = new GXVector3(w, h, z), TEX0 = new GXVector2(1, 0) },   // Top-right
                    new GX_Vertex() { POS = new GXVector3(w, -h, z), TEX0 = new GXVector2(1, 1) },  // Bottom-right

                    // Second Triangle (Top-right -> Top-left -> Bottom-left)
                    new GX_Vertex() { POS = new GXVector3(-w, -h, z), TEX0 = new GXVector2(0, 1) }, // Bottom-left
                    new GX_Vertex() { POS = new GXVector3(-w, h, z), TEX0 = new GXVector2(0, 0) },  // Top-left
                    new GX_Vertex() { POS = new GXVector3(w, h, z), TEX0 = new GXVector2(1, 0) },   // Top-right
                };

            dobj.Pobj = gen.CreatePOBJsFromTriangleList(verts, new GXAttribName[]
            {
                    GXAttribName.GX_VA_POS,
                    GXAttribName.GX_VA_TEX0,
            }, null);

            dobj.Pobj.Flags = POBJ_FLAG.CULLFRONT;

            return dobj;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static HSD_JOBJ GenerateIconModel(MexCharacterSelectIcon icon, MexWorkspace ws)
        {
            var fighter = ws.Project.GetFighterByExternalID(icon.Fighter);
            MexImage? iconImage = fighter?.Assets.CSSIconAsset.GetTexFile(ws);

            iconImage ??= ws.Project.ReservedAssets.CSSNullAsset.GetTexFile(ws);
            MexImage? background = ws.Project.ReservedAssets.CSSBackAsset.GetTexFile(ws);

            var source = new HSD_JOBJ()
            {
                TX = icon.X,
                TY = icon.Y,
                TZ = icon.Z,
                SX = icon.ScaleX,
                SY = icon.ScaleY,
                SZ = 1,
            };

            POBJ_Generator gen = new();

            // background
            {
                float w = 3.265f;
                float h = 3.361f;
                float z = 0.2f;

                var dobj = GenerateQuadDobj(gen, w, h, z);
                if (background != null)
                {
                    dobj.Mobj.Textures = background.ToTObj();
                    dobj.Mobj.Textures.AlphaOperation = ALPHAMAP.NONE;
                    dobj.Mobj.Textures.TEV = new HSD_TOBJ_TEV()
                    {
                        color_op = TevColorOp.GX_TEV_ADD,
                        alpha_op = TevAlphaOp.GX_TEV_ADD,
                        color_bias = TevBias.GX_TB_ZERO,
                        alpha_bias = TevBias.GX_TB_ZERO,
                        color_scale = TevScale.GX_CS_SCALE_1,
                        alpha_scale = TevScale.GX_CS_SCALE_1,
                        color_clamp = true,
                        alpha_clamp = true,
                        color_a_in = TOBJ_TEV_CC.TEX0_RGB,
                        color_b_in = TOBJ_TEV_CC.KONST_RGB,
                        color_c_in = TOBJ_TEV_CC.GX_CC_TEXC,
                        color_d_in = TOBJ_TEV_CC.GX_CC_ZERO,
                        alpha_a_in = TOBJ_TEV_CA.GX_CC_ZERO,
                        alpha_b_in = TOBJ_TEV_CA.GX_CC_ZERO,
                        alpha_c_in = TOBJ_TEV_CA.GX_CC_ZERO,
                        alpha_d_in = TOBJ_TEV_CA.GX_CC_ZERO,
                        active = TOBJ_TEVREG_ACTIVE.KONST_R |
                                TOBJ_TEVREG_ACTIVE.KONST_G |
                                TOBJ_TEVREG_ACTIVE.KONST_B |
                                TOBJ_TEVREG_ACTIVE.TEV0_R |
                                TOBJ_TEVREG_ACTIVE.TEV0_G |
                                TOBJ_TEVREG_ACTIVE.TEV0_B |
                                TOBJ_TEVREG_ACTIVE.COLOR_TEV,
                        constant = System.Drawing.Color.FromArgb(204, 204, 229),
                        tev0 = System.Drawing.Color.FromArgb(0, 25, 51),
                    };
                }
                
                source.Dobj = dobj;
            }
            // foreground
            {
                float w = 3.4f;
                float h = 3.5f;
                float z = 0.0f;

                var dobj = GenerateQuadDobj(gen, w, h, z);
                if (iconImage != null)
                {
                    var tobj = iconImage.ToTObj();
                    tobj.ColorOperation = COLORMAP.BLEND;
                    tobj.AlphaOperation = ALPHAMAP.BLEND;
                    dobj.Mobj.Textures = tobj;
                }
                source.Dobj.Add(dobj);
            }

            gen.SaveChanges();

            return source;
        }
    }
}
