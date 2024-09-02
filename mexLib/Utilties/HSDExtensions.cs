using HSDRaw;
using HSDRaw.Common.Animation;
using HSDRaw.Common;
using HSDRaw.Tools;

namespace mexLib
{
    public static class HSDExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="symbol"></param>
        /// <param name="data"></param>
        public static void ExportFile(string filepath, string symbol, HSDAccessor data)
        {
            var file = new HSDRawFile();
            file.Roots.Add(new HSDRootNode()
            {
                Data = data,
                Name = symbol
            });
            file.Save(filepath);
        }
        /// <summary>
        /// Adds a new symbol to file if it doesn't exist and creates it if it does
        /// </summary>
        /// <param name="file"></param>
        /// <param name="symbol"></param>
        /// <param name="data"></param>
        public static void CreateUpdateSymbol(this HSDRawFile file, string symbol, HSDAccessor data)
        {
            if (file == null)
                return;

            if (file[symbol] != null)
            {
                file[symbol].Data = data;
            }
            else
            {
                file.Roots.Add(new HSDRootNode()
                {
                    Name = symbol,
                    Data = data,
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="anim"></param>
        /// <param name="icons"></param>
        /// <param name="keys"></param>
        /// <returns>self</returns>
        public static HSD_TexAnim GenerateTextureAnimation(this HSD_TexAnim anim, List<HSD_TOBJ> icons, List<FOBJKey>? keys)
        {
            if (keys == null)
            {
                keys = Enumerable.Range(0, icons.Count).Select(e => new FOBJKey()
                {
                    Frame = e,
                    Value = e,
                    InterpolationType = GXInterpolationType.HSD_A_OP_CON
                }).ToList();
            }

            // generate texture animation
            anim.AnimationObject = new HSD_AOBJ()
            {
                EndFrame = keys.Max(e => e.Frame),
                FObjDesc = new FOBJ_Player((int)TexTrackType.HSD_A_T_TIMG, keys.OrderBy(e => e.Frame)).ToFobjDesc()
            };

            // add a track for palettes if applicable
            if (icons.Any(e=>e.ImageData != null && GXImageConverter.IsPalettedFormat(e.ImageData.Format)))
            {
                anim.AnimationObject.FObjDesc.Next = new FOBJ_Player((int)TexTrackType.HSD_A_T_TCLT, keys.OrderBy(e => e.Frame)).ToFobjDesc();
            }

            // set tobjs
            anim.FromTOBJs(icons, false);

            return anim;
        }
    }
}
