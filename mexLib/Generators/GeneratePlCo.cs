using HSDRaw;
using HSDRaw.Melee;

namespace mexLib.Generators
{
    public static class GeneratePlCo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        public static void Compile(MexWorkspace ws)
        {
            //get plco data
            HSDRawFile plcoFile = new(ws.GetFilePath("PlCo.dat"));
            SBM_ftLoadCommonData? plCo = plcoFile["ftLoadCommonData"].Data as SBM_ftLoadCommonData;

            if (plCo == null)
                return;

            // dump fighter bone table data
            for (int internalId = 0; internalId < ws.Project.Fighters.Count; internalId++)
            {
                ws.Project.Fighters[internalId].SetPlCo(plCo, internalId);
            }

            //save plyco
            GeneratePlCoDummy(ws, plCo);
            plcoFile.Save(ws.GetFilePath("PlCo.dat"));
        }
        /// <summary>
        /// 
        /// </summary>
        private static void GeneratePlCoDummy(MexWorkspace ws, SBM_ftLoadCommonData plCo)
        {
            // 
            byte[] tb1 = new byte[54];
            byte[] tb2 = new byte[54];
            tb2[53] = 255;
            for (byte i = 0; i < 53; i++)
            {
                tb1[i] = i;
                tb2[i] = i;
            }
            SBM_BoneLookupTable commonBoneTable = new()
            {
                BoneCount = 53,
            };
            commonBoneTable._s.SetReferenceStruct(0x00, new HSDStruct(tb1));
            commonBoneTable._s.SetReferenceStruct(0x04, new HSDStruct(tb2));
            plCo.BoneTables.Set(ws.Project.Fighters.Count, commonBoneTable);
            plCo.FighterTable.Set(ws.Project.Fighters.Count, new SBM_PlCoFighterBoneExt()
            {
                Entries = new SBM_PlCoFighterBoneExtEntry[] {
                    new ()
                    {
                        Value1 = 52,
                        Value2 = 4
                    }
                }
            });
        }
    }
}
