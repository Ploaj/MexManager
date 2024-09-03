using HSDRaw;
using HSDRaw.Common;
using HSDRaw.Melee;
using HSDRaw.MEX;
using HSDRaw.MEX.Menus;
using HSDRaw.MEX.Sounds;
using HSDRaw.MEX.Stages;
using mexLib.Installer;
using System.Text;

namespace mexLib
{
    public class MexGenerator
    {
        /// ------------------------------------
        /// Source
        /// ------------------------------------
        public MexWorkspace Workspace { get; internal set; }

        public MEX_Data Data { get; } = new MEX_Data();

        /// ------------------------------------
        /// Generated
        /// ------------------------------------
        public List<MEX_Item> MexItems { get; } = new List<MEX_Item>();

        public List<MEX_EffectFiles> EffectFiles { get; } = new List<MEX_EffectFiles>();

        public List<MEX_StageIDTable> StageIDs { get; } = new List<MEX_StageIDTable>();

        private Dictionary<int, int> StageToID { get; } = new Dictionary<int, int>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        public MexGenerator(MexWorkspace workspace)
        {
            Workspace = workspace;
            var dol = new MexScubber.MexDOL(workspace.GetDOL());
            EffectFiles.AddRange(MexDefaultData.GenerateDefaultMexEffectSlots(dol));
            StageIDs.AddRange(MexDefaultData.GenerateDefaultStageIDs(dol));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public void Save()
        {
            // save mxdtfile
            var file = new HSDRawFile();
            file.Roots.Add(new HSDRootNode()
            {
                Name = "mexData",
                Data = Data,
            });
            file.Save(Workspace.GetFilePath("MxDt.dat"));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stage"></param>
        /// <returns></returns>
        public int GetStageExternalID(int stage)
        {
            if (StageToID.ContainsKey(stage))
                return StageToID[stage];

            return -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stages"></param>
        public void IndexStages(IEnumerable<MexStage> stages)
        {
            StageToID.Clear();
            int stageId = 0;
            foreach (var v in stages)
            {
                var stage_id_index = StageIDs.FindIndex(e => e.StageID == stageId);

                // check if this is a new stage
                if (stage_id_index == -1)
                {
                    // assign new external id for this stage
                    StageToID.Add(stageId, StageIDs.Count);
                    StageIDs.Add(new MEX_StageIDTable() { StageID = stageId });
                }
                else
                {
                    StageToID.Add(stageId, stage_id_index);
                }
                stageId++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public int GetEffectID(string filename, string symbol)
        {
            if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(symbol))
                return -1;

            // find existing effect slot and update the symbol if needed
            var effect = EffectFiles.FindIndex(e => e.FileName == filename);

            if (effect != -1)
            {
                EffectFiles[effect].Symbol = symbol;
                return effect;
            }

            // find slot for new effect
            var empty = EffectFiles.FindIndex(e =>
                e != EffectFiles[30] &&
                string.IsNullOrEmpty(e.FileName));

            if (empty != -1)
            {
                EffectFiles[empty].FileName = filename;
                EffectFiles[empty].Symbol = symbol;
                return empty;
            }

            // add new effect slot
            if (EffectFiles.Count < 64 && empty != 64)
            {
                EffectFiles.Add(new MEX_EffectFiles() { FileName = filename, Symbol = symbol });
                return EffectFiles.Count - 1;
            }

            // no room for new effects
            return -1;
        }
    }

    public static class MxDtCompiler
    {
        public static void Compile(MexWorkspace workspace)
        {
            var proj = workspace.Project;
            var gen = new MexGenerator(workspace);
            var mexData = gen.Data;

            // compile music table
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var menu_playlist = proj.MenuPlaylist.ToMexPlaylist();
            mexData.MusicTable = new()
            {
                BGMFileNames = new HSDFixedLengthPointerArrayAccessor<HSD_String>() { Array = proj.Music.Select(e => new HSD_String() { Value = e.FileName }).ToArray() },
                BGMLabels = new HSDFixedLengthPointerArrayAccessor<HSD_ShiftJIS_String>() { Array = proj.Music.Select(e => new HSD_ShiftJIS_String() { Value = e.Name }).ToArray() },
                MenuPlaylist = menu_playlist.MenuPlaylist,
                MenuPlayListCount = menu_playlist.MenuPlayListCount,
            };

            // generate stage ids
            gen.IndexStages(proj.Stages);

            // compile stage data
            mexData.StageData = new MEX_StageData();
            mexData.StageFunctions = new HSDFixedLengthPointerArrayAccessor<MEX_Stage>();

            for (int i = 0; i < proj.Stages.Count; i++)
                proj.Stages[i].ToMxDt(gen, i);

            mexData.StageData.StageIDTable = new HSDArrayAccessor<MEX_StageIDTable>() { Array = gen.StageIDs.ToArray() };

            // compile fighter data
            mexData.FighterData = new MEX_FighterData();
            mexData.FighterFunctions = new MEX_FighterFunctionTable();
            mexData.KirbyData = new MEX_KirbyTable();
            mexData.KirbyFunctions = new MEX_KirbyFunctionTable();

            // generate fighter runtimes
            mexData.FighterData._s.GetCreateReference<HSDAccessor>(0x40)._s.Resize(proj.Fighters.Count * 8);
            mexData.FighterData.RuntimeIntroParamLookup._s.Resize(proj.Fighters.Count * 4);

            // generate kirby runtime
            mexData.KirbyData.CapFileRuntime = new HSDAccessor() { _s = new HSDStruct(4 * proj.Fighters.Count) };
            mexData.KirbyData.CapFtCmdRuntime = new HSDAccessor() { _s = new HSDStruct(4 * proj.Fighters.Count) };
            mexData.KirbyData.CostumeRuntime = new HSDAccessor() { _s = new HSDStruct(4 * proj.Fighters.Count) };
            mexData.KirbyFunctions.MoveLogicRuntime = new HSDAccessor() { _s = new HSDStruct(4 * proj.Fighters.Count) };

            // write fighter data
            int internalId = 0;
            foreach (var f in proj.Fighters)
                f.ToMxDt(gen, internalId++);

            // save effects
            var effectFileCount = gen.EffectFiles.Count;
            mexData.EffectTable = new()
            {
                EffectFiles = new HSDArrayAccessor<MEX_EffectFiles>() { Array = gen.EffectFiles.ToArray() },
                RuntimeUnk1 = new HSDAccessor() { _s = new HSDStruct(0x60) },
                RuntimeUnk3 = new HSDAccessor() { _s = new HSDStruct(4 * effectFileCount) },
                RuntimeTexGrNum = new HSDAccessor() { _s = new HSDStruct(4 * effectFileCount) },
                RuntimeTexGrData = new HSDAccessor() { _s = new HSDStruct(4 * effectFileCount) },
                RuntimeUnk4 = new HSDAccessor() { _s = new HSDStruct(4 * effectFileCount) },
                RuntimePtclLast = new HSDAccessor() { _s = new HSDStruct(4 * effectFileCount) },
                RuntimePtclData = new HSDAccessor() { _s = new HSDStruct(4 * effectFileCount) },
                RuntimeLookup = new HSDAccessor() { _s = new HSDStruct(4 * effectFileCount) },
            };

            // save item table
            mexData.ItemTable = new MEX_ItemTables();
            mexData.ItemTable._s.SetInt32(0x00, unchecked((int)0x803F14C4));
            mexData.ItemTable._s.SetInt32(0x04, unchecked((int)0x803F3100));
            mexData.ItemTable._s.SetInt32(0x08, unchecked((int)0x803F23CC));
            mexData.ItemTable._s.SetInt32(0x0C, unchecked((int)0x803F4D20));
            //mxdt.ItemTable.CommonItems = new HSDArrayAccessor<MEX_Item>() { Array = CommonItems };
            //mxdt.ItemTable.FighterItems = new HSDArrayAccessor<MEX_Item>() { Array = FighterItems };
            //mxdt.ItemTable.Pokemon = new HSDArrayAccessor<MEX_Item>() { Array = PokemonItems };
            //mxdt.ItemTable.StageItems = new HSDArrayAccessor<MEX_Item>() { Array = StageItems };
            mexData.ItemTable.MEXItems = new HSDArrayAccessor<MEX_Item>() { Array = gen.MexItems.ToArray() };
            mexData.ItemTable._s.GetCreateReference<HSDAccessor>(0x14)._s.Resize(Math.Max(4, gen.MexItems.Count * 4));

            // add scene data
            mexData.SceneData = workspace.Project.SceneData;

            // write sound data
            mexData.SSMTable = new MEX_SSMTable()
            {
                SSM_SSMFiles = new HSDNullPointerArrayAccessor<HSD_String>(),
                SSM_BufferSizes = new HSDArrayAccessor<MEX_SSMSizeAndFlags>(),
                SSM_LookupTable = new HSDArrayAccessor<MEX_SSMLookup>(),
            };

            mexData.SSMTable.SSM_BufferSizes.Set(proj.Soundbanks.Count, new MEX_SSMSizeAndFlags());// blank entry at end
            mexData.SSMTable.SSM_LookupTable.Set(proj.Soundbanks.Count, new MEX_SSMLookup());// blank entry at beginning

            for (int i = 0; i < proj.Soundbanks.Count; i++)
            {
                proj.Soundbanks[i].ToMxDt(gen, i);
            }

            var ssm_runtime_length = proj.Soundbanks.Count * 4;
            HSDStruct rtTable = new (6 * 4);
            rtTable.SetReferenceStruct(0x00, new HSDStruct(Enumerable.Repeat((byte)0x01, 0x180).ToArray()));
            rtTable.SetReferenceStruct(0x04, new HSDStruct(Enumerable.Repeat((byte)0x02, ssm_runtime_length).ToArray()));
            rtTable.SetReferenceStruct(0x08, new HSDStruct(Enumerable.Repeat((byte)0x03, ssm_runtime_length).ToArray()));
            rtTable.SetReferenceStruct(0x0C, new HSDStruct(Enumerable.Repeat((byte)0x04, ssm_runtime_length).ToArray()));
            rtTable.SetReferenceStruct(0x10, new HSDStruct(Enumerable.Repeat((byte)0x05, ssm_runtime_length).ToArray()));
            rtTable.SetReferenceStruct(0x14, new HSDStruct(Enumerable.Repeat((byte)0x06, ssm_runtime_length).ToArray()));
            mexData.SSMTable._s.SetReferenceStruct(0x0C, rtTable);

            // meta data
            mexData.MetaData = new MEX_Meta()
            {
                NumOfCSSIcons = proj.CharacterSelect.FighterIcons.Count,
                NumOfSSSIcons = proj.StageSelects[0].StageIcons.Count + 1,
                NumOfEffects = gen.EffectFiles.Count,
                NumOfSSMs = proj.Soundbanks.Count,
                NumOfMusic = proj.Music.Count,
                NumOfInternalIDs = proj.Fighters.Count,
                NumOfExternalIDs = proj.Fighters.Count,
                NumOfInternalStage = proj.Stages.Count,
                NumOfExternalStage = gen.StageIDs.Count,
                EnterScene = proj.StartingScene,
                LastMajor = proj.LastMajorSceneID,
                LastMinor = proj.LastMinorSceneID,
                TrophyCount = proj.TrophyCount,
                TrophySDOffset = proj.TrophySDOffset,
            };
            mexData.MetaData._s.SetByte(0, workspace.VersionMajor);
            mexData.MetaData._s.SetByte(1, workspace.VersionMinor);

            // write menu data
            mexData.MenuTable = new MEX_MenuTable();
            proj.CharacterSelect.ToMxDt(gen);
            proj.StageSelects[0].ToMxDt(gen);

            // write misc data
            mexData.MiscData = new HSDRaw.MEX.Misc.MEX_Misc()
            {
                GawColors = new HSDArrayAccessor<HSDRaw.MEX.Misc.MEX_GawColor>() { Array = proj.GawColors.ToArray() }
            };

            // save files
            gen.Save();
        }
    }
}
