using System.Text;

namespace mexLib.Utilties
{
    public static class SourceGenerator
    {

        private static readonly Dictionary<uint, uint> MoveLogicAddrToCount = new ()
        {
            { 0x803C7120, 12 },
            { 0x803C7788, 35 },
            { 0x803C72B8, 23 },
            { 0x803CB838, 46 },
            { 0x803C8368, 203 },
            { 0x803CEDC0, 23 },
            { 0x803C7E18, 21 },
            { 0x803CC060, 24 },
            { 0x803CC650, 36 },
            { 0x803CCCB8, 30 },
            { 0x803CD2D0, 26 },
            { 0x803CD838, 26 },
            { 0x803CDD78, 26 },
            { 0x803CE2D0, 18 },
            { 0x803CE6D0, 28 },
            { 0x803CFEF0, 32 },
            { 0x803D0B00, 20 },
            { 0x803D0628, 20 },
            { 0x803CF420, 32 },
            { 0x803CFA58, 18 },
            { 0x803D0FA0, 21 },
            { 0x803D1498, 10 },
            { 0x803D1848, 35 },
            { 0x803D1EA8, 26 },
            { 0x803D23E8, 40 },
            { 0x803D29F8, 23 },
            { 0x803D2E80, 32 },
            { 0x803D3A30, 50 },
            { 0x803D41F8, 49 },
            { 0x803D35E8, 24 },
            { 0x803D3998, 1 },
            { 0x803C7260, 2 },
            { 0x803CA04C, 4 },
            { 0x803D0868, 2 },
            { 0x803D38C8, 1 },
        };

        private enum AttackKind
        {
            ATKKIND_0,
            ATKKIND_NONE,
            ATKKIND_JAB1,
            ATKKIND_JAB2,
            ATKKIND_JAB3,
            ATKKIND_JAB4,
            ATKKIND_DASH,
            ATKKIND_FTILT,
            ATKKIND_UTILT,
            ATKKIND_DTILT,
            ATKKIND_FSMASH,
            ATKKIND_USMASH,
            ATKKIND_DSMASH,
            ATKKIND_NAIR,
            ATKKIND_FAIR,
            ATKKIND_BAIR,
            ATKKIND_UAIR,
            ATKKIND_DAIR,
            ATKKIND_SPECIALN,
            ATKKIND_SPECIALS,
            ATKKIND_SPECIALHI,
            ATKKIND_SPECIALLW,
            ATKKIND_22,
            ATKKIND_23,
            ATKKIND_24,
            ATKKIND_25,
            ATKKIND_26,
            ATKKIND_27,
            ATKKIND_28,
            ATKKIND_29,
            ATKKIND_30,
            ATKKIND_31,
            ATKKIND_32,
            ATKKIND_33,
            ATKKIND_34,
            ATKKIND_35,
            ATKKIND_36,
            ATKKIND_37,
            ATKKIND_38,
            ATKKIND_39,
            ATKKIND_40,
            ATKKIND_41,
            ATKKIND_42,
            ATKKIND_43,
            ATKKIND_44,
            ATKKIND_45,
            ATKKIND_46,
            ATKKIND_47,
            ATKKIND_48,
            ATKKIND_49,
            ATKKIND_DOWNATTACKU,
            ATKKIND_DOWNATTACKD,
            ATKKIND_PUMMEL,
            ATKKIND_FTHROW,
            ATKKIND_BTHROW,
            ATKKIND_UPTHROW,
            ATKKIND_DTHROW,
            ATKKIND_57,
            ATKKIND_58,
            ATKKIND_59,
            ATKKIND_60,
            ATKKIND_61,
            ATKKIND_62,
            ATKKIND_63,
            ATKKIND_64,
            ATKKIND_65,
            ATKKIND_66,
            ATKKIND_67,
            ATKKIND_68,
            ATKKIND_69,
            ATKKIND_70,
            ATKKIND_71,
            ATKKIND_72,
            ATKKIND_73,
            ATKKIND_74,
            ATKKIND_75,
            ATKKIND_76,
            ATKKIND_77,
            ATKKIND_78,
            ATKKIND_79,
            ATKKIND_80,
            ATKKIND_81,
            ATKKIND_82,
            ATKKIND_83,
            ATKKIND_84,
            ATKKIND_85,
            ATKKIND_86,
            ATKKIND_87,
        }

        private struct MoveLogic
        {
            public int ActionID;
            public int StateFlags;
            public byte AttackID;
            public byte BitFlags;

            public uint AnimationCallBack;
            public uint IASACallBack;
            public uint PhysicsCallback;
            public uint CollisionCallback;
            public uint CameraCallback;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="fighter"></param>
        /// <param name="moveLogicTable"></param>
        /// <returns></returns>
        public static bool ExtractMoveLogicTable(MexWorkspace ws, string structName, uint moveLogic, out string? moveLogicTable)
        {
            if (moveLogic == 0 ||
                !MoveLogicAddrToCount.ContainsKey(moveLogic))
            {
                moveLogicTable = null;
                return false;
            }

            StringBuilder table = new ();
            var dol = new MexDOL(ws.GetDOL());
            table.AppendLine($"FtState {structName}[] = {{");

            int index = 341;
            for (uint i = 0; i < MoveLogicAddrToCount[moveLogic]; i++)
            {
                var m = dol.GetStruct<MoveLogic>(moveLogic, i);
                // table.AppendLine($"\t// State: {index} - " + (fighterData != null && m.AnimationID != -1 && fighterData.FighterActionTable.Commands[m.AnimationID].Name != null ? System.Text.RegularExpressions.Regex.Replace(fighterData.FighterActionTable.Commands[m.AnimationID].Name.Replace("_figatree", ""), @"Ply.*_Share_ACTION_", "") : "Animation: " + m.AnimationID.ToString("X")));
                index++;

                //int action_id;
                //int flags;
                //char move_id;
                //char bitflags1;
                //void* animation_callback;
                //void* iasa_callback;
                //void* physics_callback;
                //void* collision_callback;
                //void* camera_callback;
                table.AppendLine(string.Format(
                    "\t[{18}] = {{" +
                    "\n\t\t.{0,-20} = {1}," +
                    "\n\t\t.{2,-20} = 0x{3:X8}," +
                    "\n\t\t.{4,-20} = {5}," +
                    "\n\t\t.{6,-20} = 0x{7:X8}," +
                    "\n\t\t.{8,-20} = 0x{9:X8}," +
                    "\n\t\t.{10,-20} = 0x{11:X8}," +
                    "\n\t\t.{12,-20} = 0x{13:X8}," +
                    "\n\t\t.{14,-20} = 0x{15:X8}," +
                    "\n\t\t.{16,-20} = {17}," +
                    "\n\t}},",
                    "action_id",            m.ActionID,
                    "flags",                m.StateFlags,
                    "move_id",              ((AttackKind)m.AttackID),
                    "bitflags1",            m.BitFlags,
                    "animation_callback",   m.AnimationCallBack,
                    "iasa_callback",        m.IASACallBack,
                    "physics_callback",     m.PhysicsCallback,
                    "collision_callback",   m.CollisionCallback,
                    "camera_callback",      m.CameraCallback == 0x800761C8 ? "Fighter_UpdateCameraBox" : $"0x{m.CameraCallback:X8}",
                    i
                    ));
            }

            table.Append(@"};");

            moveLogicTable = table.ToString();
            return true;
        }

    }
}
