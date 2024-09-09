using System.ComponentModel;
using HSDRaw.Common;
using HSDRaw;
using HSDRaw.Melee;
using HSDRaw.MEX;
using mexLib.Attributes;
using mexLib.Installer;
using mexLib.MexScubber;
using System.Collections.ObjectModel;

namespace mexLib.Types
{
    public partial class MexFighter
    {
        [Category("0 - General"), DisplayName("Name")]
        public string Name { get; set; } = "New Fighter";

        [Category("0 - General"), DisplayName("Series")]
        [MexLink(MexLinkType.Series)]
        public int SeriesID { get; set; } = 0;

        [Category("0 - General"), DisplayName("Can Wall Jump"), Description("Determines if fighter can wall jump")]
        public bool CanWallJump { get; set; }

        [Category("0 - General"), DisplayName("Sub-Fighter"), Description("The fighter associated with this fighter (Sheik/Zelda and Ice Climbers)")]
        public int SubCharacter { get; set; }

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
        public SBM_BoneLookupTable BoneTable { get; set; } = new SBM_BoneLookupTable();

        [Browsable(false)]
        public SBM_PlCoUnknownFighterTable? UnkTable { get; set; } = new SBM_PlCoUnknownFighterTable();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mexData"></param>
        /// <param name="workspace"></param>
        public void ToMxDt(MexGenerator mex, int internalId)
        {
            var mexData = mex.Data;
            var externalId = MexFighterIDConverter.ToExternalID(internalId, mex.Workspace.Project.Fighters.Count);
            var kb = mexData.KirbyData;
            var fd = mexData.FighterData;

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

            fd.VictoryThemeIDs[externalId] = (int)VictoryTheme;
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
            if (HasKirbyCostumes)
            {
                kb.KirbyCostumes.Set(internalId, new MEX_KirbyCostume() 
                { 
                    Array = Costumes.Select(e => new MEX_CostumeFileSymbol()
                    {
                        FileName = e.KirbyFile.FileName,
                        JointSymbol = e.KirbyFile.JointSymbol,
                        MatAnimSymbol = e.KirbyFile.MaterialSymbol,
                    }).ToArray() 
                });
                kb.CostumeRuntime._s.SetReference(internalId * 4, new HSDAccessor() { _s = new HSDStruct(Costumes.Count * 8) });
            }
            else
            {
                kb.KirbyCostumes.Set(internalId, null);
                kb.CostumeRuntime._s.SetReference(internalId * 4, null);
            }

            // Functions
            Functions.ToMxDt(mexData, internalId);

            // save items
            var itemEntries = new ushort[Items.Count];
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
        /// <param name="dol"></param>
        /// <param name="index"></param>
        public void FromDOL(MexDOL dol, uint index, SBM_ftLoadCommonData plco)
        {
            // load data from plco
            BoneTable = plco.BoneTables[(int)index];
            UnkTable = plco.FighterTable[(int)index];

            // get external id
            var exid = (uint)MexFighterIDConverter.ToExternalID((int)index, 0x21);

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
                var demoOffset = dol.GetStruct<uint>(0x803C2468, index);
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
            var effect_id = dol.GetStruct<byte>(0x803C26FC, index);
            var kirby_effect_id = dol.GetStruct<byte>(0x803CB46C, index);

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

            // Functions
            Functions.FromDOL(dol, index);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
