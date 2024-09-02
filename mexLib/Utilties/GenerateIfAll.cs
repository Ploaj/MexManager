using HSDRaw.Common.Animation;
using HSDRaw.Common;
using HSDRaw.MEX;
using HSDRaw.Tools;
using HSDRaw;

namespace mexLib.Utilties
{
    public static class GenerateIfAll
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public static bool Compile(MexWorkspace ws)
        {
            var path = ws.GetFilePath("IfAll.usd");

            if (!File.Exists(path))
                return false;

            HSDRawFile ifallFile = new (path);

            ifallFile.CreateUpdateSymbol("Eblm_matanim_joint", GenerateEmblems(ws));
            ifallFile.CreateUpdateSymbol("Stc_icns", Generate_Stc_icns(ws));

            ifallFile.Save(path);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        private static HSD_MatAnimJoint GenerateEmblems(MexWorkspace ws)
        {
            // stick icons
            List<FOBJKey> keys = new ();
            List<HSD_TOBJ> icons = new ();

            // gather reserved icons
            foreach (var f in Directory.GetFiles(ws.GetAssetPath("series\\")))
            {
                if (!Path.GetExtension(f).ToLower().Equals(".tex"))
                    continue;

                var fileName = Path.GetFileNameWithoutExtension(f);

                if (int.TryParse(fileName, out int index))
                {
                    keys.Add(new FOBJKey()
                    {
                        Frame = index,
                        Value = icons.Count,
                        InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                    });
                    icons.Add(new MexImage(f).ToTObj());
                }
            }

            // generate texture animation
            return new HSD_MatAnimJoint()
            {
                MaterialAnimation = new HSD_MatAnim()
                {
                    TextureAnimation = new HSD_TexAnim().GenerateTextureAnimation(icons, keys)
                }
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        private static MEX_Stock Generate_Stc_icns(MexWorkspace ws)
        {
            // stick icons
            List<FOBJKey> keys = new ();
            List<HSD_TOBJ> icons = new ();

            // gather reserved icons
            foreach (var f in Directory.GetFiles(ws.GetAssetPath("icons\\")))
            {
                if (!Path.GetExtension(f).ToLower().Equals(".tex"))
                    continue;

                var fileName = Path.GetFileNameWithoutExtension(f);

                if (int.TryParse(fileName, out int index))
                {
                    keys.Add(new FOBJKey()
                    {
                        Frame = index,
                        Value = icons.Count,
                        InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                    });
                    icons.Add(new MexImage(f).ToTObj());
                }
            }

            int reservedCount = icons.Count;
            int stride = ws.Project.Fighters.Count;

            // gather costume stock icons
            for (int internalId = 0; internalId < ws.Project.Fighters.Count; internalId++)
            {
                var f = ws.Project.Fighters[internalId];
                int costume_index = 0;
                foreach (var c in f.Costumes.Costumes)
                {
                    var textureAsset = c.GetIconPath(ws);
                    if (File.Exists(textureAsset))
                    {
                        keys.Add(new FOBJKey()
                        {
                            Frame = reservedCount + internalId + stride * costume_index,
                            Value = icons.Count,
                            InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                        });
                        icons.Add(new MexImage(textureAsset).ToTObj());
                    }
                    else
                    {
                        keys.Add(new FOBJKey()
                        {
                            Frame = reservedCount + internalId + stride * costume_index,
                            Value = 0,
                            InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                        });
                    }
                    costume_index++;
                }
            }

            // generate stock icon node
            return new MEX_Stock()
            {
                Reserved = (short)reservedCount,
                Stride = (short)stride,
                MatAnimJoint = new HSD_MatAnimJoint()
                {
                    MaterialAnimation = new HSD_MatAnim()
                    {
                        TextureAnimation = new HSD_TexAnim().GenerateTextureAnimation(icons, keys)
                    }
                },
                CustomStockLength = 0,
                CustomStockEntries = new HSDRaw.HSDArrayAccessor<MEX_StockEgg>()
                {
                }
            };
        }
    }
}
