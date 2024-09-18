using HSDRaw;
using HSDRaw.Common;
using mexLib.Types;
using HSDRaw.Tools;
using HSDRaw.GX;
using HSDRaw.Common.Animation;
using mexLib.AssetTypes;

namespace mexLib.Generators
{
    public class GenerateGmRst
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public static bool Compile(MexWorkspace workspace)
        {
            var path = workspace.GetFilePath("GmRst.usd");
            var data = workspace.FileManager.Get(path);

            if (data == Array.Empty<byte>())
                return false;

            HSDRawFile file = new(path);

            GenerateEmblems(workspace, file);
            GenerateBanners(workspace, file);

            using MemoryStream stream = new();
            file.Save(stream);
            workspace.FileManager.Set(path, stream.ToArray());

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        private static void GenerateBanners(MexWorkspace workspace, HSDRawFile file)
        {
            // pnlsce SOBJ -> entry 0 -> material animation
            var pnlsce = file["pnlsce"];
            if (pnlsce.Data is not HSD_SOBJ sobj)
                return;

            if (sobj.JOBJDescs.Length < 1)
                return;

            List<FOBJKey> largekeys = new List<FOBJKey>();
            List<HSD_TOBJ> largetobjs = new List<HSD_TOBJ>();
            List<FOBJKey> smallkeys = new List<FOBJKey>();
            List<HSD_TOBJ> smalltobjs = new List<HSD_TOBJ>();
            // loop through fighters
            for (int internalId = 0; internalId < workspace.Project.Fighters.Count; internalId++)
            {
                // get fighter  external id
                int externalId = MexFighterIDConverter.ToExternalID(internalId, workspace.Project.Fighters.Count);

                // search and set asset for fighter
                var fighter = workspace.Project.Fighters[internalId];

                // sheik hack
                if (externalId == 19)
                    externalId = 25;
                else if (externalId > 19 && externalId <= 25)
                    externalId -= 1;

                // get larget name banner
                if (fighter.Assets.ResultBannerBigAsset.GetTexFile(workspace) is MexImage largeBanner)
                {
                    largekeys.Add(new FOBJKey()
                    {
                        Frame = externalId,
                        Value = largetobjs.Count,
                        InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                    });
                    largetobjs.Add(largeBanner.ToTObj());
                }

                // get small name banner
                if (fighter.Assets.ResultBannerSmallAsset.GetTexFile(workspace) is MexImage smallBanner)
                {
                    smallkeys.Add(new FOBJKey()
                    {
                        Frame = externalId,
                        Value = smalltobjs.Count,
                        InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                    });
                    smalltobjs.Add(smallBanner.ToTObj());
                }
            }

            // edit the material animations
            var matanim = sobj.JOBJDescs[0].MaterialAnimations[0].TreeList;

            // large banner
            {
                // add additional assets
                // additional assets red 181, blue 182, green 183, nocontest 184+200
                MexTextureAsset[] reserved =
                {
                    workspace.Project.ReservedAssets.RstRedTeamAsset,
                    workspace.Project.ReservedAssets.RstBlueTeamAsset,
                    workspace.Project.ReservedAssets.RstGreenTeamAsset,
                    workspace.Project.ReservedAssets.RstNoContestAsset,
                };
                for (int i = 0; i < 4; i++)
                {
                    if (reserved[i].GetTexFile(workspace) is MexImage asset)
                    {
                        largekeys.Add(new FOBJKey()
                        {
                            Frame = 181 + i,
                            Value = largetobjs.Count,
                            InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                        });
                        largetobjs.Add(asset.ToTObj());
                    }
                }

                // joint 10 object 1
                var joint = matanim[10].MaterialAnimation.Next;

                // set tobjs
                joint.TextureAnimation.FromTOBJs(largetobjs, false);

                // edit timg track
                var timg = joint.TextureAnimation.AnimationObject.FObjDesc.List.Find(e => e.TexTrackType == TexTrackType.HSD_A_T_TIMG);
                if (timg != null)
                {
                    timg.SetKeys(largekeys.OrderBy(e => e.Frame).ToList(), timg.TrackType);
                }
            }

            // add small name banners
            {
                // blank at 180+200
                smallkeys.Add(new FOBJKey()
                {
                    Frame = 180,
                    Value = smalltobjs.Count,
                    InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                });
                smallkeys.Add(new FOBJKey()
                {
                    Frame = 200,
                    Value = smalltobjs.Count,
                    InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                });
                smalltobjs.Add(new MexImage(8, 8, GXTexFmt.I4, GXTlutFmt.IA8).ToTObj());

                // joint 33 object 0
                // joint 41 object 0
                // joint 49 object 0
                // joint 57 object 0
                int[] indices = { 33, 41, 49, 57 };
                foreach(var i in indices)
                {
                    // get material anim
                    var joint = matanim[i].MaterialAnimation;

                    // set tobjs
                    joint.TextureAnimation.FromTOBJs(smalltobjs, false);

                    // edit timg track
                    var timg = joint.TextureAnimation.AnimationObject.FObjDesc.List.Find(e => e.TexTrackType == TexTrackType.HSD_A_T_TIMG);
                    if (timg != null)
                    {
                        timg.SetKeys(smallkeys.OrderBy(e => e.Frame).ToList(), timg.TrackType);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="series"></param>
        /// <returns></returns>
        private static HSD_JOBJ GenericIconModel(MexWorkspace workspace, MexSeries series)
        {
            // create joint
            var joint = new HSD_JOBJ()
            {
                SX = 3, SY = 3, SZ = 3,
                TZ = 67.4f,
            };

            var obj = series.ModelAsset.GetOBJFile(workspace);
            if (obj == null)
                return joint;

            // create dobj
            joint.Dobj = new HSD_DOBJ()
            {
                Mobj = new HSD_MOBJ()
                {
                    RenderFlags = RENDER_MODE.CONSTANT | RENDER_MODE.NO_ZUPDATE | RENDER_MODE.XLU,
                    Material = new HSD_Material()
                    {
                        // Diffuse 128, 128, 230
                        DIF_A = 255,
                        DIF_R = 128,
                        DIF_G = 128,
                        DIF_B = 230,

                        // Specular 255, 255, 255
                        SPC_A = 255,
                        SPC_R = 255,
                        SPC_G = 255,
                        SPC_B = 255,

                        // Ambient 128, 128, 128
                        AMB_A = 255,
                        AMB_R = 128,
                        AMB_G = 128,
                        AMB_B = 128,

                        // ALPHA = 0.6, Shine = 50
                        Alpha = 0.6f,
                        Shininess = 50,
                    }
                }
            };

            // TODO: pobj only has pos
            POBJ_Generator gen = new ();

            var verts = obj.Faces.SelectMany(e =>
            {
                return e.Vertices.Select(f => new GX_Vertex()
                {
                    POS = new GXVector3(
                        obj.Vertices[f.VertexIndex].X,
                        obj.Vertices[f.VertexIndex].Y,
                        obj.Vertices[f.VertexIndex].Z),
                });
            }).ToList();

            joint.Dobj.Pobj = gen.CreatePOBJsFromTriangleList(
                verts,
                new GXAttribName[]
                {
                    GXAttribName.GX_VA_POS,
                    GXAttribName.GX_VA_NULL,
                },
                new List<HSD_Envelope>());

            gen.SaveChanges();

            return joint;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="file"></param>
        private static void GenerateEmblems(MexWorkspace workspace, HSDRawFile file)
        {
            // generate 3d emblems + animation
            // flmsce SOBJ -> entry 0
            var flmsce = file["flmsce"];
            if (flmsce.Data is not HSD_SOBJ sobj)
                return;

            if (sobj.JOBJDescs.Length < 1)
                return;

            // icons joints are all children of 5
            var group = sobj.JOBJDescs[0];
            var jointBase = group.RootJoint.TreeList[5];
            var animjointBase = group.JointAnimations[0].TreeList[5];
            var matanim_jointBase = group.MaterialAnimations[0].TreeList[5];

            // clear old children
            jointBase.Child = null;

            // generate icons models
            foreach (var series in workspace.Project.Series)
            {
                var joint = GenericIconModel(workspace, series);
                jointBase.AddChild(joint);
            }
            jointBase.UpdateFlags(); // recalulate flags just in case

            // Joint Animation
            if (animjointBase.Child != null)
            {
                var source = animjointBase.Child;
                source.Next = null;
                animjointBase.Child = null;
                for (int i = 0; i < workspace.Project.Series.Count; i++)
                    animjointBase.AddChild(HSDAccessor.DeepClone<HSD_AnimJoint>(source));
            }
            // Material Animation
            if (matanim_jointBase.Child != null)
            {
                var source = matanim_jointBase.Child;
                source.Next = null;
                matanim_jointBase.Child = null;
                for (int i = 0; i < workspace.Project.Series.Count; i++)
                    matanim_jointBase.AddChild(HSDAccessor.DeepClone<HSD_MatAnimJoint>(source));
            }
        }
    }
}
