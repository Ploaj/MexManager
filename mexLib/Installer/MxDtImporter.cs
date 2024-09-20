using HSDRaw.Melee;
using HSDRaw;
using mexLib.MexScubber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mexLib.Types;
using HSDRaw.MEX;
using mexLib.Utilties;
using HSDRaw.MEX.Stages;
using System.Reflection;
using HSDRaw.Common.Animation;
using HSDRaw.MEX.Menus;
using MeleeMedia.IO;
using HSDRaw.Tools;

namespace mexLib.Installer
{
    public interface IMexImportSource
    {

    }

    public class MexImportFileSystem : IMexImportSource
    {
        public string Path { get; internal set; }

        public MexImportFileSystem(string path)
        {
            Path = path;
        }

    }


    public class MxDtImporter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public static MexInstallerError? Install(MexWorkspace workspace)
        {
            // get project
            var project = workspace.Project;

            // extract base data from dol
            var dol = new MexDOL(workspace.GetDOL());

            // load codes
            if (workspace.FileManager.Exists(workspace.GetFilePath("codes.ini")))
            {
                var ini = CodeLoader.FromINI(workspace.FileManager.Get(workspace.GetFilePath("codes.ini")));
                foreach (var code in ini)
                    project.Codes.Add(code);
            }

            // get plco
            var plcoPath = workspace.GetFilePath("PlCo.dat");
            if (!File.Exists(plcoPath))
                return new MexInstallerError("PlCo.dat not found");
            HSDRawFile plcoFile = new(plcoPath);
            var plco = plcoFile["ftLoadCommonData"].Data as SBM_ftLoadCommonData;
            if (plco == null)
                return new MexInstallerError("Error reading PlCo.dat");

            // create series
            foreach (var s in MexDefaultData.GenerateDefaultSeries())
                project.Series.Add(s);

            // extract and update with data from mxdt
            var mxdtPath = workspace.GetFilePath("MxDt.dat");
            if (!File.Exists(mxdtPath))
                return new MexInstallerError("MxDt.dat not found");
            HSDRawFile mxdtFile = new(mxdtPath);
            var mxdt = mxdtFile["mexData"].Data as MEX_Data;
            if (mxdt == null)
                return new MexInstallerError("Error reading MxDt.dat");

            // load fighters
            for (int i = 0; i < mxdt.MetaData.NumOfInternalIDs; i++)
            {
                var fighter = new MexFighter();
                fighter.FromMxDt(workspace, mxdt, dol, i);
                if (i < plco.BoneTables.Length)
                    fighter.BoneTable = plco.BoneTables[i];
                project.Fighters.Add(fighter);
            }

            // load music
            ShiftJIS.ToShiftJIS("");
            for (int i =0; i < mxdt.MetaData.NumOfMusic; i++)
            {
                project.Music.Add(new MexMusic()
                {
                    FileName = mxdt.MusicTable.BGMFileNames[i].Value,
                    Name = mxdt.MusicTable.BGMLabels[i].Value,
                });
            }

            // load menu playlist
            for (int i = 0; i < mxdt.MusicTable.MenuPlayListCount; i++)
            {
                project.MenuPlaylist.Entries.Add(new MexPlaylistEntry()
                {
                    MusicID = mxdt.MusicTable.MenuPlaylist[i].HPSID,
                    ChanceToPlay = (byte)mxdt.MusicTable.MenuPlaylist[i].ChanceToPlay,
                });
            }

            // load stages
            for (int i = 0; i < mxdt.MetaData.NumOfInternalStage; i++)
            {
                var group = new MexStage();
                group.FromMxDt(mxdt, i);
                project.Stages.Add(group);

                // apply vanilla names
                if (i < MexDefaultData.Stage_Names.Length)
                {
                    group.Name = MexDefaultData.Stage_Names[i].Item1;
                    group.Location = MexDefaultData.Stage_Names[i].Item2;
                    group.SeriesID = MexDefaultData.Stage_Series[i];
                }
                else
                {
                    // default to smash bros group
                    group.SeriesID = 11;
                }

                // get vanilla mapdesc pointers
                if (i < 71)
                {
                    var functionPointer = dol.GetStruct<uint>(0x803DFEDC, (uint)i);

                    if (functionPointer != 0)
                    {
                        dol.GetData(functionPointer, 0x34);

                        group.MapDescPointer = dol.GetStruct<uint>(functionPointer + 4, 0);
                        group.MovingCollisionPointer = dol.GetStruct<uint>(functionPointer + 44, 0);
                    }
                }
            }

