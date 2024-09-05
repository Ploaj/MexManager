using HSDRaw.Common.Animation;
using HSDRaw.Common;
using HSDRaw.MEX;
using HSDRaw.Tools;
using HSDRaw;

namespace mexLib.Generators
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
            var data = ws.FileManager.Get(path);

            if (data == Array.Empty<byte>())
                return false;

            HSDRawFile ifallFile = new(path);

            ifallFile.CreateUpdateSymbol("Eblm_matanim_joint", GenerateEmblems(ws));
            ifallFile.CreateUpdateSymbol("Stc_icns", Generate_Stc_icns(ws));

            using MemoryStream stream = new MemoryStream();
            ifallFile.Save(stream);
            ws.FileManager.Set(path, stream.ToArray());
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
            List<FOBJKey> keys = new();
            List<HSD_TOBJ> icons = new();

            // gather reserved icons
            int icon_index = 0;
            foreach (var s in ws.Project.Series)
            {
                var iconTex = ws.FileManager.Get(ws.GetAssetPath($"series//{s.Icon.Replace(".png", ".tex")}"));

                if (iconTex != null)
                {
                    keys.Add(new FOBJKey()
                    {
                        Frame = icon_index,
                        Value = icons.Count,
                        InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                    });
                    icons.Add(MexImage.FromByteArray(iconTex).ToTObj());
                }
                icon_index++;
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
            List<FOBJKey> keys = new();
            List<HSD_TOBJ> icons = new();

            // TODO: use proper filenames for stock icons

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
                    if (ws.FileManager.Exists(textureAsset))
                    {
                        keys.Add(new FOBJKey()
                        {
                            Frame = reservedCount + internalId + stride * costume_index,
                            Value = icons.Count,
                            InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                        });
                        icons.Add(MexImage.FromByteArray(ws.FileManager.Get(textureAsset)).ToTObj());
                    }
                    else
                    {
                        keys.Add(new FOBJKey()
                        {
                            Frame = reservedCount + internalId + stride * costume_index,
                            Value = 0,
                            InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                        });
                        icons.Add(new MexImage(8, 8, HSDRaw.GX.GXTexFmt.I4, HSDRaw.GX.GXTlutFmt.IA8).ToTObj());
                    }
                    costume_index++;
                }
            }

            var stock = new MEX_Stock()
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
                CustomStockEntries = new HSDArrayAccessor<MEX_StockEgg>()
                {
                }
            };

            stock.Optimize();

            // generate stock icon node
            return stock;
        }
    }
}
