﻿using HSDRaw;
using HSDRaw.Common;
using HSDRaw.MEX;
using HSDRaw.MEX.Misc;
using mexLib.Attributes;
using mexLib.Installer;
using mexLib.MexScubber;
using mexLib.Utilties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO.Compression;

namespace mexLib.Types
{
    public partial class MexFighter : MexReactiveObject
    {
        [Category("0 - General"), DisplayName("Name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        private string _name = "New Fighter";

        [Category("0 - General"), DisplayName("Series")]
        [MexLink(MexLinkType.Series)]
        public int SeriesID { get; set; } = 0;

        [Category("0 - General"), DisplayName("Can Wall Jump"), Description("Determines if fighter can wall jump")]
        public bool CanWallJump { get; set; }

        [Category("0 - General"), DisplayName("Sub-Fighter"), Description("The fighter associated with this fighter (Sheik/Zelda and Ice Climbers)")]
        public int SubCharacter
        {
            get => _subCharacter; 
            set
            {
                _subCharacter = value;
                if (_subCharacter >= 255)
                    _subCharacter = -1;
            }
        }
        private int _subCharacter = -1;

        [Category("0 - General"), DisplayName("Sub-Fighter Behavior"), Description("The association between this fighter and the sub-fighter")]
        public SubCharacterBehavior SubCharacterBehavior { get; set; }

        [Category("1 - Audio"), DisplayName("SoundBank")]
        [MexLink(MexLinkType.Sound)]
        public int SoundBank { get; set; } = 55; // default is null

        [Browsable(false)]
        public uint SSMBitfield1 { get; set; }

        [Browsable(false)]
        public uint SSMBitfield2 { get; set; }

        [Category("1 - Audio"), DisplayName("Narrator Sound Clip"), Description("Sound effect index of narrator sound clip")]
        public int AnnouncerCall { get; set; }

        [Category("1 - Audio"), DisplayName("Victory Theme"), Description("Music to play on victory screen")]
        [MexLink(MexLinkType.Music)]
        public int VictoryTheme { get; set; }

        [Category("1 - Audio"), DisplayName("Fighter Music 1"), Description("Possible music to play for fighter credits")]
        [MexLink(MexLinkType.Music)]
        public int FighterMusic1 { get; set; }

        [Category("1 - Audio"), DisplayName("Fighter Music 2"), Description("Possible music to play for fighter credits")]
        [MexLink(MexLinkType.Music)]
        public int FighterMusic2 { get; set; }

        [Category("2 - Single Player"), DisplayName("Target Test Stage"), Description("The stage id of the target test stage for this fighter")]
        [MexLink(MexLinkType.Stage)]
        public int TargetTestStage { get; set; } = 0;

        [Category("2 - Single Player"), DisplayName(""), Description("")]
        public short ClassicTrophyId { get; set; }

        [Category("2 - Single Player"), DisplayName(""), Description("")]
        public short AdventureTrophyId { get; set; }

        [Category("2 - Single Player"), DisplayName(""), Description("")]
        public short AllStarTrophyId { get; set; }

        [Category("2 - Single Player"), DisplayName("Race to the Finish Time"), Description("Seconds the fighter has to complete \"Race to the Finish\"")]
        public uint RacetoTheFinishTime { get; set; }

        [Category("2 - Single Player"), DisplayName("Result Screen Scale"), Description("Amount to scale model on result screen")]
        public float ResultScreenScale { get; set; }

        [Category("2 - Single Player"), DisplayName("Ending Screen Scale"), Description("Amount to scale model on trophy fall screen")]
        public float EndingScreenScale { get; set; }

        [Browsable(false)]
        public ObservableCollection<MexItem> Items { get; set; } = new ObservableCollection<MexItem>();

        [Browsable(false)]
        public MexFighterBoneDefinitions BoneDefinitions { get; set; } = new MexFighterBoneDefinitions();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mexData"></param>
        /// <param name="workspace"></param>
        public void ToMxDt(MexGenerator mex, int internalId)
        {
            MEX_Data mexData = mex.Data;
            int externalId = MexFighterIDConverter.ToExternalID(internalId, mex.Workspace.Project.Fighters.Count);
            MEX_KirbyTable kb = mexData.KirbyData;
            MEX_FighterData fd = mexData.FighterData;

            fd.NameText.Set(externalId, new HSDRaw.Common.HSD_String(Name));
            fd.CharFiles.Set(internalId, new MEX_CharFileStrings() { FileName = Files.FighterDataPath, Symbol = Files.FighterDataSymbol });

            fd.AnimCount.Set(internalId, new MEX_AnimCount() { AnimCount = (int)Files.AnimCount });
            fd.AnimFiles.Set(internalId, new HSD_String(Files.AnimFile));
            fd.ResultAnimFiles.Set(externalId, new HSD_String(Files.RstAnimFile));
            fd.RstRuntime.Set(internalId, new HSDRaw.MEX.Characters.MEX_RstRuntime() { AnimMax = (int)Files.RstAnimCount });
            fd.InsigniaIDs[externalId] = (byte)SeriesID;
            fd.WallJump[internalId] = CanWallJump ? (byte)1 : (byte)0;
            fd.EffectIDs[internalId] = (byte)mex.GetEffectID(Files.EffectFile, Files.EffectSymbol);
            fd.AnnouncerCalls[externalId] = AnnouncerCall;

            // create costume strings
            fd.CostumeFileSymbols.Set(internalId, CostumesToMxDt());

            // gaw colors hack
            if (GaWData != null)
            {
                // write misc data
                mexData.MiscData = new MEX_Misc()
                {
                    GawColors = new HSDArrayAccessor<MEX_GawColor>()
                    {
                        Array = GaWData.Colors.Select(e => new MEX_GawColor()
                        {
                            FillColor = Color.FromArgb((int)e.Fill),
                            OutlineColor = Color.FromArgb((int)e.Outline),
                        }).ToArray()
                    }
                };
            }

            // create costume runtime
            fd.CostumePointers.Set(internalId, new MEX_CostumeRuntimePointers()
            {
                CostumeCount = (byte)Costumes.Count,
                Pointer = new HSDRaw.HSDAccessor() { _s = new HSDRaw.HSDStruct(0x18 * Costumes.Count) }
            });

            // create costume lookups
            fd.CostumeIDs.Set(externalId, new MEX_CostumeIDs()
            {
                CostumeCount = (byte)Costumes.Count,
                RedCostumeIndex = RedCostumeIndex,
                BlueCostumeIndex = BlueCostumeIndex,
                GreenCostumeIndex = GreenCostumeIndex
            });

            fd.DefineIDs.Set(externalId, new MEX_CharDefineIDs()
            {
                InternalID = (byte)(internalId + (internalId == 11 ? -1 : 0)),
                SubCharacterBehavior = SubCharacterBehavior,
                SubCharacterInternalID = (byte)SubCharacter
            });

            fd.SSMFileIDs.Set(externalId, new MEX_CharSSMFileID()
            {
                SSMID = (byte)SoundBank,
                Unknown = 0,
                BitField1 = (int)SSMBitfield1,
                BitField2 = (int)SSMBitfield2
            });

            fd.VictoryThemeIDs[externalId] = VictoryTheme;
            fd.FighterSongIDs.Set(externalId, new HSDRaw.MEX.Characters.MEX_FighterSongID()
            {
                SongID1 = (short)FighterMusic1,
                SongID2 = (short)FighterMusic2,
            });

            fd.FtDemo_SymbolNames.Set(internalId, new MEX_FtDemoSymbolNames()
            {
                Intro = Files.DemoIntro,
                Ending = Files.DemoEnding,
                Result = Files.DemoResult,
                ViWait = Files.DemoWait
            });

            fd.VIFiles.Set(externalId, new HSD_String(Files.DemoFile));

            fd.ResultScale[externalId] = ResultScreenScale;
            fd.TargetTestStageLookups[externalId] = (ushort)TargetTestStage;
            fd.RaceToFinishTimeLimits[externalId] = (int)RacetoTheFinishTime;
            fd.EndClassicFiles.Set(externalId, new HSD_String(Media.EndClassicFile));
            fd.EndAdventureFiles.Set(externalId, new HSD_String(Media.EndAdventureFile));
            fd.EndAllStarFiles.Set(externalId, new HSD_String(Media.EndAllStarFile));
            fd.EndMovieFiles.Set(externalId, new HSD_String(Media.EndMovieFile));

            fd.ClassicTrophyLookup[externalId] = ClassicTrophyId;
            fd.AdventureTrophyLookup[externalId] = AdventureTrophyId;
            fd.AllStarTrophyLookup[externalId] = AllStarTrophyId;
            fd.EndingFallScale[externalId] = EndingScreenScale;

            // Kirby
            kb.CapFiles.Set(internalId, new MEX_KirbyCapFiles()
            {
                FileName = string.IsNullOrEmpty(Files.KirbyCapFileName) ? null : Files.KirbyCapFileName,
                Symbol = string.IsNullOrEmpty(Files.KirbyCapSymbol) ? null : Files.KirbyCapSymbol,
            });
            kb.KirbyEffectIDs[internalId] = (byte)mex.GetEffectID(Files.KirbyEffectFile, Files.KirbyEffectSymbol);
            if (KirbyCostumes.Count > 0)
            {
                kb.KirbyCostumes.Set(internalId, new MEX_KirbyCostume()
                {
                    Array = KirbyCostumes.Select(e => new MEX_CostumeFileSymbol()
                    {
                        FileName = e.FileName,
                        JointSymbol = e.JointSymbol,
                        MatAnimSymbol = e.MaterialSymbol,
                    }).ToArray()
                });
                kb.CostumeRuntime._s.SetReference(internalId * 4, new HSDAccessor() { _s = new HSDStruct(KirbyCostumes.Count * 8) });
            }
            else
            {
                kb.KirbyCostumes.Set(internalId, null);
                kb.CostumeRuntime._s.SetReference(internalId * 4, null);
            }

            // Functions
            Functions.ToMxDt(mexData, internalId);

            // save items
            ushort[] itemEntries = new ushort[Items.Count];
            for (int i = 0; i < itemEntries.Length; i++)
            {
                itemEntries[i] = (ushort)(MexDefaultData.BaseItemCount + mex.MexItems.Count);
                mex.MexItems.Add(Items[i].ToMexItem());
            }
            mexData.FighterData.FighterItemLookup.Set(internalId, new MEX_ItemLookup() { Entries = itemEntries });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="mxdt"></param>
        internal void FromMxDt(MexWorkspace workspace, MEX_Data mxdt, MexDOL dol, int internalId)
        {
            int externalId = MexFighterIDConverter.ToExternalID(internalId, mxdt.MetaData.NumOfInternalIDs);
            MEX_KirbyTable kb = mxdt.KirbyData;
            MEX_FighterData fd = mxdt.FighterData;

            // import general
            Name = fd.NameText[externalId].Value;
            SeriesID = fd.InsigniaIDs[externalId];
            CanWallJump = fd.WallJump[internalId] != 0;
            AnnouncerCall = fd.AnnouncerCalls[externalId];

            // import files
            Files.FighterDataPath = fd.CharFiles[internalId].FileName;
            Files.FighterDataSymbol = fd.CharFiles[internalId].Symbol;

            Files.AnimCount = (uint)fd.AnimCount[internalId].AnimCount;
            Files.AnimFile = fd.AnimFiles[internalId].Value;
            Files.RstAnimFile = fd.ResultAnimFiles[externalId].Value;
            Files.RstAnimCount = (uint)fd.RstRuntime[internalId].AnimMax;

            byte effect_id = fd.EffectIDs[internalId];
            if (effect_id != 255)
            {
                Files.EffectFile = mxdt.EffectTable.EffectFiles[effect_id].FileName;
                Files.EffectSymbol = mxdt.EffectTable.EffectFiles[effect_id].Symbol;
            }

            // import costume strings
            CostumesFromMxDt(fd.CostumeFileSymbols[internalId]);

            // import gaw colors hack
            if (internalId == 24)
            {
                GaWData = new FighterGaWExt();
                foreach (MEX_GawColor? e in mxdt.MiscData.GawColors.Array)
                {
                    GaWData.Colors.Add(new GAWColor()
                    {
                        Fill = (uint)e.FillColor.ToArgb(),
                        Outline = (uint)e.OutlineColor.ToArgb(),
                    });
                }
            }

            // import costume lookups
            RedCostumeIndex = fd.CostumeIDs[externalId].RedCostumeIndex;
            BlueCostumeIndex = fd.CostumeIDs[externalId].BlueCostumeIndex;
            GreenCostumeIndex = fd.CostumeIDs[externalId].GreenCostumeIndex;

            // import character ids
            SubCharacter = fd.DefineIDs[externalId].SubCharacterInternalID;
            SubCharacterBehavior = fd.DefineIDs[externalId].SubCharacterBehavior;

            // import sound
            SoundBank = fd.SSMFileIDs[externalId].SSMID;
            SSMBitfield1 = (uint)fd.SSMFileIDs[externalId].BitField1;
            SSMBitfield2 = (uint)fd.SSMFileIDs[externalId].BitField2;

            // import music
            VictoryTheme = fd.VictoryThemeIDs[externalId];
            FighterMusic1 = fd.FighterSongIDs[externalId].SongID1;
            FighterMusic2 = fd.FighterSongIDs[externalId].SongID2;

            // import demo symbols
            Files.DemoIntro = fd.FtDemo_SymbolNames[internalId].Intro;
            Files.DemoEnding = fd.FtDemo_SymbolNames[internalId].Ending;
            Files.DemoResult = fd.FtDemo_SymbolNames[internalId].Result;
            Files.DemoWait = fd.FtDemo_SymbolNames[internalId].ViWait;
            Files.DemoFile = fd.VIFiles[externalId].Value;

            // import misc 2
            ResultScreenScale = fd.ResultScale[externalId];
            TargetTestStage = fd.TargetTestStageLookups[externalId];
            RacetoTheFinishTime = (uint)fd.RaceToFinishTimeLimits[externalId];

            // import media
            Media.EndClassicFile = fd.EndClassicFiles[externalId].Value;
            Media.EndAdventureFile = fd.EndAdventureFiles[externalId].Value;
            Media.EndAllStarFile = fd.EndAllStarFiles[externalId].Value;
            Media.EndMovieFile = fd.EndMovieFiles[externalId].Value;

            // import trophy
            ClassicTrophyId = fd.ClassicTrophyLookup[externalId];
            AdventureTrophyId = fd.AdventureTrophyLookup[externalId];
            AllStarTrophyId = fd.AllStarTrophyLookup[externalId];
            EndingScreenScale = fd.EndingFallScale[externalId];

            // Kirby
            Files.KirbyCapFileName = kb.CapFiles[internalId].FileName;
            Files.KirbyCapSymbol = kb.CapFiles[internalId].Symbol;

            byte kbeffect_id = kb.KirbyEffectIDs[internalId];
            if (kbeffect_id != 255)
            {
                Files.KirbyEffectFile = mxdt.EffectTable.EffectFiles[kbeffect_id].FileName;
                Files.KirbyEffectSymbol = mxdt.EffectTable.EffectFiles[kbeffect_id].Symbol;
            }

            // Kirby Costumes
            MEX_KirbyCostume costumes = kb.KirbyCostumes[internalId];
            if (costumes != null)
            {
                for (int i = 0; i < costumes.Length; i++)
                {
                    KirbyCostumes.Add(new()
                    {
                        FileName = costumes[i].FileName,
                        JointSymbol = costumes[i].JointSymbol,
                        MaterialSymbol = costumes[i].MatAnimSymbol,
                    });
                }
            }

            // Functions
            Functions.FromMxDt(mxdt, internalId);

            // extract logic pointers from dol since old mex stored the actual data
            if (!MexFighterIDConverter.IsMexFighter(internalId, workspace.Project.Fighters.Count))
            {
                uint oldInternal = (uint)MexFighterIDConverter.ToInternalID(
                    MexFighterIDConverter.ToExternalID(internalId, workspace.Project.Fighters.Count),
                    0x21);

                // get original internal id
                Functions.MoveLogicPointer = dol.GetStruct<uint>(0x803C12E0, oldInternal);
                Functions.DemoMoveLogicPointer = dol.GetStruct<uint>(0x803C1364, oldInternal);
            }

            // import items
            Items.Clear();
            foreach (ushort i in fd.FighterItemLookup[internalId].Entries)
            {
                MexItem item = new();
                item.FromMexItem(mxdt.ItemTable.MEXItems[i - MexDefaultData.BaseItemCount]);
                Items.Add(item);
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
            uint exid = (uint)MexFighterIDConverter.ToExternalID((int)index, 0x21);

            // default data
            Name = MexDefaultData.Fighter_Names[exid];
            SeriesID = MexDefaultData.Fighter_SeriesIDs[exid];
            AnnouncerCall = MexDefaultData.Fighter_AnnouncerCalls[exid];
            TargetTestStage = MexDefaultData.Fighter_TargetTestStages[exid];
            VictoryTheme = MexDefaultData.Fighter_VictoryThemes[exid];
            RacetoTheFinishTime = MexDefaultData.Fighter_RaceToTheFinishTimes[exid];

            CanWallJump = MexDefaultData.Fighter_CanWallJump[index] != 0;

            // scrub general data
            Files.FighterDataPath = dol.GetStruct<string>(0x803c1f40, index, 8);
            Files.FighterDataSymbol = dol.GetStruct<string>(0x803c1f40 + 0x4, index, 8);
            Files.AnimFile = dol.GetStruct<string>(0x803C23E4, index);
            Files.AnimCount = dol.GetStruct<uint>(0x803C0FC8 + 0x4, index, 8);
            // have to search for result anim
            for (uint i = 0; i < 0x21; i++)
            {
                uint rstId = dol.GetStruct<uint>(0x803d53a8 + 0x00, i, 8);

                if (rstId == 0x21)
                    break;

                if (rstId == exid)
                {
                    Files.RstAnimFile = dol.GetStruct<string>(0x803d53a8 + 0x04, i, 8);
                    break;
                }
            }
            Files.RstAnimCount = dol.GetStruct<uint>(0x803C25F4 + 0x04, index, 8);

            // demo
            if (exid < 0x21 - 7)
            {
                Files.DemoFile = dol.GetStruct<string>(0x803FFFA8, exid);
                uint demoOffset = dol.GetStruct<uint>(0x803C2468, index);
                Files.DemoResult = dol.GetStruct<string>(demoOffset + 0x00, 0, 0x10);
                Files.DemoIntro = dol.GetStruct<string>(demoOffset + 0x04, 0, 0x10);
                Files.DemoEnding = dol.GetStruct<string>(demoOffset + 0x08, 0, 0x10);
                Files.DemoWait = dol.GetStruct<string>(demoOffset + 0x0C, 0, 0x10);
            }

            // media
            Media.EndClassicFile = dol.GetStruct<string>(0x803DB8B8, exid);
            Media.EndAdventureFile = dol.GetStruct<string>(0x803DBBF4, exid);
            Media.EndAllStarFile = dol.GetStruct<string>(0x803DBF10, exid);
            Media.EndMovieFile = dol.GetStruct<string>(0x803DB1F4, exid);

            // fighter music
            FighterMusic1 = dol.GetStruct<byte>(0x803BC4A0 + 0x00, exid, 2);
            FighterMusic2 = dol.GetStruct<byte>(0x803BC4A0 + 0x01, exid, 2);

            // trophies
            ClassicTrophyId = dol.GetStruct<short>(0x803B7978, exid);
            AdventureTrophyId = dol.GetStruct<short>(0x803B79BC, exid);
            AllStarTrophyId = dol.GetStruct<short>(0x803B7A00, exid);

            // kirby
            Files.KirbyCapFileName = dol.GetStruct<string>(0x803CA9D0 + 0x00, index, 8);
            Files.KirbyCapSymbol = dol.GetStruct<string>(0x803CA9D0 + 0x04, index, 8);

            // misc
            ResultScreenScale = dol.GetStruct<float>(0x803D7058, exid);
            EndingScreenScale = dol.GetStruct<float>(0x803DB2EC, exid);
            SubCharacter = (sbyte)dol.GetStruct<byte>(0x803BCDE0 + 0x01, exid, 3);
            SubCharacterBehavior = (SubCharacterBehavior)dol.GetStruct<byte>(0x803BCDE0 + 0x02, exid, 3);

            // effects
            byte effect_id = dol.GetStruct<byte>(0x803C26FC, index);
            byte kirby_effect_id = dol.GetStruct<byte>(0x803CB46C, index);

            Files.EffectFile = dol.GetStruct<string>(0x803c025c + 0x00, effect_id, 0x0C);
            Files.EffectSymbol = dol.GetStruct<string>(0x803c025c + 0x04, effect_id, 0x0C);

            Files.KirbyEffectFile = dol.GetStruct<string>(0x803c025c + 0x00, kirby_effect_id, 0x0C);
            Files.KirbyEffectSymbol = dol.GetStruct<string>(0x803c025c + 0x04, kirby_effect_id, 0x0C);

            // ssm 
            SoundBank = dol.GetStruct<byte>(0x803BB3C0 + 0x00, exid, 0x10);
            SSMBitfield1 = dol.GetStruct<uint>(0x803BB3C0 + 0x08, exid, 0x10);
            SSMBitfield2 = dol.GetStruct<uint>(0x803BB3C0 + 0x0C, exid, 0x10);

            // css
            CostumesFromDOL(dol, index);

            // ext
            LoadExtFromDol(index);

            // Functions
            Functions.FromDOL(dol, index);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
        public class FighterPackOptions
        {
            [Category("Options")]
            [DisplayName("Include Files")]
            public bool ExportFiles { get; set; } = true;

            [Category("Options")]
            [DisplayName("Include Soundbank")]
            public bool ExportSoundBank { get; set; } = true;

            [Category("Options")]
            [DisplayName("Include Media")]
            public bool ExportMedia { get; set; } = true;

            [Category("Options")]
            [DisplayName("Include Costumes")]
            public bool ExportCostumes { get; set; } = true;

            // TODO: convert sound ids to mex
            // TODO: convert effect ids to mex

            // TODO: extract and create demo files ftDemoIntroMotionFile

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="zip"></param>
        public MexInstallerError? ToPackage(MexWorkspace workspace, Stream stream, FighterPackOptions options)
        {
            // create zip
            using ZipWriter zip = new(stream);

            // fighter to package
            zip.WriteAsJson("fighter.json", this);

            // write files
            if (options.ExportFiles)
                Files.ToPackage(workspace, zip);

            // write assets
            Assets.ToPackage(workspace, zip);

            // write media
            if (options.ExportMedia)
            {
                zip.TryWriteFile(workspace, Media.EndAllStarFile, Media.EndAllStarFile);
                zip.TryWriteFile(workspace, Media.EndClassicFile, Media.EndClassicFile);
                zip.TryWriteFile(workspace, Media.EndAdventureFile, Media.EndAdventureFile);
                zip.TryWriteFile(workspace, Media.EndMovieFile, Media.EndMovieFile);
            }

            // write soundbank
            if (options.ExportSoundBank)
                if (SoundBank != 55)
                {
                    using MemoryStream ms = new();
                    MexSoundGroup.ToPackage(workspace.Project.SoundGroups[SoundBank], ms);
                    zip.Write("sound.zip", ms.ToArray());
                }

            // write costumes
            if (options.ExportCostumes)
                foreach (MexCostume c in Costumes)
                {
                    using MemoryStream s = new();
                    c.PackToZip(workspace, s);
                    zip.Write(Path.GetFileNameWithoutExtension(c.File.FileName) + ".zip", s.ToArray());
                }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="zip"></param>
        public static MexInstallerError? FromPackage(MexWorkspace workspace, Stream stream, out MexFighter? fighter)
        {
            fighter = null;

            // load zip
            using ZipArchive zip = new(stream);

            // import fighter from package
            {
                ZipArchiveEntry? entry = zip.GetEntry("fighter.json");
                if (entry == null)
                    return new MexInstallerError("\"fighter.json\" was not found in zip");

                // parse group entry
                fighter = MexJsonSerializer.Deserialize<MexFighter>(entry.Extract());
                if (fighter == null)
                    return new MexInstallerError("Error parsing \"fighter.json\"");

                // load assets
                fighter.Assets.FromPackage(workspace, zip);

                // load media
                {
                    fighter.Media.EndClassicFile = zip.TryReadFile(workspace, fighter.Media.EndClassicFile);
                    fighter.Media.EndAdventureFile = zip.TryReadFile(workspace, fighter.Media.EndAdventureFile);
                    fighter.Media.EndAllStarFile = zip.TryReadFile(workspace, fighter.Media.EndAllStarFile);
                    fighter.Media.EndMovieFile = zip.TryReadFile(workspace, fighter.Media.EndMovieFile);
                }

                // init defaults
                fighter.TargetTestStage = MexStageIDConverter.ToExternalID(40);
                fighter.VictoryTheme = 10;
                fighter.FighterMusic1 = 0;
                fighter.FighterMusic2 = 0;
                fighter.AnnouncerCall = 540000;
                fighter.ClassicTrophyId = 0;
                fighter.AdventureTrophyId = 1;
                fighter.AllStarTrophyId = 2;
            }

            // load soundbank
            {
                ZipArchiveEntry? entry = zip.GetEntry("sound.zip");

                if (entry != null)
                {
                    using MemoryStream ms = new(entry.Extract());
                    MexSoundGroup.FromPackage(workspace, ms, out MexSoundGroup? group);
                    if (group != null)
                    {
                        fighter.SoundBank = workspace.Project.AddSoundGroup(group);
                    }
                    else
                    {
                        fighter.SoundBank = 55;
                    }
                }
                else
                {
                    fighter.SoundBank = 55;
                }
            }

            // import costumes
            List<string> costumes = fighter.Costumes.Select(e => e.File.FileName).ToList();
            fighter.Costumes.Clear();
            foreach (string? costume in costumes)
            {
                ZipArchiveEntry? entry = zip.GetEntry(Path.GetFileNameWithoutExtension(costume) + ".zip");
                if (entry != null)
                {
                    using MemoryStream cstream = new(entry.Extract());
                    System.Text.StringBuilder log = new();
                    foreach (MexCostume c in MexCostume.FromZip(workspace, cstream, log))
                        fighter.Costumes.Add(c);
                }
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        public void Delete(MexWorkspace workspace)
        {
            //Files.Delete(workspace); // TODO: delete files that are not used by other fighter
            Assets.Delete(workspace);
            Media.Delete(workspace);

            foreach (MexCostume c in Costumes)
            {
                c.DeleteAssets(workspace);
                //c.DeleteFiles(workspace);// TODO: delete files that are not used by other fighter
            }
        }
    }
}
