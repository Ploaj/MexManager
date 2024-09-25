using HSDRaw.Melee;
using System.ComponentModel;

namespace mexLib.Types
{
    public partial class MexFighter
    {
        public class MexFighterBoneDefinitions
        {
            [Browsable(false)]
            public SBM_BoneLookupTable Lookup { get; set; } = new SBM_BoneLookupTable();

            public BindingList<MexFighterBoneExt> Ext { get; set; } = new BindingList<MexFighterBoneExt>();
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class MexFighterBoneExt
        {
            public byte X00 { get; set; }
            public byte X01 { get; set; }
            public byte X02 { get; set; }
            public byte X03 { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plco"></param>
        /// <param name="index"></param>
        public void LoadFromPlCo(SBM_ftLoadCommonData plco, uint index)
        {
            BoneDefinitions.Lookup = plco.BoneTables[(int)index];

            BoneDefinitions.Ext.Clear();
            if (plco.FighterTable[(int)index] != null)
                foreach (var e in plco.FighterTable[(int)index].Entries)
                    BoneDefinitions.Ext.Add(new MexFighterBoneExt()
                    {
                        X00 = e.Value1,
                        X01 = e.Value2,
                        X02 = e.Value3,
                        X03 = e.Value4,
                    });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plco"></param>
        /// <param name="index"></param>
        public void SetPlCo(SBM_ftLoadCommonData plco, int index)
        {
            plco.BoneTables.Set(index, BoneDefinitions.Lookup);
            if (BoneDefinitions.Ext.Count == 0)
            {
                plco.FighterTable.Set(index, null);
            }
            else
            {
                SBM_PlCoFighterBoneExt tbl = new()
                {
                    Entries = BoneDefinitions.Ext.Select(e => new SBM_PlCoFighterBoneExtEntry()
                    {
                        Value1 = e.X00,
                        Value2 = e.X01,
                        Value3 = e.X02,
                        Value4 = e.X03,
                    }).ToArray()
                };
                plco.FighterTable.Set(index, tbl);
            }

        }
    }
}
