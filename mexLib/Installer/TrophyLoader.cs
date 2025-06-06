﻿using HSDRaw;
using HSDRaw.Melee;
using HSDRaw.Melee.Ty;
using HSDRaw.MEX;
using mexLib.HsdObjects;
using static mexLib.Types.MexTrophy;

namespace mexLib.Installer
{
    public class TrophyLoader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        public static void LoadFromFileSystem(MexWorkspace workspace)
        {
            MexProject project = workspace.Project;
            project.Trophies.Clear();

            // load files
            HSDRawFile tyDataFile = new(workspace.FileManager.Get(workspace.GetFilePath("TyDataf.dat")));
            HSDRawFile tyDataInfoFile = new(workspace.FileManager.Get(workspace.GetFilePath("TyDatai.usd")));
            HSDRawFile sdToyFile = new(workspace.FileManager.Get(workspace.GetFilePath("SdToy.usd")));
            HSDRawFile sdToyExpFile = new(workspace.FileManager.Get(workspace.GetFilePath("SdToyExp.usd")));

            // load symbols
            SBM_SISData? sdToy = sdToyFile["SIS_ToyData_E"].Data as SBM_SISData;
            SBM_SISData? sdToyExp = sdToyExpFile["SIS_ToyDataExp_E"].Data as SBM_SISData;
            HSDShortArray? tyExpDifferentTbl = tyDataInfoFile["tyExpDifferentTbl"].Data as HSDShortArray;
            HSDArrayAccessor<SBM_TyModelFileEntry>? tyModelFileTbl = tyDataFile["tyModelFileTbl"].Data as HSDArrayAccessor<SBM_TyModelFileEntry>;
            HSDArrayAccessor<SBM_TyModelFileEntry>? tyModelFileUsTbl = tyDataFile["tyModelFileUsTbl"].Data as HSDArrayAccessor<SBM_TyModelFileEntry>;

            HSDArrayAccessor<SBM_tyDisplayModelEntry>? tyDisplayModelTbl = tyDataInfoFile["tyDisplayModelTbl"].Data as HSDArrayAccessor<SBM_tyDisplayModelEntry>;
            HSDArrayAccessor<SBM_tyDisplayModelEntry>? tyDisplayModelUsTbl = tyDataInfoFile["tyDisplayModelUsTbl"].Data as HSDArrayAccessor<SBM_tyDisplayModelEntry>;
            HSDArrayAccessor<SBM_tyInitModelEntry>? tyInitModelTbl = tyDataInfoFile["tyInitModelTbl"].Data as HSDArrayAccessor<SBM_tyInitModelEntry>;
            HSDArrayAccessor<SBM_tyInitModelEntry>? tyInitModelDTbl = tyDataInfoFile["tyInitModelDTbl"].Data as HSDArrayAccessor<SBM_tyInitModelEntry>;

            HSDArrayAccessor<SBM_tyModelSortEntry>? tyModelSortTbl = tyDataInfoFile["tyModelSortTbl"].Data as HSDArrayAccessor<SBM_tyModelSortEntry>;
            HSDShortArray? tyNoGetUsTbl = tyDataInfoFile["tyNoGetUsTbl"].Data as HSDShortArray;

            // check if symbols found
            if (sdToy == null ||
                sdToyExp == null ||
                tyExpDifferentTbl == null ||
                tyModelFileTbl == null ||
                tyModelFileUsTbl == null ||
                tyDisplayModelTbl == null ||
                tyDisplayModelUsTbl == null ||
                tyInitModelTbl == null ||
                tyInitModelDTbl == null ||
                tyModelSortTbl == null ||
                tyNoGetUsTbl == null)
                return;

            // init data
            int trophyCount = 293;

            int nameOff = 0x002;
            int nameOffAlt = 297;

            // extract and update with data from mxdt
            string mxdtPath = workspace.GetFilePath("MxDt.dat");
            if (File.Exists(mxdtPath))
            {
                HSDRawFile mxdtFile = new(mxdtPath);
                MEX_Data? mxdt = mxdtFile["mexData"].Data as MEX_Data;
                if (mxdt != null)
                {
                    trophyCount = mxdt.MetaData.TrophyCount;
                    nameOffAlt = mxdt.MetaData.TrophySDOffset;
                }
            }

            int descOff = 0x002;
            int descOffAlt = 0x374;
            int src1Off = 0x128;
            int src1OffAlt = 0x37A;
            int src2Off = 0x24E;
            int src2OffAlt = 0x380;

            // check for offset node
            HSDAccessor? offset = sdToyExpFile["offset"]?.Data;
            if (offset != null)
            {
                SISOffset off = new() { _s = offset._s };
                descOff = off.Desciption;
                descOffAlt = off.DesciptionAlt;
                src1Off = off.Src1;
                src1OffAlt = off.Src1Alt;
                src2Off = off.Src2;
                src2OffAlt = off.Src2Alt;
            }

            // 0x04E 0x174 - special

            // 0x002 0x128 - name 

            // 0x002 0x374 - description
            // 0x128 0x37A - source1
            // 0x24E 0x380 - source2

            // init trophy data
            for (int i = 0; i < trophyCount; i++)
            {
                project.Trophies.Add(new Types.MexTrophy());
            }

