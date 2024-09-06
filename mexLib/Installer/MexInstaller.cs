using HSDRaw;
using HSDRaw.Common;
using HSDRaw.Melee;
using HSDRaw.Melee.Mn;
using HSDRaw.MEX.Misc;
using HSDRaw.MEX.Scenes;
using MeleeMedia.Audio;
using mexLib.MexScubber;
using System.Drawing;
using mexLib.Types;
using System.Diagnostics;
using System.Security.Cryptography;

namespace mexLib.Installer
{
    public class MexInstallerError
    {
        public string Message { get; internal set; }

        public MexInstallerError(string message) { this.Message = message; }
    }

    public class MexInstaller
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="dol"></param>
        public static MexInstallerError? Install(MexWorkspace workspace, MexDOL dol)
        {
            var project = workspace.Project;

            // check and apply dol patch
            if (!dol.ApplyPatch())
                return new MexInstallerError("Failed to apply DOL Patch");

            // TODO: load codes

            // get files
            var plcoPath = workspace.GetFilePath("PlCo.dat");
            if (!File.Exists(plcoPath))
                return new MexInstallerError("PlCo.dat not found");
            HSDRawFile plcoFile = new (plcoPath);
            var plco = plcoFile["ftLoadCommonData"].Data as SBM_ftLoadCommonData;
            if (plco == null)
                return new MexInstallerError("Error reading PlCo.dat");

            var semPath = workspace.GetFilePath(@"audio//us//smash2.sem");
            if (!File.Exists(semPath))
                return new MexInstallerError("smash2.sem not found");
            var sem = SEM.ReadSEMFile(semPath);

            var smstPath = workspace.GetFilePath(@"SmSt.dat");
            if (!File.Exists(smstPath))
                return new MexInstallerError("SmSt.dat not found");
            HSDRawFile smstFile = new(smstPath);
            var smst = smstFile["smSoundTestLoadData"].Data as smSoundTestLoadData;
            if (smst == null)
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
            foreach (var s in MexDefaultData.GenerateDefaultSeries())
                project.Series.Add(s);

            // load fighters
            for (uint i = 0; i < 0x21; i++)
            {
                var fighter = new MexFighter();
                fighter.FromDOL(dol, i, plco);
                project.Fighters.Add(fighter);

                // load data from plco
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
                var music = new MexMusic();
                music.FromDOL(dol, i);
                project.Music.Add(music);
            }

            // load stages
            for (uint i = 0; i < 71; i++)
            {
                var stage = new MexStage();
                stage.FromDOL(dol, i);
                project.Stages.Add(stage);
            }

            // load sounds
            var soundNames = smst.SoundNames;
            var soundids = smst.SoundIDs.ToList();
            for (uint i = 0; i < 55; i++)
            {
                var sound = new MexSoundbank();
                sound.FromDOL(dol, i);
                sound.ScriptBank = sem[(int)i];

                // extract ssm
                var ssm = new SSM();
                ssm.Open(workspace.GetFilePath($"audio//us//{sound.FileName}"));

                //// export ssm files
                //var ssmPath = workspace.GetAssetPath($"audio//{i:D3}//");
                //Directory.CreateDirectory( ssmPath );
                //int si = 0;
                //foreach (var s in ssm.Sounds)
                //{
                //    s.ExportFormat(ssmPath + $"{si++:D3}.dsp");
                //}

                // load script meta data
                for (int j = 0; j < sound.ScriptBank.Scripts.Length; j++)
                {
                    // load script name
                    var sindex = soundids.IndexOf((int)(i * 10000 + j));
                    if (sindex != -1 && sindex < soundNames.Length)
                    {
                        sound.ScriptBank.Scripts[j].Name = soundNames[sindex];

                        // adjust sound id to relative
                        if (sound.ScriptBank.Scripts[j].SFXID != -1)
                            sound.ScriptBank.Scripts[j].SFXID -= ssm.StartIndex;
                    }

                }

                project.Soundbanks.Add(sound);
            }

            // create a null bank
            project.Soundbanks.Add(new MexSoundbank()
            {
                FileName = "null.ssm",
            });
            new SSM() { Name = "null.ssm", StartIndex = 540000 }.Save(workspace.GetFilePath("audio\\us\\null.ssm"));

            // load scenes
            project.SceneData = InstallScenes(dol);

            // load ifalldata
            InstallStockIcons(workspace);

            // extract css
            project.CharacterSelect.FromDOL(dol);
            InstallCSS(workspace);

            // extract sss
            var sss = new MexStageSelect();
            sss.FromDOL(dol);
            project.StageSelects.Add(sss);
            InstallSSS(workspace);

            // load misc
            InstallMisc(project);

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        private static MexInstallerError? InstallStockIcons(MexWorkspace workspace)
        {
            var ifAllPath = workspace.GetFilePath(@"IfAll.dat");
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
                var joint = stc[0].MaterialAnimations[0].TreeList[1];
                var anim = joint.MaterialAnimation.TextureAnimation;
                var tobjs = anim.ToTOBJs();
                var keys = anim.AnimationObject.FObjDesc.GetDecodedKeys();

                // get resereved icons
                int[] reserved = { 124, 26, 27, 28, 57, 58, 59 };
                for (int i = 0; i < reserved.Length; i++)
                {
                    workspace.Project.ReservedAssets.IconsAssets[i].SetFromMexImage(workspace, new MexImage(tobjs[(int)keys[reserved[i]].Value]));
                }
                // this icon exists in mnslchr, but I'm store it manually
                workspace.Project.ReservedAssets.IconsAssets[^1].SetFromMexImage(workspace, MexImage.FromByteArray(MexDefaultData.SinglePlayerIcon));

                // get fighter icons
                int internalId = 0;
                foreach (var f in workspace.Project.Fighters)
                {
                    int externalId = MexFighterIDConverter.ToExternalID(internalId, workspace.Project.Fighters.Count);
                    if (externalId == 18)
                        externalId = 25;
                    else if (externalId >= 19)
                        externalId -= 1;

                    for (int i = 0; i < f.Costumes.Costumes.Count; i++)
                    {
                        var k = keys.Find(e => e.Frame == externalId + (i * 30));
                        if (k != null)
                        {
                            f.Costumes.Costumes[i].IconAsset.SetFromMexImage(workspace, new MexImage(tobjs[(int)k.Value]));
                        }
                    }

                    internalId++;
                }
            }

            // extract emblems
            var dmgmrk = ifAllFile["DmgMrk_scene_models"].Data as HSDNullPointerArrayAccessor<HSD_JOBJDesc>;
            if (dmgmrk != null)
            {
                int[] series_order = { 0, 1, 2, 3, 9, 4, 5, 6, 8, 7, 10, 13, 11, 12, 14, 15 };
                var emblem_matanim_joint = dmgmrk[0].MaterialAnimations[0].Child.MaterialAnimation.TextureAnimation.ToTOBJs();
                for (int i = 0; i < series_order.Length; i++)
                {
                    var img = new MexImage(emblem_matanim_joint[series_order[i]]);
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
            var sssFile = workspace.GetFilePath("MnSlMap.usd");
            if (!File.Exists(sssFile))
                return new MexInstallerError("MnSlMap.usd not found");

            var file = new HSDRawFile(sssFile);
            var dataTable = file["MnSelectStageDataTable"].Data as SBM_MnSelectStageDataTable;
            if (dataTable == null)
                return new MexInstallerError("Error reading MnSlMap.usd");

            // create asset folder
            var sssFolder = workspace.GetAssetPath("sss\\");
            Directory.CreateDirectory(sssFolder);

            // get model animation
            int off1 = 1;
            int off2 = 20;
            int off3 = 14;
            int off_random = 17;
            var position_joints = dataTable.PositionModel.TreeList;
            var position_animjoints = dataTable.PositionAnimation.TreeList;

            // extract textures
            var tex0 = dataTable.IconDoubleMatAnimJoint.Child.Next.MaterialAnimation.Next.TextureAnimation.ToTOBJs();
            var tex0_extra = dataTable.IconDoubleMatAnimJoint.Child.MaterialAnimation.Next.TextureAnimation.ToTOBJs();
            var tex1 = dataTable.IconLargeMatAnimJoint.Child.MaterialAnimation.Next.TextureAnimation.ToTOBJs();
            var tex2 = dataTable.IconSpecialMatAnimJoint.Child.MaterialAnimation.Next.TextureAnimation.ToTOBJs();

            var nameTOBJs = dataTable.StageNameMatAnimJoint.Child.Child.MaterialAnimation.TextureAnimation.ToTOBJs();

            // get random
            workspace.Project.StageSelects[0].RandomIcon.FromJoint(position_joints[off_random], position_animjoints[off_random]);
            new MexImage(tex0[0]).Save(workspace.GetAssetPath($"sss\\Null_icon.tex"));
            new MexImage(tex0[1]).Save(workspace.GetAssetPath($"sss\\Locked_icon.tex"));
            new MexImage(nameTOBJs[^1]).Save(workspace.GetAssetPath($"sss\\Random_tag.tex"));

            // messy
            foreach (var icon in workspace.Project.StageSelects[0].StageIcons)
            {
                var name = workspace.Project.Stages[MexStageIDConverter.ToInternalID(icon.StageID)].Name;

                // get icon
                int index = icon.PreviewID;
                HSD_TOBJ? stage_icon;
                if (index < 22) // double icons
                {
                    if (index % 2 == 0)
                    {
                        stage_icon = tex0[index / 2 + 2];
                        icon.FromJoint(position_joints[index / 2 + off1], position_animjoints[index / 2 + off1]);
                    }
                    else
                    {
                        stage_icon = tex0_extra[index / 2 + 2];
                        icon.FromJoint(position_joints[index / 2 + off1], position_animjoints[index / 2 + off1]);
                        icon.Y -= 5.6f;
                        icon.Z -= 0.5f;
                    }
                    icon.ScaleX = 1;
                    icon.ScaleY = 1;
                } else if (index < 24) // single icons
                {
                    stage_icon = tex1[index - 24 + 4];
                    icon.FromJoint(position_joints[index - 24 + off2], position_animjoints[index - 24 + off2]);
                    icon.ScaleX = 1.0f;
                    icon.ScaleY = 1.1f;
                }
                else // small icons
                {
                    stage_icon = tex2[index - 26 + 4];
                    icon.FromJoint(position_joints[index - 26 + off3], position_animjoints[index - 26 + off3]);
                    icon.ScaleX = 0.8f;
                    icon.ScaleY = 0.8f;
                }
                if (stage_icon != null)
                    new MexImage(stage_icon).Save(workspace.GetAssetPath($"sss\\{name}_icon.tex"));

                // get name
                new MexImage(nameTOBJs[icon.PreviewID]).Save(workspace.GetAssetPath($"sss\\{name}_tag.tex"));
            }

            // extract icon model
            var model = dataTable.IconDoubleModel;
            var icon_joint = model.Child.Next;
            model.Child = icon_joint;
            var iconFile = new HSDRawFile();
            iconFile.Roots.Add(new HSDRootNode()
            {
                Name = "icon_joint",
                Data = model
            });
            iconFile.Save(workspace.GetAssetPath("sss\\icon_joint.dat"));

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        private static MexInstallerError? InstallCSS(MexWorkspace workspace)
        {
            var cssFile = workspace.GetFilePath("MnSlChr.usd");
            if (!File.Exists(cssFile))
                return new MexInstallerError("MnSlChr.usd not found");

            var file = new HSDRawFile(cssFile);
            var dataTable = file["MnSelectChrDataTable"].Data as SBM_SelectChrDataTable;
            if (dataTable == null)
                return new MexInstallerError("Error reading MnSlChr.usd");

            // extract csps
            var cspFolder = workspace.GetAssetPath("csp\\");
            Directory.CreateDirectory(cspFolder);

            // stride is 30
            // external id order
            var portrait_anim = dataTable.PortraitMaterialAnimation.TreeList[6].MaterialAnimation.TextureAnimation;
            var keys = portrait_anim.AnimationObject.FObjDesc.GetDecodedKeys();
            foreach (var k in keys)
                if ((k.Frame % 30) >= 19)
                    k.Frame++;
            var tobjs = portrait_anim.ToTOBJs();
            for (int i = 0; i < 0x21; i++)
            {
                // get figher from external id
                int fighterId = MexFighterIDConverter.ToInternalID(i, 0x21);
                var fighter = workspace.Project.Fighters[fighterId];

                for (int j = 0; j < fighter.Costumes.Costumes.Count; j++)
                {
                    // get key on this frame
                    var k = keys.Find(e => e.Frame == i + 30 * j);
                    if (k != null)
                    {
                        var costumeName = Path.GetFileNameWithoutExtension(fighter.Costumes.Costumes[j].File.FileName);
                        var image = new MexImage(tobjs[(int)k.Value]);
                        image.Save(workspace.GetAssetPath($"csp\\{costumeName}.tex"));
                    }
                }
            }

            // extract fighter icons
            var iconFolders = workspace.GetAssetPath("css\\");
            Directory.CreateDirectory(iconFolders);
            var single_joint = dataTable.SingleMenuModel.TreeList;
            var single_anim = dataTable.SingleMenuAnimation.TreeList;
            var single_matanim = dataTable.SingleMenuMaterialAnimation.TreeList;
            // 16 - 34
            // 16mario, 17luigi, 18bowser, 19peach, 20yoshi, 21dk, 22falcon
            // 23fox, 24ness, 25climbers, 26kirby, 27samus, 28zelda, 29link
            // 30pikachu, 31jigglypuff, 32mewtwo, 33game, 34marth
            // 4dr, 6ganon, 8falco, 10younlink, 12pichu, 14roy
            var internal_to_joint_index = new int[] { 
                16, 23, 22, 21, 26, 18, 29, -1, 24, 19,
                25, -1, 30, 27, 20, 31, 32, 17, 34, 28,
                10, 4, 8, 12, 33, 6, 14
            };
            for (int i = 0; i < internal_to_joint_index.Length; i++)
            {
                if (internal_to_joint_index[i] == -1)
                    continue;

                var externalId = MexFighterIDConverter.ToExternalID(i, workspace.Project.Fighters.Count);
                var fighter = workspace.Project.Fighters[i];

                var joint_index = internal_to_joint_index[i];
                var joint = single_joint[joint_index];

                // get icon position
                var icon = workspace.Project.CharacterSelect.FighterIcons.FirstOrDefault(e => e.Fighter == externalId);
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
                        var cloneJoint = single_joint[joint_index - 1];
                        var anim = single_anim[joint_index - 1];
                        var trax = anim.AOBJ.FObjDesc.GetDecodedKeys();
                        var tray = anim.AOBJ.FObjDesc.Next.GetDecodedKeys();
                        icon.X = trax[^1].Value + 3.4f;
                        icon.Y = tray[^1].Value - 3.5f;
                        icon.Z = cloneJoint.TZ;
                    }
                }

                // zelda and blank asset
                if (internal_to_joint_index[i] == 28)
                {
                    var matanim = single_matanim[internal_to_joint_index[i]];

                    var image = new MexImage(matanim.MaterialAnimation.Next.TextureAnimation.ToTOBJs()[0]);
                    image.Save(workspace.GetAssetPath($"css\\{fighter.Name}.tex"));

                    // extrac these assets for later use
                    new MexImage(joint.Dobj.Next.Mobj.Textures).Save(workspace.GetAssetPath($"css\\Null.tex"));
                    new MexImage(joint.Dobj.Mobj.Textures).Save(workspace.GetAssetPath($"css\\Back.tex"));
                }
                else
                {
                    new MexImage(joint.Dobj.Next.Mobj.Textures).Save(workspace.GetAssetPath($"css\\{fighter.Name}.tex"));
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
            var scene = new MEX_SceneData()
            {
                MajorScenes = new HSDArrayAccessor<MEX_MajorScene>(),
                MinorSceneFunctions = new HSDArrayAccessor<MEX_MinorFunctionTable>(),
            };

            // MajorScenes - 0x803DACA4 0x2E
            for (uint i = 0; i < 0x2E; i++)
            {
                var minorScenePointer = dol.GetStruct<uint>(0x803DACA4 + 0x10, i, 0x14);

                var minorScenes = new MEX_MinorScene[MajorSceneMinorCounts[i]];
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private static void InstallMisc(MexProject project)
        {
            project.GawColors = new System.Collections.ObjectModel.ObservableCollection<MEX_GawColor>()
            {
                new () { FillColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00), OutlineColor = Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF) },
                new () { FillColor = Color.FromArgb(0xFF, 0x6E, 0x00, 0x00), OutlineColor = Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF) },
                new () { FillColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x6E), OutlineColor = Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF) },
                new () { FillColor = Color.FromArgb(0xFF, 0x00, 0x6E, 0x00), OutlineColor = Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF) },
            };
        }
    }
}