            // load sounds
            for (int i = 0; i < mxdt.MetaData.NumOfSSMs; i++)
            {
                var group = new MexSoundGroup();
                group.FromMxDt(mxdt, i);
                project.SoundGroups.Add(group);
            }

            // load scenes
            project.SceneData = mxdt.SceneData;

            // load ifalldata
            LoadIfAll(workspace);

            // extract css
            LoadCharacterSelect(workspace, mxdt);

            // extract sss
            LoadStageSelect(workspace, mxdt);

            // extract result screen
            MexInstaller.InstallResultScreen(workspace);

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="mxdt"></param>
        /// <returns></returns>
        private static MexInstallerError? LoadStageSelect(MexWorkspace workspace, MEX_Data mxdt)
        {
            var project = workspace.Project;

            var path = workspace.GetFilePath("MnSlMap.usd");
            if (!File.Exists(path))
                return new MexInstallerError("MnSlMap.usd");
            HSDRawFile file = new(path);

            var mexMapData = file["mexMapData"].Data as MEX_mexMapData;
            if (mexMapData == null)
                return new MexInstallerError("mexMapData not found");

            var icons = mxdt.MenuTable.SSSIconData.Array;

            // load 30 icons per page by default
            var joints = mexMapData.PositionModel.Children;
            var animjoints = mexMapData.PositionAnimJoint.Children;
            for (int i = 0; i < icons.Length; i++)
            {
                var page_index = i / 30;
                var joint = joints[i];

                if (page_index >= project.StageSelects.Count)
                {
                    project.StageSelects.Add(new MexStageSelect()
                    {
                        Name = $"Page_{page_index}"
                    });
                }

                var page = project.StageSelects[page_index];
                var newicon = new MexStageSelectIcon()
                {
                    X = joint.TX,
                    Y = joint.TY,
                    Z = joint.TZ,
                };
                newicon.FromIcon(icons[i]);
                page.StageIcons.Add(newicon);
            }

            // apply template by default
            // I'm not going to load the original animation data as that's too much
            foreach (var page in project.StageSelects)
            {
                page.Template.ApplyTemplate(page.StageIcons);
            }

            // load stage icon assets and banners
            var texanim = mexMapData.IconMatAnimJoint.Child.MaterialAnimation.Next.TextureAnimation;
            var icontobjs = texanim.ToTOBJs();
            var iconkeys = texanim.AnimationObject.FObjDesc.GetDecodedKeys();

            var banneranim = mexMapData.StageNameMaterialAnimation.Child.Child.MaterialAnimation.TextureAnimation;
            var bannertobjs = banneranim.ToTOBJs();
            var bannerkeys = banneranim.AnimationObject.FObjDesc.GetDecodedKeys();

            // null icon
            if (iconkeys.Find(e => e.Frame == 0) is FOBJKey keynull)
                project.ReservedAssets.SSSNullAsset.SetFromMexImage(
                    workspace,
                    new MexImage(icontobjs[(int)keynull.Value]));

            // locked icon
            if (iconkeys.Find(e => e.Frame == 1) is FOBJKey keylocked)
                project.ReservedAssets.SSSLockedNullAsset.SetFromMexImage(
                    workspace,
                    new MexImage(icontobjs[(int)keylocked.Value]));

            // 
            for (int i = 0; i < icons.Length; i++)
            {
                var icon = icons[i];

                // extract random banner
                if (icon.IconState == 3)
                {
                    if (iconkeys.Find(e => e.Frame == 1) is FOBJKey keyrandom)
                        project.ReservedAssets.SSSRandomBannerAsset.SetFromMexImage(
                            workspace,
                            new MexImage(bannertobjs[(int)keyrandom.Value]));
                }

                // convert icon external id to internal id
                var internalId = MexStageIDConverter.ToInternalID(icon.ExternalID);

                // check if stage exists
                if (internalId > project.Stages.Count)
                    continue;

                // get stage
                var stage = project.Stages[internalId];

                // get icon
                var key = iconkeys.Find(e => e.Frame == 2 + i);
                if (key != null)
                    stage.Assets.IconAsset.SetFromMexImage(workspace, new MexImage(icontobjs[(int)key.Value]));

                // get banner
                key = bannerkeys.Find(e => e.Frame == i);
                if (key != null)
                    stage.Assets.BannerAsset.SetFromMexImage(workspace, new MexImage(bannertobjs[(int)key.Value]));
            }

            // if no random was found, just choose last image, as that's what old mex would do
            if (string.IsNullOrEmpty(project.ReservedAssets.SSSRandomBanner) &&
                bannertobjs.Length > 0)
            {
                project.ReservedAssets.SSSRandomBannerAsset.SetFromMexImage(
                    workspace,
                    new MexImage(bannertobjs[^1]));
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="mxdt"></param>
        /// <returns></returns>
        private static MexInstallerError? LoadCharacterSelect(MexWorkspace workspace, MEX_Data mxdt)
        {
            var project = workspace.Project;
            project.CharacterSelect.FromMxDt(mxdt);

            var path = workspace.GetFilePath("MnSlChr.usd");
            if (!File.Exists(path))
                return new MexInstallerError("MnSlChr.usd");
            HSDRawFile file = new(path);

            // find mexSelectChr
            var mexSelectChr = file["mexSelectChr"].Data as MEX_mexSelectChr;
            if (mexSelectChr == null)
                return new MexInstallerError("mexSelectChr not found");

            var cssicons = mxdt.MenuTable.CSSIconData.Icons.ToList();
            var icons = mexSelectChr.IconModel.Children;
            var keys = mexSelectChr.CSPMatAnim.TextureAnimation.AnimationObject.FObjDesc.GetDecodedKeys();
            var tobjs = mexSelectChr.CSPMatAnim.TextureAnimation.ToTOBJs();

            // get back
            if (string.IsNullOrEmpty(project.ReservedAssets.CSSBack) &&
                icons.Length > 0)
                project.ReservedAssets.CSSBackAsset.SetFromMexImage(
                    workspace,
                    new MexImage(icons[0].Dobj.Mobj.Textures));

            for (int internalId = 0; internalId < mexSelectChr.CSPStride; internalId++)
            {
                // extract fighter icon
                var externalId = MexFighterIDConverter.ToExternalID(internalId, project.Fighters.Count);
                var icon = cssicons.FindIndex(e => e.ExternalCharID == externalId);
                if (icon != -1)
                {
                    project.Fighters[internalId].Assets.CSSIconAsset.SetFromMexImage(
                        workspace, 
                        new MexImage(icons[icon].Dobj.Next.Mobj.Textures));
                }

                // extract csps
                for (int costumeId = 0; costumeId < project.Fighters[internalId].Costumes.Count; costumeId++)
                {
                    var key = keys.Find(e => e.Frame == costumeId * mexSelectChr.CSPStride + externalId);
                    if (key != null)
                    {
                        project.Fighters[internalId].Costumes[costumeId].CSPAsset.SetFromMexImage(workspace, new MexImage(tobjs[(int)key.Value]));
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        private static MexInstallerError? LoadIfAll(MexWorkspace workspace)
        {
            var project = workspace.Project;

            var path = workspace.GetFilePath("IfAll.usd");
            if (!File.Exists(path))
                return new MexInstallerError("IfAll.usd not found");
            HSDRawFile file = new(path);

            // extract emblems
            var emblems = file["Eblm_matanim_joint"].Data as HSD_MatAnimJoint;
            if (emblems != null)
            {
                var keys = emblems.MaterialAnimation.TextureAnimation.AnimationObject.FObjDesc.GetDecodedKeys();
                var tobjs = emblems.MaterialAnimation.TextureAnimation.ToTOBJs();
                for (int i = 0; i < project.Series.Count; i++)
                {
                    var key = keys.Find(e => e.Frame == i);
                    if (key != null)
                    {
                        project.Series[i].IconAsset.SetFromMexImage(workspace, new MexImage(tobjs[(int)key.Value]));
                    }
                }
            }

            // extract stock icons
            var stc = file["Stc_icns"].Data as MEX_Stock;
            if (stc != null)
            {
                var keys = stc.MatAnimJoint.MaterialAnimation.TextureAnimation.AnimationObject.FObjDesc.GetDecodedKeys();
                var tobjs = stc.MatAnimJoint.MaterialAnimation.TextureAnimation.ToTOBJs();

                // get reserved icons
                for (int i = 0; i < project.ReservedAssets.IconsAssets.Length; i++)
                {
                    var key = keys.Find(e => e.Frame == i);
                    if (key != null)
                    {
                        project.ReservedAssets.IconsAssets[i].SetFromMexImage(workspace, new MexImage(tobjs[(int)key.Value]));
                    }
                }

                // index by internal id weirdly enough
                for (int internalId = 0; internalId < project.Fighters.Count; internalId++)
                {
                    for (int costumeId = 0; costumeId < project.Fighters[internalId].Costumes.Count; costumeId++)
                    {
                        var key = keys.Find(e => e.Frame == stc.Reserved + costumeId * stc.Stride + internalId);
                        if (key != null)
                        {
                            project.Fighters[internalId].Costumes[costumeId].IconAsset.SetFromMexImage(workspace, new MexImage(tobjs[(int)key.Value]));
                        }
                    }
                }
            }

            return null;
        }
    }
}