            // grab data
            List<short> expDif = tyExpDifferentTbl.Array.ToList();
            SBM_TyModelFileEntry[] fileNameArray = tyModelFileTbl.Array;
            SBM_TyModelFileEntry[] fileNameArrayUS = tyModelFileUsTbl.Array;

            // load file names
            LoadTrophyFileNames(project, fileNameArray, false);
            LoadTrophyFileNames(project, fileNameArrayUS, true);

            // load text
            for (int i = 0; i < trophyCount; i++)
            {
                // get name
                project.Trophies[i].Data.Text = new TrophyTextEntry()
                {
                    Name = sdToy.SISData[i + nameOff - 1].TextCode,
                    Description = sdToyExp.SISData[i + descOff - 1].TextCode,
                    Source1 = sdToyExp.SISData[i + src1Off - 1].TextCode,
                    Source2 = sdToyExp.SISData[i + src2Off - 1].TextCode,
                };
                project.Trophies[i].Data.Text.DecodeAllStrings();

                int alti = expDif.IndexOf((short)i);
                if (alti != -1 && i != 0)
                {
                    project.Trophies[i].HasUSData = true;

                    project.Trophies[i].USData.Text = new TrophyTextEntry()
                    {
                        Name = sdToy.SISData[alti + nameOffAlt - 1].TextCode,
                        Description = sdToyExp.SISData[alti + descOffAlt - 1].TextCode,
                        Source1 = sdToyExp.SISData[alti + src1OffAlt - 1].TextCode,
                        Source2 = sdToyExp.SISData[alti + src2OffAlt - 1].TextCode,
                    };
                    project.Trophies[i].USData.Text.DecodeAllStrings();
                }
            }

            // load 2d parameters
            Load2DDisplayModelInfo(project, tyDisplayModelTbl.Array, false);
            Load2DDisplayModelInfo(project, tyDisplayModelUsTbl.Array, true);

            // load model display parameters
            LoadDisplayModelInfo(project, tyInitModelTbl.Array, false);
            LoadDisplayModelInfo(project, tyInitModelDTbl.Array, true);

            // load model sort params
            foreach (SBM_tyModelSortEntry i in tyModelSortTbl.Array)
            {
                if (i.TrophyID == -1)
                    break;

                project.Trophies[i.x02].SortSeries = i.TrophyID;
            }

            // load no get info
            foreach (short i in tyNoGetUsTbl.Array)
                if (i >= 0 && i < project.Trophies.Count)
                    project.Trophies[i].JapanOnly = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="trophies"></param>
        /// <param name="displayModel"></param>
        private static void LoadDisplayModelInfo(MexProject project, SBM_tyInitModelEntry[] displayModel, bool is_us)
        {
            foreach (SBM_tyInitModelEntry i in displayModel)
            {
                if (i.TrophyID == -1)
                    break;

                if (i.TrophyID >= 0 && i.TrophyID < project.Trophies.Count)
                {
                    TrophyParams param = new()
                    {
                        TrophyType = (TrophyType)i.TrophyType,
                        OffsetX = i.OffsetX,
                        OffsetY = i.OffsetY,
                        OffsetZ = i.OffsetZ,
                        TrophyScale = i.ModelScale,
                        StandScale = i.StandScale,
                        Rotation = i.YRotation,
                        UnlockCondition = (TrophyUnlockKind)i.x20,
                        UnlockParam = i.x21,
                    };

                    if (is_us)
                        project.Trophies[i.TrophyID].USData.Param3D = param;
                    else
                        project.Trophies[i.TrophyID].Data.Param3D = param;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trophies"></param>
        /// <param name="displayModel"></param>
        private static void Load2DDisplayModelInfo(MexProject project, SBM_tyDisplayModelEntry[] displayModel, bool is_us)
        {
            foreach (SBM_tyDisplayModelEntry m in displayModel)
            {
                if (m.TrophyID == -1)
                    break;

                if (m.TrophyID >= 0 && m.TrophyID < project.Trophies.Count)
                {
                    Trophy2DParams param2d = new()
                    {
                        FileIndex = m.Trophy2DFileIndex,
                        ImageIndex = m.Trophy2DImageIndex,
                        OffsetX = m.OffsetX,
                        OffsetY = m.OffsetY,
                    };

                    if (is_us)
                        project.Trophies[m.TrophyID].USData.Param2D = param2d;
                    else
                        project.Trophies[m.TrophyID].Data.Param2D = param2d;

                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trophies"></param>
        /// <param name="files"></param>
        private static void LoadTrophyFileNames(MexProject project, SBM_TyModelFileEntry[] files, bool is_us)
        {
            foreach (SBM_TyModelFileEntry v in files)
            {
                if (v.TrophyIndex < 0)
                    continue;

                if (v.TrophyIndex >= project.Trophies.Count)
                    continue;

                TrophyFileEntry file = new()
                {
                    File = v.FileName.TrimEnd('\0'),
                    Symbol = v.SymbolName.TrimEnd('\0'),
                };

                if (is_us)
                    project.Trophies[v.TrophyIndex].USData.File = file;
                else
                    project.Trophies[v.TrophyIndex].Data.File = file;
            }
        }
    }
}
