using mexLib.MexScubber;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mexLib
{
    public class MexFighterCostumes
    {
        [Category("Parameters"), DisplayName("Red Costume Index"), Description("")]
        public byte RedCostumeIndex { get; set; }

        [Category("Parameters"), DisplayName("Blue Costume Index"), Description("")]
        public byte BlueCostumeIndex { get; set; }

        [Category("Parameters"), DisplayName("Green Costume Index"), Description("")]
        public byte GreenCostumeIndex { get; set; }

        [Browsable(false)]
        public ObservableCollection<MexFighterCostume> Costumes { get; set; } = new ObservableCollection<MexFighterCostume>();

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
                var costume = new MexFighterCostume()
                {
                    FileName = dol.GetStruct<string>(costumePointer + 0x00, i, 0x0C),
                    ModelSymbol = dol.GetStruct<string>(costumePointer + 0x04, i, 0x0C),
                    MaterialSymbol = dol.GetStruct<string>(costumePointer + 0x08, i, 0x0C),
                    VisibilityIndex = (int)i,
                };

                if (costumePointerKirby != 0)
                {
                    costume.KirbyFileName = dol.GetStruct<string>(costumePointerKirby + 0x00, i, 0x0C);
                    costume.KirbyModelSymbol = dol.GetStruct<string>(costumePointerKirby + 0x04, i, 0x0C);
                    costume.KirbyMaterialSymbol = dol.GetStruct<string>(costumePointerKirby + 0x08, i, 0x0C);
                }

                Costumes.Add(costume);
            }
        }
    }

    public class MexFighterCostume
    {
        [Category("Player"), DisplayName("Filename"), Description("Filename of the costume")]
        public string FileName { get; set; } = "";

        [Category("Player"), DisplayName("Model Symbol"), Description("Joint symbol inside the costume dat.")]
        public string ModelSymbol { get; set; } = "";

        [Category("Player"), DisplayName("Material Symbol"), Description("Material symbol inside the costume dat. Leave blank if there isn't one.")]
        public string MaterialSymbol { get; set; } = "";

        [Category("Player"), DisplayName("Visibility Table Index"), Description("Table to use for visibility lookup. Unless specified otherwise, use 0.")]
        public int VisibilityIndex { get; set; } = 0;


        [Category("Kirby"), DisplayName("Filename"), Description("Filename of the costume. Leave Empty if kirby does not use costume.")]
        public string KirbyFileName { get; set; } = "";

        [Category("Kirby"), DisplayName("Model Symbol"), Description("Joint symbol inside the costume dat. Leave Empty if kirby does not use costume.")]
        public string KirbyModelSymbol { get; set; } = "";

        [Category("Kirby"), DisplayName("Material Symbol"), Description("Material symbol inside the costume dat. Leave blank if there isn't one. Leave Empty if kirby does not use costume.")]
        public string KirbyMaterialSymbol { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public string GetIconPath(MexWorkspace workspace)
        {
            return workspace.GetAssetPath($"icons\\{Path.GetFileNameWithoutExtension(FileName)}.tex");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public string GetCSPPath(MexWorkspace workspace)
        {
            return workspace.GetAssetPath($"csp\\{Path.GetFileNameWithoutExtension(FileName)}.tex");
        }
    }
}
