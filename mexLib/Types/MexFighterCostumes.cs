using HSDRaw;
using HSDRaw.MEX;
using mexLib.MexScubber;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace mexLib.Types
{
    public partial class MexFighter
    {
        [Browsable(false)]
        public MexFighterCostumes Costumes { get; set; } = new MexFighterCostumes();

        public class MexFighterCostumes
        {
            [Category("Parameters"), DisplayName("Red Costume Index"), Description("")]
            public byte RedCostumeIndex { get; set; }

            [Category("Parameters"), DisplayName("Blue Costume Index"), Description("")]
            public byte BlueCostumeIndex { get; set; }

            [Category("Parameters"), DisplayName("Green Costume Index"), Description("")]
            public byte GreenCostumeIndex { get; set; }

            [Browsable(false)]
            public ObservableCollection<MexCostume> Costumes { get; set; } = new ObservableCollection<MexCostume>();

            public bool HasKirbyCostumes
            {
                get
                {
                    return Costumes.Any(e => !string.IsNullOrEmpty(e.KirbyFileName));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public MEX_CostumeFileSymbolTable ToMxDt()
            {
                return new MEX_CostumeFileSymbolTable()
                {
                    CostumeSymbols = new HSDArrayAccessor<MEX_CostumeFileSymbol>()
                    {
                        Array = Costumes.Select(e => new MEX_CostumeFileSymbol()
                        {
                            FileName = e.File.FileName,
                            JointSymbol = e.File.JointSymbol,
                            MatAnimSymbol = e.File.MaterialSymbol,
                            VisibilityLookupIndex = e.VisibilityIndex,
                        }).ToArray()
                    }
                };
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static string ColorNameFromFileName(string fileName)
            {
                var code = Path.GetFileNameWithoutExtension(fileName);

                if (code.Length != 6)
                    return code;

                return code[4..] switch
                {
                    "Nr" => "Normal",
                    "Re" => "Red",
                    "Bu" => "Blue",
                    "Gr" => "Green",
                    "Ye" => "Yellow",
                    "Or" => "Orange",
                    "La" => "Purple",
                    "Gy" => "Gray",
                    "Aq" => "Aqua",
                    "Pi" => "Pink",
                    "Wh" => "White",
                    "Bk" => "Black",
                    _ => code,
                };
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dol"></param>
            /// <param name="index"></param>
            public void FromDOL(MexDOL dol, uint index)
            {
                // get external id
                var exid = (uint)MexFighterIDConverter.ToExternalID((int)index, 0x21);

                // css
                int costumeCount = dol.GetStruct<byte>(0x803C0EC0 + 0x4, index, 8);
                RedCostumeIndex = dol.GetStruct<byte>(0x803d51a0 + 0x1, exid, 4);
                BlueCostumeIndex = dol.GetStruct<byte>(0x803d51a0 + 0x2, exid, 4);
                GreenCostumeIndex = dol.GetStruct<byte>(0x803d51a0 + 0x3, exid, 4);

                // costumes
                var costumePointer = dol.GetStruct<uint>(0x803C2360, index);
                var costumePointerKirby = dol.GetStruct<uint>(0x803CB3E8, index);
                for (uint i = 0; i < costumeCount; i++)
                {
                    var costume = new MexCostume()
                    {
                        File = new MexCostumeFile()
                        {
                            FileName = dol.GetStruct<string>(costumePointer + 0x00, i, 0x0C),
                            JointSymbol = dol.GetStruct<string>(costumePointer + 0x04, i, 0x0C),
                            MaterialSymbol = dol.GetStruct<string>(costumePointer + 0x08, i, 0x0C),
                        },
                        VisibilityIndex = (int)i,
                    };

                    costume.Name = ColorNameFromFileName(costume.FileName);

                    if (costumePointerKirby != 0)
                    {
                        costume.KirbyFile = new MexCostumeFile()
                        {
                            FileName = dol.GetStruct<string>(costumePointerKirby + 0x00, i, 0x0C),
                            JointSymbol = dol.GetStruct<string>(costumePointerKirby + 0x04, i, 0x0C),
                            MaterialSymbol = dol.GetStruct<string>(costumePointerKirby + 0x08, i, 0x0C),
                        };
                    }

                    Costumes.Add(costume);
                }
            }
        }
    }
    
}
