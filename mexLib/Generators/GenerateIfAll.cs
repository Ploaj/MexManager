using HSDRaw.Common.Animation;
using HSDRaw.Common;
using HSDRaw.MEX;
using HSDRaw.Tools;
using HSDRaw;
using System;

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

            var emblems = GenerateEmblems(ws);
            var stock_icons = Generate_Stc_icns(ws);

            ifallFile.CreateUpdateSymbol("Eblm_matanim_joint", emblems);
            ifallFile.CreateUpdateSymbol("Stc_icns", stock_icons);

            using MemoryStream stream = new ();
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
                var iconTex = s.IconAsset.GetTexFile(ws);

                if (iconTex != null)
                {
                    keys.Add(new FOBJKey()
                    {
                        Frame = icon_index,
                        Value = icons.Count,
                        InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                    });
                    icons.Add(iconTex.ToTObj());
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

            // gather reserved icons
            for (int i = 0; i < ws.Project.ReservedAssets.IconsAssets.Length; i++)
            {
                keys.Add(new FOBJKey()
                {
                    Frame = i,
                    Value = icons.Count,
                    InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                });
                if (ws.Project.ReservedAssets.IconsAssets[i].GetTexFile(ws) is MexImage tex)
                    icons.Add(tex.ToTObj());
            }

            int reservedCount = icons.Count;
            int stride = ws.Project.Fighters.Count;

            // gather costume stock icons
            for (int internalId = 0; internalId < ws.Project.Fighters.Count; internalId++)
            {
                var f = ws.Project.Fighters[internalId];
                int costume_index = 0;
                foreach (var c in f.Costumes)
                {
                    var textureAsset = c.IconAsset.GetTexFile(ws);
                    if (textureAsset != null)
                    {
                        keys.Add(new FOBJKey()
                        {
                            Frame = reservedCount + internalId + stride * costume_index,
                            Value = icons.Count,
                            InterpolationType = GXInterpolationType.HSD_A_OP_CON,
                        });
                        icons.Add(textureAsset.ToTObj());
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
