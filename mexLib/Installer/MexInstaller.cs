using HSDRaw;
using HSDRaw.Common;
using HSDRaw.Melee;
using HSDRaw.Melee.Mn;
using HSDRaw.MEX.Scenes;
using MeleeMedia.Audio;
using mexLib.MexScubber;
using mexLib.Types;

namespace mexLib.Installer
{
    public class MexInstallerError
    {
        public string Message { get; internal set; }

        public MexInstallerError(string message) { Message = message; }
    }

    public class MexInstaller
    {
        /// <summary>
        /// Resets all fighters move logic pointers
        /// </summary>
        /// <param name="workspace"></param>
        public static void CorrectFixMoveLogicPointers(MexWorkspace workspace)
        {
            MexDOL dol = new MexDOL(workspace.GetDOL());
            Dictionary<uint, (uint, uint)> onloadToLogic = new ();
            for (uint i = 0; i < 0x21; i++)
            {
                onloadToLogic.Add(dol.GetStruct<uint>(0x803C1154, i), 
                    (dol.GetStruct<uint>(0x803C12E0, i), dol.GetStruct<uint>(0x803C1364, i)));
            }

            foreach (var f in workspace.Project.Fighters)
            {
                if (f.Functions.MoveLogicPointer == 0 &&
                    onloadToLogic.TryGetValue(f.Functions.OnLoad, out (uint, uint) p))
                {
                    f.Functions.MoveLogicPointer = p.Item1;
                    f.Functions.DemoMoveLogicPointer = p.Item2;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="dol"></param>
        public static MexInstallerError? Install(MexWorkspace workspace, MexDOL dol)
        {
            MexProject project = workspace.Project;

            // check and apply dol patch
            if (!dol.ApplyPatch())
                return new MexInstallerError("Failed to apply DOL Patch");

            // get files
            string plcoPath = workspace.GetFilePath("PlCo.dat");
            if (!File.Exists(plcoPath))
                return new MexInstallerError("PlCo.dat not found");
            HSDRawFile plcoFile = new(plcoPath);
            SBM_ftLoadCommonData? plco = plcoFile["ftLoadCommonData"].Data as SBM_ftLoadCommonData;
            if (plco == null)
                return new MexInstallerError("Error reading PlCo.dat");

            // init menu playlist
            project.MenuPlaylist.Entries.Add(new MexPlaylistEntry()
            {
                MusicID = 0x34,
                ChanceToPlay = 75,
            });
            project.MenuPlaylist.Entries.Add(new MexPlaylistEntry()
            {
                MusicID = 0x36,
                ChanceToPlay = 25,
            });

            // create series
            foreach (MexSeries s in MexDefaultData.GenerateDefaultSeries())
                project.Series.Add(s);

            // load fighters
            for (uint i = 0; i < 0x21; i++)
            {
                MexFighter fighter = new();
                fighter.FromDOL(dol, i);
                fighter.LoadFromPlCo(plco, i);
                project.Fighters.Add(fighter);
            }

            // load items 0x2b + 0x76 + 0x2F + 0x1D
            //project.CommonItems = Enumerable.Range(0, 0x2b)
            //    .Select(i => new MEX_Item
            //    {
            //        _s = new HSDRaw.HSDStruct(dol.GetData(0x803F14C4 + 60 * (uint)i, 60))
            //    }).ToArray();
            //project.FighterItems = Enumerable.Range(0, 0x76)
            //    .Select(i => new MEX_Item
            //    {
            //        _s = new HSDRaw.HSDStruct(dol.GetData(0x803F3100 + 60 * (uint)i, 60))
            //    }).ToArray();
            //project.PokemonItems = Enumerable.Range(0, 0x2F)
            //    .Select(i => new MEX_Item
            //    {
            //        _s = new HSDRaw.HSDStruct(dol.GetData(0x803F23CC + 60 * (uint)i, 60))
            //    }).ToArray();
            //project.StageItems = Enumerable.Range(0, 0x1D)
            //    .Select(i => new MEX_Item
            //    {
            //        _s = new HSDRaw.HSDStruct(dol.GetData(0x803F4D20 + 60 * (uint)i, 60))
            //    }).ToArray();

            // load music
            for (uint i = 0; i < 98; i++)
            {
                MexMusic music = new();
                music.FromDOL(dol, i);
                project.Music.Add(music);
            }

            // load stages
            for (uint i = 0; i < 71; i++)
            {
                MexStage stage = new();
                stage.FromDOL(dol, i);
                project.Stages.Add(stage);
            }

            // load sounds
            for (uint i = 0; i < 55; i++)
            {
                MexSoundGroup sound = new();
                sound.FromDOL(dol, i);
                project.SoundGroups.Add(sound);
            }

            // create a null bank
            project.SoundGroups.Add(new MexSoundGroup()
            {
                FileName = "null.ssm",
            });
            new SSM() { Name = "null.ssm", StartIndex = 5000 }.Save(workspace.GetFilePath("audio/us/null.ssm"));

            // load scenes
            project.SceneData = InstallScenes(dol);

            // load ifalldata
            InstallStockIcons(workspace);

            // extract css
            project.CharacterSelect.FromDOL(dol);
            InstallCSS(workspace);

            // extract sss
            MexStageSelect sss = new() { Name = "Melee" };
            sss.FromDOL(dol);
            project.StageSelects.Add(sss);
            InstallSSS(workspace);

            // extract result screen
            InstallResultScreen(workspace);

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        private static MexInstallerError? InstallStockIcons(MexWorkspace workspace)
        {
            string ifAllPath = workspace.GetFilePath(@"IfAll.dat");
            if (!File.Exists(ifAllPath))
                return new MexInstallerError("IfAll.dat not found");

            HSDRawFile ifAllFile = new(ifAllPath);

            // red dot is frame 185 of single menu model 53
            // rest are in Stc_scemdls joint 1
            // external id (sort of) order
            // stride of 30
            //      19-24 get subtracted by 1
            //      18 is 25

            //      26 is smash logo
            //      27 master hand
            //      28 crazy hand
            //      57 target
            //      58 giga bowser
            //      59 sandbag
            //      124 is blank
            if (ifAllFile["Stc_scemdls"].Data is HSDNullPointerArrayAccessor<HSD_JOBJDesc> stc)
            {
                HSDRaw.Common.Animation.HSD_MatAnimJoint joint = stc[0].MaterialAnimations[0].TreeList[1];
                HSDRaw.Common.Animation.HSD_TexAnim anim = joint.MaterialAnimation.TextureAnimation;
                HSD_TOBJ[] tobjs = anim.ToTOBJs();
                List<HSDRaw.Tools.FOBJKey> keys = anim.AnimationObject.FObjDesc.GetDecodedKeys();

                // get resereved icons
                int[] reserved = { 180, 26, 27, 28, 57, 58, 59 };
                for (int i = 0; i < reserved.Length; i++)
                {
                    HSDRaw.Tools.FOBJKey? key = keys.Find(e => e.Frame == reserved[i]);
                    if (key != null)
                        workspace.Project.ReservedAssets.IconsAssets[i].SetFromMexImage(workspace, new MexImage(tobjs[(int)key.Value]));
                }
                // this icon exists in mnslchr, but I'm store it manually
                workspace.Project.ReservedAssets.IconsAssets[^1].SetFromMexImage(workspace, MexImage.FromByteArray(MexDefaultData.SinglePlayerIcon));

                // get fighter icons
                int internalId = 0;
                foreach (MexFighter f in workspace.Project.Fighters)
                {
                    if (internalId > 26 || internalId == 11) // 11 is nana
                    {
                        internalId++;
                        continue;
                    }

                    int externalId = MexFighterIDConverter.ToExternalID(internalId, workspace.Project.Fighters.Count);
                    if (internalId == 7)
                    {
                        externalId = 25;
                    }
                    else
                    if (externalId == 26)
                    {
                    }
                    else if (externalId >= 19)
                    {
                        externalId -= 1;
                    }

                    for (int i = 0; i < f.Costumes.Count; i++)
                    {
                        HSDRaw.Tools.FOBJKey? k = keys.Find(e => e.Frame == externalId + (i * 30));
                        if (k != null)
                        {
                            f.Costumes[i].IconAsset.SetFromMexImage(workspace, new MexImage(tobjs[(int)k.Value]));
                        }
                    }

                    internalId++;
                }
            }

            // extract emblems
            HSDNullPointerArrayAccessor<HSD_JOBJDesc>? dmgmrk = ifAllFile["DmgMrk_scene_models"].Data as HSDNullPointerArrayAccessor<HSD_JOBJDesc>;
            if (dmgmrk != null)
            {
                int[] series_order = { 0, 1, 2, 3, 9, 4, 5, 6, 8, 7, 10, 13, 11, 12, 14, 15 };
                HSD_TOBJ[] emblem_matanim_joint = dmgmrk[0].MaterialAnimations[0].Child.MaterialAnimation.TextureAnimation.ToTOBJs();
                for (int i = 0; i < series_order.Length; i++)
                {
                    MexImage img = new(emblem_matanim_joint[series_order[i]]);
                    workspace.Project.Series[i].IconAsset.SetFromMexImage(workspace, img);
                }
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        private static MexInstallerError? InstallSSS(MexWorkspace workspace)
        {
            string sssFile = workspace.GetFilePath("MnSlMap.usd");
            if (!File.Exists(sssFile))
                return new MexInstallerError("MnSlMap.usd not found");

            HSDRawFile file = new(sssFile);
            SBM_MnSelectStageDataTable? dataTable = file["MnSelectStageDataTable"].Data as SBM_MnSelectStageDataTable;
            if (dataTable == null)
                return new MexInstallerError("Error reading MnSlMap.usd");

            // get model animation
            int off1 = 1;
            int off2 = 20;
            int off3 = 14;
            int off_random = 17;
            List<HSD_JOBJ> position_joints = dataTable.PositionModel.TreeList;
            List<HSDRaw.Common.Animation.HSD_AnimJoint> position_animjoints = dataTable.PositionAnimation.TreeList;

            // extract textures
            HSD_TOBJ[] tex0 = dataTable.IconDoubleMatAnimJoint.Child.Next.MaterialAnimation.Next.TextureAnimation.ToTOBJs();
            HSD_TOBJ[] tex0_extra = dataTable.IconDoubleMatAnimJoint.Child.MaterialAnimation.Next.TextureAnimation.ToTOBJs();
            HSD_TOBJ[] tex1 = dataTable.IconLargeMatAnimJoint.Child.MaterialAnimation.Next.TextureAnimation.ToTOBJs();
            HSD_TOBJ[] tex2 = dataTable.IconSpecialMatAnimJoint.Child.MaterialAnimation.Next.TextureAnimation.ToTOBJs();

            HSD_TOBJ[] nameTOBJs = dataTable.StageNameMatAnimJoint.Child.Child.MaterialAnimation.TextureAnimation.ToTOBJs();

            // get random
            //workspace.Project.StageSelects[0].RandomIcon.FromJoint(0, position_joints[off_random], position_animjoints[off_random]);
            MexReservedAssets reserved = workspace.Project.ReservedAssets;
            reserved.SSSNullAsset.SetFromMexImage(workspace, new MexImage(tex0[0]));
            reserved.SSSLockedNullAsset.SetFromMexImage(workspace, new MexImage(tex0[1]));
            reserved.SSSRandomBannerAsset.SetFromMexImage(workspace, new MexImage(nameTOBJs[^1]));

            // messy
            foreach (MexStageSelectIcon icon in workspace.Project.StageSelects[0].StageIcons)
            {
                if (icon.StageID == 0)
                {
                    icon.FromJoint(0, position_joints[off_random], position_animjoints[off_random]);
                    continue;
                }

                MexStage stage = workspace.Project.Stages[MexStageIDConverter.ToInternalID(icon.StageID)];

                // deswizzle icon
                int index = icon.PreviewID;
                HSD_TOBJ? stage_icon;
                if (index < 22) // double icons
                {
                    if (index % 2 == 0)
                    {
                        stage_icon = tex0[index / 2 + 2];
                        icon.FromJoint(index / 2 + off1, position_joints[index / 2 + off1], position_animjoints[index / 2 + off1]);
                    }
                    else
                    {
                        stage_icon = tex0_extra[index / 2 + 2];
                        icon.FromJoint(index / 2 + off1, position_joints[index / 2 + off1], position_animjoints[index / 2 + off1]);
                        icon.Y -= 5.6f;
                        icon.Z -= 0.5f;
                    }
                    icon.ScaleX = 1;
                    icon.ScaleY = 1;
                }
                else if (index < 24) // single icons
                {
                    stage_icon = tex1[index - 24 + 4];
                    icon.FromJoint(index - 24 + off2, position_joints[index - 24 + off2], position_animjoints[index - 24 + off2]);
                    icon.ScaleX = 1.0f;
                    icon.ScaleY = 1.1f;
                }
                else // small icons
                {
                    stage_icon = tex2[index - 26 + 4];
                    icon.FromJoint(index - 26 + off3, position_joints[index - 26 + off3], position_animjoints[index - 26 + off3]);
                    icon.ScaleX = 0.8f;
                    icon.ScaleY = 0.8f;
                }

                // set icon
                if (stage_icon != null)
                    stage.Assets.IconAsset.SetFromMexImage(workspace, new MexImage(stage_icon));

                // set banner
                stage.Assets.BannerAsset.SetFromMexImage(workspace, new MexImage(nameTOBJs[icon.PreviewID]));
            }

            // TODO: extract 2d series assets

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public static MexInstallerError? InstallResultScreen(MexWorkspace workspace)
        {
            string rstFile = workspace.GetFilePath("GmRst.usd");
            if (!File.Exists(rstFile))
                return new MexInstallerError("GmRst.usd not found");

            HSDRawFile file = new(rstFile);
            HSD_SOBJ? pnlsce = file["pnlsce"].Data as HSD_SOBJ;
            if (pnlsce == null)
                return new MexInstallerError("Error reading GmRst.usd");

            List<HSDRaw.Common.Animation.HSD_MatAnimJoint> matanim_joints = pnlsce.JOBJDescs[0].MaterialAnimations[0].TreeList;

            // get the nodes to extract from
            HSD_TOBJ[] big_banners = matanim_joints[10].MaterialAnimation.Next.TextureAnimation.ToTOBJs();
            List<HSDRaw.Tools.FOBJKey> big_banner_keys = matanim_joints[10].MaterialAnimation.Next.TextureAnimation.AnimationObject.FObjDesc.GetDecodedKeys();
            HSD_TOBJ[] small_banners = matanim_joints[33].MaterialAnimation.TextureAnimation.ToTOBJs();
            List<HSDRaw.Tools.FOBJKey> small_banners_keys = matanim_joints[33].MaterialAnimation.TextureAnimation.AnimationObject.FObjDesc.GetDecodedKeys();

            // fighter assets are external with sheik at end (>=19 0-=1) (=19->29)
            for (int internalId = 0; internalId < workspace.Project.Fighters.Count; internalId++) // 26
            {
                // get fighter  external id
                int externalId = MexFighterIDConverter.ToExternalID(internalId, 0x21);

                // sheik hack
                if (externalId == 19)
                    externalId = 25;
                else if (externalId > 19)
                    externalId -= 1;

                // search and set asset for fighter
                MexFighter fighter = workspace.Project.Fighters[internalId];
                {
                    HSDRaw.Tools.FOBJKey? key = big_banner_keys.Find(e => e.Frame == externalId);
                    if (key != null)
                        fighter.Assets.ResultBannerBigAsset.SetFromMexImage(workspace, new MexImage(big_banners[(int)key.Value]));
                }
                {
                    HSDRaw.Tools.FOBJKey? key = small_banners_keys.Find(e => e.Frame == externalId);
                    if (key != null)
                        fighter.Assets.ResultBannerSmallAsset.SetFromMexImage(workspace, new MexImage(small_banners[(int)key.Value]));
                }
            }

            // additional assets red 181, blue 182, green 183, nocontest 184
            {
                HSDRaw.Tools.FOBJKey? key = big_banner_keys.Find(e => e.Frame == 181);
                if (key != null)
                    workspace.Project.ReservedAssets.RstRedTeamAsset.SetFromMexImage(workspace, new MexImage(big_banners[(int)key.Value]));
            }
            {
                HSDRaw.Tools.FOBJKey? key = big_banner_keys.Find(e => e.Frame == 182);
                if (key != null)
                    workspace.Project.ReservedAssets.RstBlueTeamAsset.SetFromMexImage(workspace, new MexImage(big_banners[(int)key.Value]));
            }
            {
                HSDRaw.Tools.FOBJKey? key = big_banner_keys.Find(e => e.Frame == 183);
                if (key != null)
                    workspace.Project.ReservedAssets.RstGreenTeamAsset.SetFromMexImage(workspace, new MexImage(big_banners[(int)key.Value]));
            }
            {
                HSDRaw.Tools.FOBJKey? key = big_banner_keys.Find(e => e.Frame == 184);
                if (key != null)
                    workspace.Project.ReservedAssets.RstNoContestAsset.SetFromMexImage(workspace, new MexImage(big_banners[(int)key.Value]));
            }

            // extract 3d series emblems
            HSD_SOBJ? flmsce = file["flmsce"].Data as HSD_SOBJ;
            if (flmsce == null)
                return new MexInstallerError("Error reading GmRst.usd");
            HSD_JOBJ[] emblem_joints = flmsce.JOBJDescs[0].RootJoint.TreeList[5].Children;

            for (int i = 0; i < emblem_joints.Length; i++)
            {
                if (i < workspace.Project.Series.Count)
                {
                    if (emblem_joints[i].Dobj != null)
                        workspace.Project.Series[i].ModelAsset.SetFromDObj(workspace, emblem_joints[i].Dobj);
                }
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        private static MexInstallerError? InstallCSS(MexWorkspace workspace)
        {
            string cssFile = workspace.GetFilePath("MnSlChr.usd");
            if (!File.Exists(cssFile))
                return new MexInstallerError("MnSlChr.usd not found");

            HSDRawFile file = new(cssFile);
            SBM_SelectChrDataTable? dataTable = file["MnSelectChrDataTable"].Data as SBM_SelectChrDataTable;
            if (dataTable == null)
                return new MexInstallerError("Error reading MnSlChr.usd");

            // extract csps
            string cspFolder = workspace.GetAssetPath("csp/");
            Directory.CreateDirectory(cspFolder);

            // stride is 30
            // external id order
            HSDRaw.Common.Animation.HSD_TexAnim portrait_anim = dataTable.PortraitMaterialAnimation.TreeList[6].MaterialAnimation.TextureAnimation;
            List<HSDRaw.Tools.FOBJKey> keys = portrait_anim.AnimationObject.FObjDesc.GetDecodedKeys();
            foreach (HSDRaw.Tools.FOBJKey? k in keys)
                if ((k.Frame % 30) >= 19)
                    k.Frame++;
            HSD_TOBJ[] tobjs = portrait_anim.ToTOBJs();
            for (int internalId = 0; internalId < 0x21; internalId++)
            {
                // get figher from external id
                int fighterId = MexFighterIDConverter.ToInternalID(internalId, 0x21);

                if (internalId > 26)
                    continue;

                MexFighter fighter = workspace.Project.Fighters[fighterId];

                for (int j = 0; j < fighter.Costumes.Count; j++)
                {
                    // get key on this frame
                    HSDRaw.Tools.FOBJKey? k = keys.Find(e => e.Frame == internalId + 30 * j);
                    if (k != null)
                    {
                        fighter.Costumes[j].CSPAsset.SetFromMexImage(workspace, new MexImage(tobjs[(int)k.Value]));
                    }
                }
            }

            // extract fighter icons
            List<HSD_JOBJ> single_joint = dataTable.SingleMenuModel.TreeList;
            List<HSDRaw.Common.Animation.HSD_AnimJoint> single_anim = dataTable.SingleMenuAnimation.TreeList;
            List<HSDRaw.Common.Animation.HSD_MatAnimJoint> single_matanim = dataTable.SingleMenuMaterialAnimation.TreeList;
            // 16 - 34
            // 16mario, 17luigi, 18bowser, 19peach, 20yoshi, 21dk, 22falcon
            // 23fox, 24ness, 25climbers, 26kirby, 27samus, 28zelda, 29link
            // 30pikachu, 31jigglypuff, 32mewtwo, 33game, 34marth
            // 4dr, 6ganon, 8falco, 10younlink, 12pichu, 14roy
            int[] internal_to_joint_index = new int[] {
                16, 23, 22, 21, 26, 18, 29, -1, 24, 19,
                25, -1, 30, 27, 20, 31, 32, 17, 34, 28,
                10, 4, 8, 12, 33, 6, 14
            };
            for (int i = 0; i < internal_to_joint_index.Length; i++)
            {
                if (internal_to_joint_index[i] == -1)
                    continue;

                int externalId = MexFighterIDConverter.ToExternalID(i, workspace.Project.Fighters.Count);
                MexFighter fighter = workspace.Project.Fighters[i];

                int joint_index = internal_to_joint_index[i];
                HSD_JOBJ joint = single_joint[joint_index];

                // get icon position
                MexCharacterSelectIcon? icon = workspace.Project.CharacterSelect.FighterIcons.FirstOrDefault(e => e.Fighter == externalId);
                if (icon != null)
                {
                    icon.X = joint.TX + 3.4f;
                    icon.Y = joint.TY - 3.5f;
                    icon.Z = joint.TZ;

                    // clone fighters....
                    if (joint_index == 4 ||
                        joint_index == 6 ||
                        joint_index == 8 ||
                        joint_index == 10 ||
                        joint_index == 12 ||
                        joint_index == 14)
                    {
                        HSD_JOBJ cloneJoint = single_joint[joint_index - 1];
                        HSDRaw.Common.Animation.HSD_AnimJoint anim = single_anim[joint_index - 1];
                        List<HSDRaw.Tools.FOBJKey> trax = anim.AOBJ.FObjDesc.GetDecodedKeys();
                        List<HSDRaw.Tools.FOBJKey> tray = anim.AOBJ.FObjDesc.Next.GetDecodedKeys();
                        icon.X = trax[^1].Value + 3.4f;
                        icon.Y = tray[^1].Value - 3.5f;
                        icon.Z = cloneJoint.TZ;
                    }
                }

                // zelda and blank asset
                if (internal_to_joint_index[i] == 28)
                {
                    HSDRaw.Common.Animation.HSD_MatAnimJoint matanim = single_matanim[internal_to_joint_index[i]];

                    MexImage image = new(matanim.MaterialAnimation.Next.TextureAnimation.ToTOBJs()[0]);
                    fighter.Assets.CSSIconAsset.SetFromMexImage(workspace, image);

                    // extract reserved assets
                    workspace.Project.ReservedAssets.CSSNullAsset.SetFromMexImage(workspace, new MexImage(joint.Dobj.Next.Mobj.Textures));
                    workspace.Project.ReservedAssets.CSSBackAsset.SetFromMexImage(workspace, new MexImage(joint.Dobj.Mobj.Textures));
                }
                else
                {
                    fighter.Assets.CSSIconAsset.SetFromMexImage(workspace, new MexImage(joint.Dobj.Next.Mobj.Textures));
                }
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        private static readonly int[] MajorSceneMinorCounts = new int[] { 2, 9, 26, 49, 29, 16, 2, 2, 2, 5, 2, 2, 2, 3, 3, 9, 9, 9, 9, 4, 2, 5, 5, 5, 7, 13, 3, 8, 4, 9, 9, 4, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 9, 3, 9, 0 };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dol"></param>
        /// <returns></returns>
        public static MEX_SceneData InstallScenes(MexDOL dol)
        {
            // Scenes
            MEX_SceneData scene = new()
            {
                MajorScenes = new HSDArrayAccessor<MEX_MajorScene>(),
                MinorSceneFunctions = new HSDArrayAccessor<MEX_MinorFunctionTable>(),
            };

            // MajorScenes - 0x803DACA4 0x2E
            for (uint i = 0; i < 0x2E; i++)
            {
                uint minorScenePointer = dol.GetStruct<uint>(0x803DACA4 + 0x10, i, 0x14);

                MEX_MinorScene[] minorScenes = new MEX_MinorScene[MajorSceneMinorCounts[i]];
                for (uint j = 0; j < MajorSceneMinorCounts[i]; j++)
                {
                    minorScenes[j] = new MEX_MinorScene()
                    {
                        MinorSceneID = dol.GetStruct<byte>(minorScenePointer + 0x00, j, 0x18),
                        PersistantHeapCount = dol.GetStruct<byte>(minorScenePointer + 0x01, j, 0x18),
                        ScenePrepFunction = (int)dol.GetStruct<uint>(minorScenePointer + 0x04, j, 0x18),
                        SceneDecideFunction = (int)dol.GetStruct<uint>(minorScenePointer + 0x08, j, 0x18),
                        CommonMinorID = dol.GetStruct<byte>(minorScenePointer + 0x0C, j, 0x18),
                        StaticStruct1 = (int)dol.GetStruct<uint>(minorScenePointer + 0x10, j, 0x18),
                        StaticStruct2 = (int)dol.GetStruct<uint>(minorScenePointer + 0x14, j, 0x18),
                    };
                }

                scene.MajorScenes.Add(new MEX_MajorScene()
                {
                    Preload = dol.GetStruct<byte>(0x803DACA4 + 0x00, i, 0x14) != 0,
                    MajorSceneID = dol.GetStruct<byte>(0x803DACA4 + 0x01, i, 0x14),
                    LoadFunction = (int)dol.GetStruct<uint>(0x803DACA4 + 0x04, i, 0x14),
                    UnloadFunction = (int)dol.GetStruct<uint>(0x803DACA4 + 0x08, i, 0x14),
                    OnBootFunction = (int)dol.GetStruct<uint>(0x803DACA4 + 0x0C, i, 0x14),
                    MinorScene = new HSDArrayAccessor<MEX_MinorScene>() { Array = minorScenes },
                    FileName = null,
                });
            }

            // MinorSceneFunctions - 0x803DA920 0x2D
            for (uint i = 0; i < 0x2D; i++)
            {
                scene.MinorSceneFunctions.Add(new MEX_MinorFunctionTable()
                {
                    MinorSceneID = dol.GetStruct<byte>(0x803DA920 + 0x00, i, 0x14),
                    SceneThink = (int)dol.GetStruct<uint>(0x803DA920 + 0x04, i, 0x14),
                    SceneLoad = (int)dol.GetStruct<uint>(0x803DA920 + 0x08, i, 0x14),
                    SceneLeave = (int)dol.GetStruct<uint>(0x803DA920 + 0x0C, i, 0x14),
                    FileName = "",
                });
            }

            return scene;
        }
    }
}
