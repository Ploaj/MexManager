using HSDRaw.MEX.Scenes;
using mexLib.Types;
using mexLib.Utilties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mexLib
{
    public class MexProject
    {
        public int StartingScene { get; set; } = 2;

        public int LastMajorSceneID { get; set; } = 45;

        public int LastMinorSceneID { get; set; } = 45;

        public int TrophyCount { get; set; } = 0x125;

        public int TrophySDOffset { get; set; } = 302;

        public MexPlaylist MenuPlaylist { get; set; } = new MexPlaylist();

        public MexReservedAssets ReservedAssets { get; set; } = new MexReservedAssets();

        [JsonIgnore]
        public MexCode MainCode { get; set; } = new MexCode();

        [JsonIgnore]
        public ObservableCollection<MexCode> Codes { get; set; } = new ObservableCollection<MexCode>();

        [JsonIgnore]
        public MexCharacterSelect CharacterSelect { get; set; } = new MexCharacterSelect();

        [JsonIgnore]
        public ObservableCollection<MexStageSelect> StageSelects { get; set; } = new ObservableCollection<MexStageSelect>();

        [JsonIgnore]
        public ObservableCollection<MexFighter> Fighters { get; set; } = new ObservableCollection<MexFighter>();

        [JsonIgnore]
        public ObservableCollection<MexStage> Stages { get; set; } = new ObservableCollection<MexStage>();

        [JsonIgnore]
        public ObservableCollection<MexSoundGroup> SoundGroups { get; set; } = new ObservableCollection<MexSoundGroup>();

        [JsonIgnore]
        public ObservableCollection<MexMusic> Music { get; set; } = new ObservableCollection<MexMusic>();

        [JsonIgnore]
        public ObservableCollection<MexSeries> Series { get; set; } = new ObservableCollection<MexSeries>();

        [JsonIgnore]
        public MEX_SceneData SceneData { get; set; } = new MEX_SceneData();

        //public MEX_Item[] CommonItems = new MEX_Item[0];
        //public MEX_Item[] FighterItems = new MEX_Item[0];
        //public MEX_Item[] PokemonItems = new MEX_Item[0];
        //public MEX_Item[] StageItems = new MEX_Item[0];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MexFighter? GetFighterByInternalID(int id)
        {
            if (id < 0 || id >= Fighters.Count)
                return null;
            return Fighters[id];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MexFighter? GetFighterByExternalID(int id)
        {
            var internalId = MexFighterIDConverter.ToInternalID(id, Fighters.Count);
            return GetFighterByInternalID(internalId);
        }
        /// <summary>
        /// 
        /// </summary>
        public MexProject()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MexCode> GetAllCodes()
        {
            yield return MainCode;

            foreach (var c in Codes)
                if (c.Enabled)
                    yield return c;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fighter"></param>
        /// <returns></returns>
        public bool AddNewFighter(MexFighter fighter)
        {
            var internalId = Fighters.Count - 6;
            var externalId = MexFighterIDConverter.ToExternalID(internalId, Fighters.Count);
            Fighters.Insert(Fighters.Count - 6, fighter);

            // fighter
            foreach (var f in Fighters)
            {
                if (f.SubCharacter >= externalId)
                    f.SubCharacter += 1;
            }

            // css icons
            foreach (var icon in CharacterSelect.FighterIcons)
            {
                if (icon.Fighter >= externalId)
                {
                    icon.Fighter += 1;
                }
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool RemoveFighter(MexWorkspace workspace, int internalId)
        {
            if (!MexFighterIDConverter.IsMexFighter(internalId, Fighters.Count))
                return false;

            var externalId = MexFighterIDConverter.ToExternalID(internalId, Fighters.Count);

            Fighters[internalId].Delete(workspace);
            Fighters.RemoveAt(internalId);

            // fighter
            foreach (var fighter in Fighters)
            {
                if (fighter.SubCharacter == externalId)
                    fighter.SubCharacter = -1;
                else
                if (fighter.SubCharacter > externalId)
                    fighter.SubCharacter -= 1;
            }

            // css icons
            foreach (var icon in CharacterSelect.FighterIcons)
            {
                if (icon.Fighter == externalId)
                {
                    icon.Fighter = 0;
                }
                else if (icon.Fighter > externalId)
                {
                    icon.Fighter -= 1;
                }
            }

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stage"></param>
        /// <returns></returns>
        public int AddStage(MexStage stage)
        {
            Stages.Add(stage);
            return Stages.Count - 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalId"></param>
        /// <returns></returns>
        public bool RemoveStage(MexWorkspace ws, int internalId)
        {
            // deny remove vanilla stages
            if (internalId <= 70)
                return false;

            // get stages external id
            var externalId = MexStageIDConverter.ToExternalID(internalId);

            //
            Stages[internalId].Delete(ws);

            // remove stage
            Stages.RemoveAt(internalId);

            // fighter
            foreach (var fighter in Fighters)
            {
                if (fighter.TargetTestStage == externalId)
                    fighter.TargetTestStage = 0;
                else
                if (fighter.TargetTestStage >= externalId)
                    fighter.TargetTestStage -= 1;
            }

            // stage select icons
            foreach (var sss in StageSelects)
            {
                foreach (var icon in sss.StageIcons)
                {
                    if (icon.StageID == externalId)
                    {
                        icon.StageID = 0;
                    }
                    else if (icon.StageID > externalId)
                    {
                        icon.StageID -= 1;
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="music"></param>
        public void AddMusic(MexMusic music)
        {
            Music.Add(music);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="music"></param>
        /// <returns>false if failed to remove music and true otherwise</returns>
        public bool RemoveMusic(MexMusic music)
        {
            var index = Music.IndexOf(music);

            // check if music is in project
            if (index == -1)
                return false;

            // check if vanilla music
            if (index <= 97)
                return false;

            Music.RemoveAt(index);

            foreach (var f in Fighters)
            {
                if (f.FighterMusic1 == index)
                    f.FighterMusic1 = 0;
                else if (f.FighterMusic1 > index)
                    f.FighterMusic1 -= 1;

                if (f.FighterMusic2 == index)
                    f.FighterMusic2 = 0;
                else if (f.FighterMusic2 > index)
                    f.FighterMusic2 -= 1;
            }

            // remove from stage playlists
            foreach (var s in Stages)
                s.Playlist.RemoveTrack(index);

            // remove from menu playlists
            MenuPlaylist.RemoveTrack(index);

            // remove from series playlists
            foreach (var s in Series)
                s.Playlist.RemoveTrack(index);

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="series"></param>
        /// <exception cref="NotImplementedException"></exception>
        public bool RemoveSeries(MexWorkspace workspace, MexSeries series)
        {
            var index = Series.IndexOf(series);

            // check if series is in project
            if (index == -1)
                return false;

            series.Delete(workspace);
            Series.Remove(series);

            // remove assets

            foreach (var f in Fighters)
            {
                if (f.SeriesID == index)
                    f.SeriesID = 0;
                else if (f.SeriesID > index)
                    f.SeriesID -= 1;
            }

            // remove from stage playlists
            foreach (var s in Stages)
            {
                if (s.SeriesID == index)
                    s.SeriesID = 0;
                else if (s.SeriesID > index)
                    s.SeriesID -= 1;
            }

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public int AddSoundGroup(MexSoundGroup group)
        {
            SoundGroups.Add(group);
            return SoundGroups.Count - 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool RemoveSoundGroup(MexSoundGroup group)
        {
            var soundIndex = SoundGroups.IndexOf(group);

            if (soundIndex == -1 || 
                !MexSoundGroupIDConverter.IsMexSoundGroup(soundIndex))
                return false;

            SoundGroups.RemoveAt(soundIndex);

            // fighter
            foreach (var fighter in Fighters)
            {
                if (fighter.SoundBank == soundIndex)
                    fighter.SoundBank = 55;
                else if (fighter.SoundBank > soundIndex)
                    fighter.SoundBank -= 1;
            }

            // stage
            foreach (var stage in Stages)
            {
                if (stage.SoundBank == soundIndex)
                    stage.SoundBank = 55;
                else if (stage.SoundBank > soundIndex)
                    stage.SoundBank -= 1;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static MexProject LoadFromFile(MexWorkspace ws)
        {
            // load project file
            var proj = MexJsonSerializer.Deserialize<MexProject>(ws.ProjectFilePath);

            if (proj == null)
                return new MexProject();

            // load fighters
            foreach (var f in proj.FighterSaveMap.Load<MexFighter>(ws.GetDataPath("fighters//")))
                proj.Fighters.Add(f);

            // load stages
            foreach (var f in proj.StageSaveMap.Load<MexStage>(ws.GetDataPath("stages//")))
                proj.Stages.Add(f);

            // load sounds
            foreach (var f in proj.SoundSaveMap.Load<MexSoundGroup>(ws.GetDataPath("sounds//")))
                proj.SoundGroups.Add(f);

            // Load main code
            MexJsonSerializer.LoadData<MexCode>(ws.GetDataPath("mex.json"), data => proj.MainCode = data);

            // Load codes
            MexJsonSerializer.LoadData<ObservableCollection<MexCode>>(ws.GetDataPath("codes.json"), data => proj.Codes = data);

            // Load character select
            MexJsonSerializer.LoadData<MexCharacterSelect>(ws.GetDataPath("css.json"), data => proj.CharacterSelect = data);

            // Load stage select
            MexJsonSerializer.LoadData<ObservableCollection<MexStageSelect>>(ws.GetDataPath("sss.json"), data => proj.StageSelects = data);

            // Load series
            MexJsonSerializer.LoadData<ObservableCollection<MexSeries>>(ws.GetDataPath("series.json"), data => proj.Series = data);

            // Load music
            MexJsonSerializer.LoadData<ObservableCollection<MexMusic>>(ws.GetDataPath("music.json"), data => proj.Music = data);

            // Load scene
            MexJsonSerializer.LoadData<MEX_SceneData>(ws.GetDataPath("scene.json"), data => proj.SceneData = data);

            return proj;
        }
        public class SaveMap
        {
            public List<string> FilePaths { get; set; } = new List<string>();

            public IEnumerable<T> Load<T>(string dataPath)
            {
                foreach (var f in FilePaths)
                {
                    var full = Path.Combine(dataPath, f);

                    if (!File.Exists(full))
                    {
                        yield return Activator.CreateInstance<T>();
                    }
                    else
                    {
                        var dat = MexJsonSerializer.Deserialize<T>(full);

                        if (dat != null)
                            yield return dat;
                        else
                            yield return Activator.CreateInstance<T>();
                    }
                }
            }

            public void Save<T>(IEnumerable<T> data, string dataPath)
            {
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);

                FilePaths.Clear();
                int i = 0;
                foreach (var f in data)
                {
                    var fileName = i++.ToString("D3") + ".json";
                    var fpath = Path.Combine(dataPath, fileName);
                    FilePaths.Add(fileName);
                    File.WriteAllText(fpath, MexJsonSerializer.Serialize(f));
                }
            }
        }

        [Browsable(false)]
        public SaveMap FighterSaveMap { get; set; } = new SaveMap();

        [Browsable(false)]
        public SaveMap StageSaveMap { get; set; } = new SaveMap();

        [Browsable(false)]
        public SaveMap SoundSaveMap { get; set; } = new SaveMap();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void Save(MexWorkspace workspace)
        {
            // save fighters
            FighterSaveMap.Save(Fighters, workspace.GetDataPath("fighters//"));

            // save stages
            StageSaveMap.Save(Stages, workspace.GetDataPath("stages//"));

            // save sounds
            SoundSaveMap.Save(SoundGroups, workspace.GetDataPath("sounds//"));

            // save items
            //{
            //    File.WriteAllText(Path.Combine(projectPath, "data//items_common.json"), JsonSerializer.Serialize(CommonItems, _serializeoptions));
            //    File.WriteAllText(Path.Combine(projectPath, "data//items_pokemon.json"), JsonSerializer.Serialize(PokemonItems, _serializeoptions));
            //    File.WriteAllText(Path.Combine(projectPath, "data//items_stages.json"), JsonSerializer.Serialize(StageItems, _serializeoptions));
            //    File.WriteAllText(Path.Combine(projectPath, "data//items_fighters.json"), JsonSerializer.Serialize(FighterItems, _serializeoptions));
            //}

            // save mexcode
            File.WriteAllText(workspace.GetDataPath("mex.json"), MexJsonSerializer.Serialize(MainCode));

            // save codes
            File.WriteAllText(workspace.GetDataPath("codes.json"), MexJsonSerializer.Serialize(Codes));

            // save character select
            File.WriteAllText(workspace.GetDataPath("css.json"), MexJsonSerializer.Serialize(CharacterSelect));

            // save stage select
            File.WriteAllText(workspace.GetDataPath("sss.json"), MexJsonSerializer.Serialize(StageSelects));

            // save series
            File.WriteAllText(workspace.GetDataPath("series.json"), MexJsonSerializer.Serialize(Series));

            // save music
            File.WriteAllText(workspace.GetDataPath("music.json"), MexJsonSerializer.Serialize(Music));

            // save scenes
            File.WriteAllText(workspace.GetDataPath("scene.json"), MexJsonSerializer.Serialize(SceneData));

            // save project file
            File.WriteAllText(workspace.ProjectFilePath, MexJsonSerializer.Serialize(this));
        }
    }
}