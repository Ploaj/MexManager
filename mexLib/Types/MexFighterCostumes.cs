﻿using HSDRaw;
using HSDRaw.MEX;
using mexLib.Utilties;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace mexLib.Types
{
    public partial class MexFighter
    {
        [Category("0 - General"), DisplayName("Red Costume Index"), Description("")]
        public byte RedCostumeIndex { get; set; }

        [Category("0 - General"), DisplayName("Blue Costume Index"), Description("")]
        public byte BlueCostumeIndex { get; set; }

        [Category("0 - General"), DisplayName("Green Costume Index"), Description("")]
        public byte GreenCostumeIndex { get; set; }

        [Browsable(false)]
        public ObservableCollection<MexCostume> Costumes { get; set; } = new ObservableCollection<MexCostume>();

        [Browsable(false)]
        public ObservableCollection<MexCostumeFile> KirbyCostumes { get; set; } = new ObservableCollection<MexCostumeFile>();

        public bool HasKirbyCostumes { get => KirbyCostumes.Count > 0; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MEX_CostumeFileSymbolTable CostumesToMxDt()
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
                        VisibilityLookupIndex = e.File.VisibilityIndex,
                    }).ToArray()
                }
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tbl"></param>
        public void CostumesFromMxDt(MEX_CostumeFileSymbolTable tbl)
        {
            Costumes.Clear();
            foreach (MEX_CostumeFileSymbol? c in tbl.CostumeSymbols.Array)
            {
                Costumes.Add(new MexCostume()
                {
                    File = new MexCostumeVisibilityFile()
                    {
                        FileName = c.FileName,
                        JointSymbol = c.JointSymbol,
                        MaterialSymbol = c.MatAnimSymbol,
                        VisibilityIndex = c.VisibilityLookupIndex,
                    }
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ColorNameFromFileName(string fileName)
        {
            string code = Path.GetFileNameWithoutExtension(fileName);

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
        public void CostumesFromDOL(MexDOL dol, uint index)
        {
            // get external id
            uint exid = (uint)MexFighterIDConverter.ToExternalID((int)index, 0x21);

            // css
            int costumeCount = dol.GetStruct<byte>(0x803C0EC0 + 0x4, index, 8);
            RedCostumeIndex = dol.GetStruct<byte>(0x803d51a0 + 0x1, exid, 4);
            BlueCostumeIndex = dol.GetStruct<byte>(0x803d51a0 + 0x2, exid, 4);
            GreenCostumeIndex = dol.GetStruct<byte>(0x803d51a0 + 0x3, exid, 4);

            // costumes
            uint costumePointer = dol.GetStruct<uint>(0x803C2360, index);
            for (uint i = 0; i < costumeCount; i++)
            {
                MexCostume costume = new()
                {
                    File = new MexCostumeVisibilityFile()
                    {
                        FileName = dol.GetStruct<string>(costumePointer + 0x00, i, 0x0C),
                        JointSymbol = dol.GetStruct<string>(costumePointer + 0x04, i, 0x0C),
                        MaterialSymbol = dol.GetStruct<string>(costumePointer + 0x08, i, 0x0C),
                        VisibilityIndex = (int)i,
                    },
                };

                costume.Name = ColorNameFromFileName(costume.File.FileName);

                Costumes.Add(costume);
            }

            uint costumePointerKirby = dol.GetStruct<uint>(0x803CB3E8, index);
            if (costumePointerKirby != 0)
            {
                for (uint i = 0; i < 6; i++)
                {
                    KirbyCostumes.Add(new()
                    {
                        FileName = dol.GetStruct<string>(costumePointerKirby + 0x00, i, 0x0C),
                        JointSymbol = dol.GetStruct<string>(costumePointerKirby + 0x04, i, 0x0C),
                        MaterialSymbol = dol.GetStruct<string>(costumePointerKirby + 0x08, i, 0x0C),
                    });
                }
            }
        }
    }
}
