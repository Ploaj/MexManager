using HSDRaw;
using HSDRaw.Common.Animation;
using HSDRaw.Melee;
using HSDRaw.MEX;
using HSDRaw.MEX.Menus;
using HSDRaw.MEX.Stages;
using HSDRaw.Tools;
using mexLib.MexScubber;
using mexLib.Types;
using mexLib.Utilties;

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

        public static void FixMoveLogicPointers(MexWorkspace workspace)
        {
            workspace.GetDOL();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public static MexInstallerError? Install(MexWorkspace workspace)
        {
            // get project
            MexProject project = workspace.Project;

            // extract base data from dol
            MexDOL dol = new(workspace.GetDOL());

            // load codes
            if (workspace.FileManager.Exists(workspace.GetFilePath("codes.ini")))
            {
                IEnumerable<MexCode> ini = CodeLoader.FromINI(workspace.FileManager.Get(workspace.GetFilePath("codes.ini")));
                foreach (MexCode code in ini)
                    project.Codes.Add(code);
            }

            // get plco
            string plcoPath = workspace.GetFilePath("PlCo.dat");
            if (!File.Exists(plcoPath))
                return new MexInstallerError("PlCo.dat not found");
            HSDRawFile plcoFile = new(plcoPath);
            SBM_ftLoadCommonData? plco = plcoFile["ftLoadCommonData"].Data as SBM_ftLoadCommonData;
            if (plco == null)
                return new MexInstallerError("Error reading PlCo.dat");

            // create series
            foreach (MexSeries s in MexDefaultData.GenerateDefaultSeries())
                project.Series.Add(s);

            // extract and update with data from mxdt
            string mxdtPath = workspace.GetFilePath("MxDt.dat");
            if (!File.Exists(mxdtPath))
                return new MexInstallerError("MxDt.dat not found");
            HSDRawFile mxdtFile = new(mxdtPath);
            MEX_Data? mxdt = mxdtFile["mexData"].Data as MEX_Data;
            if (mxdt == null)
                return new MexInstallerError("Error reading MxDt.dat");

            // load meta data
            project.Build.FromMxDt(mxdt);

            // load fighters
            for (uint i = 0; i < mxdt.MetaData.NumOfInternalIDs; i++)
            {
                MexFighter fighter = new();
                fighter.FromMxDt(workspace, mxdt, dol, (int)i);
                fighter.LoadFromPlCo(plco, i);
                project.Fighters.Add(fighter);
            }
            MexInstaller.CorrectFixMoveLogicPointers(workspace);

            // load music
            ShiftJIS.ToShiftJIS("");
            for (int i = 0; i < mxdt.MetaData.NumOfMusic; i++)
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
            Dictionary<uint, (uint, uint)> descLookup = new ();
            for (int i = 0; i < mxdt.MetaData.NumOfInternalStage; i++)
            {
                MexStage group = new();
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
                    uint functionPointer = dol.GetStruct<uint>(0x803DFEDC, (uint)i);

                    if (functionPointer != 0)
                    {
                        dol.GetData(functionPointer, 0x34);

                        group.MapDescPointer = dol.GetStruct<uint>(functionPointer + 4, 0);
                        group.MovingCollisionPointer = dol.GetStruct<uint>(functionPointer + 44, 0);

                        descLookup.TryAdd(group.OnStageLoad, (group.MapDescPointer, group.MovingCollisionPointer));
                    }
                }
                else
                {
                    // if someone cloned a stage we check the onload
                    // and match these pointers based on that
                    if (group.OnStageLoad != 0 &&
                        descLookup.TryGetValue(group.OnStageLoad, out var v))
                    {
                        group.MapDescPointer = v.Item1;
                        group.MovingCollisionPointer = v.Item2;
                    }
                }
            }

            // load sounds
            for (int i = 0; i < mxdt.MetaData.NumOfSSMs; i++)
            {
                MexSoundGroup group = new();
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
            MexProject project = workspace.Project;

            string path = workspace.GetFilePath("MnSlMap.usd");
            if (!File.Exists(path))
                return new MexInstallerError("MnSlMap.usd");
            HSDRawFile file = new(path);

            MEX_mexMapData? mexMapData = file["mexMapData"].Data as MEX_mexMapData;
            if (mexMapData == null)
                return new MexInstallerError("mexMapData not found");

            MEX_StageIconData[] icons = mxdt.MenuTable.SSSIconData.Array;

            // load 30 icons per page by default
            HSDRaw.Common.HSD_JOBJ[] joints = mexMapData.PositionModel.Children;
            HSD_AnimJoint[] animjoints = mexMapData.PositionAnimJoint.Children;
            for (int i = 0; i < icons.Length; i++)
            {
                int page_index = i / 30;
                HSDRaw.Common.HSD_JOBJ joint = joints[i];

                if (page_index >= project.StageSelects.Count)
                {
                    project.StageSelects.Add(new MexStageSelect()
                    {
                        Name = $"Page_{page_index}"
                    });
                }

                MexStageSelect page = project.StageSelects[page_index];
                MexStageSelectIcon newicon = new()
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
            foreach (MexStageSelect page in project.StageSelects)
            {
                page.Template.ApplyTemplate(page.StageIcons);
            }

            // load stage icon assets and banners
            HSD_TexAnim texanim = mexMapData.IconMatAnimJoint.Child.MaterialAnimation.Next.TextureAnimation;
            HSDRaw.Common.HSD_TOBJ[] icontobjs = texanim.ToTOBJs();
            List<FOBJKey> iconkeys = texanim.AnimationObject.FObjDesc.GetDecodedKeys();

            HSD_TexAnim banneranim = mexMapData.StageNameMaterialAnimation.Child.Child.MaterialAnimation.TextureAnimation;
            HSDRaw.Common.HSD_TOBJ[] bannertobjs = banneranim.ToTOBJs();
            List<FOBJKey> bannerkeys = banneranim.AnimationObject.FObjDesc.GetDecodedKeys();

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
                MEX_StageIconData icon = icons[i];

                // extract random banner
                if (icon.IconState == 3)
                {
                    if (iconkeys.Find(e => e.Frame == 1) is FOBJKey keyrandom)
                        project.ReservedAssets.SSSRandomBannerAsset.SetFromMexImage(
                            workspace,
                            new MexImage(bannertobjs[(int)keyrandom.Value]));
                }

                // convert icon external id to internal id
                int internalId = MexStageIDConverter.ToInternalID(icon.ExternalID);

                // check if stage exists
                if (internalId > project.Stages.Count)
                    continue;

                // get stage
                MexStage stage = project.Stages[internalId];

                // get icon
                FOBJKey? key = iconkeys.Find(e => e.Frame == 2 + i);
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
            MexProject project = workspace.Project;
            project.CharacterSelect.FromMxDt(mxdt);

            string path = workspace.GetFilePath("MnSlChr.usd");
            if (!File.Exists(path))
                return new MexInstallerError("MnSlChr.usd");
            HSDRawFile file = new(path);

            // find mexSelectChr
            MEX_mexSelectChr? mexSelectChr = file["mexSelectChr"].Data as MEX_mexSelectChr;
            if (mexSelectChr == null)
                return new MexInstallerError("mexSelectChr not found");

            List<MEX_CSSIcon> cssicons = mxdt.MenuTable.CSSIconData.Icons.ToList();
            HSDRaw.Common.HSD_JOBJ[] icons = mexSelectChr.IconModel.Children;
            List<FOBJKey> keys = mexSelectChr.CSPMatAnim.TextureAnimation.AnimationObject.FObjDesc.GetDecodedKeys();
            HSDRaw.Common.HSD_TOBJ[] tobjs = mexSelectChr.CSPMatAnim.TextureAnimation.ToTOBJs();

            // get back
            if (string.IsNullOrEmpty(project.ReservedAssets.CSSBack) &&
                icons.Length > 0)
                project.ReservedAssets.CSSBackAsset.SetFromMexImage(
                    workspace,
                    new MexImage(icons[0].Dobj.Mobj.Textures));

            for (int internalId = 0; internalId < mexSelectChr.CSPStride; internalId++)
            {
                // extract fighter icon
                int externalId = MexFighterIDConverter.ToExternalID(internalId, project.Fighters.Count);
                int icon = cssicons.FindIndex(e => e.ExternalCharID == externalId);
                if (icon != -1)
                {
                    project.Fighters[internalId].Assets.CSSIconAsset.SetFromMexImage(
                        workspace,
                        new MexImage(icons[icon].Dobj.Next.Mobj.Textures));
                }

                // extract csps
                for (int costumeId = 0; costumeId < project.Fighters[internalId].Costumes.Count; costumeId++)
                {
                    FOBJKey? key = keys.Find(e => e.Frame == costumeId * mexSelectChr.CSPStride + externalId);
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
            MexProject project = workspace.Project;

            string path = workspace.GetFilePath("IfAll.usd");
            if (!File.Exists(path))
                return new MexInstallerError("IfAll.usd not found");
            HSDRawFile file = new(path);

            // extract emblems
            HSD_MatAnimJoint? emblems = file["Eblm_matanim_joint"].Data as HSD_MatAnimJoint;
            if (emblems != null)
            {
                List<FOBJKey> keys = emblems.MaterialAnimation.TextureAnimation.AnimationObject.FObjDesc.GetDecodedKeys();
                HSDRaw.Common.HSD_TOBJ[] tobjs = emblems.MaterialAnimation.TextureAnimation.ToTOBJs();
                for (int i = 0; i < project.Series.Count; i++)
                {
                    FOBJKey? key = keys.Find(e => e.Frame == i);
                    if (key != null)
                    {
                        project.Series[i].IconAsset.SetFromMexImage(workspace, new MexImage(tobjs[(int)key.Value]));
                    }
                }
            }

            // extract stock icons
            MEX_Stock? stc = file["Stc_icns"].Data as MEX_Stock;
            if (stc != null)
            {
                List<FOBJKey> keys = stc.MatAnimJoint.MaterialAnimation.TextureAnimation.AnimationObject.FObjDesc.GetDecodedKeys();
                HSDRaw.Common.HSD_TOBJ[] tobjs = stc.MatAnimJoint.MaterialAnimation.TextureAnimation.ToTOBJs();

                // get reserved icons
                for (int i = 0; i < project.ReservedAssets.IconsAssets.Length; i++)
                {
                    FOBJKey? key = keys.Find(e => e.Frame == i);
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
                        FOBJKey? key = keys.Find(e => e.Frame == stc.Reserved + costumeId * stc.Stride + internalId);
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
