using HSDRaw.MEX.Stages;
using HSDRaw.MEX;
using mexLib.MexScubber;
using System.ComponentModel;
using System.Collections.ObjectModel;
using mexLib.Attributes;
using HSDRaw.Common;
using mexLib.Installer;
using mexLib.Utilties;
using System.IO.Compression;
using SixLabors.ImageSharp.Drawing.Processing;
using System;

namespace mexLib.Types
{
    public partial class MexStage : MexReactiveObject
    {
        [Category("0 - General"), DisplayName("Name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        private string _name = "";

        [Category("0 - General"), DisplayName("Location")]
        public string Location { get; set; } = "";

        [Category("0 - General"), DisplayName("Series")]
        [MexLink(MexLinkType.Series)]
        public int SeriesID { get; set; } = 0;

        [Category("0 - General"), DisplayName("File Path")]
        [MexFilePathValidator(MexFilePathType.Files)]
        public string? FileName { get => _fileName; set => _fileName = string.IsNullOrEmpty(value) ? null : value; }

        private string? _fileName;

        [Category("0 - General"), DisplayName("Additional Files")]
        public BindingList<string> AdditionalFiles { get; set; } = new BindingList<string>();

        [Category("1 - Sound"), DisplayName("Sound Bank")]
        [MexLink(MexLinkType.Sound)]
        public int SoundBank { get => _soundBank; set { _soundBank = value; OnPropertyChanged(); } }

        private int _soundBank;

        [Category("1 - Sound"), DisplayName("Reverb 1")]
        public int ReverbValue1 { get; set; }

        [Category("1 - Sound"), DisplayName("Reverb 2")]
        public int ReverbValue2 { get; set; }


        [Category("2 - Code"), DisplayName("Collision Materials")]
        [DisplayHex]
        public uint CollisionMaterials { get; set; }

        [Category("2 - Code"), DisplayName("")]
        [DisplayHex]
        public uint MapDescPointer { get; set; }

        [Category("2 - Code"), DisplayName("OnStageInit")]
        [DisplayHex]
        public uint OnStageInit { get; set; }

        [Category("2 - Code"), DisplayName("OnStageLoad")]
        [DisplayHex]
        public uint OnStageLoad { get; set; }

        [Category("2 - Code"), DisplayName("OnStageGo"), Description("Executes when GO begins in match")]
        [DisplayHex]
        public uint OnStageGo { get; set; }

        [Category("2 - Code"), DisplayName("OnUnknown1")]
        [DisplayHex]
        public uint OnGo { get; set; }

        [Category("2 - Code"), DisplayName("OnUnknown2")]
        [DisplayHex]
        public uint OnUnknown2 { get; set; }

        [Category("2 - Code"), DisplayName("OnUnknown3")]
        [DisplayHex]
        public uint OnTouchLine { get; set; }

        [Category("2 - Code"), DisplayName("OnUnknown4")]
        [DisplayHex]
        public uint OnUnknown4 { get; set; }

        // rainbow cruise is 4 and pichu target test is 0, but I think this is unused
        [Browsable(false)]
        public int UnknownValue { get; set; } = 1;

        [Category("2 - Code"), DisplayName("")]
        [DisplayHex]
        public uint MovingCollisionPointer { get; set; }

        [Category("2 - Code"), DisplayName("")]
        [DisplayHex]
        public int MovingCollisionCount { get; set; }

        [Browsable(false)]
        public ObservableCollection<MexItem> Items { get; set; } = new ObservableCollection<MexItem>();

        [Browsable(false)]
        public MexPlaylist Playlist { get; set; } = new MexPlaylist();

        public override string ToString() => Name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="index"></param>
        public void ToMxDt(MexGenerator gen, int index)
        {
            var sd = gen.Data.StageData;

            // set stage structs
            sd.StageNames.Set(index, new HSD_String(Name));
            sd.CollisionTable.Set(index, new MEX_StageCollision()
            {
                InternalID = index,
                CollisionFunction = (int)CollisionMaterials
            });

            // save sound bank indices
            sd.ReverbTable.Set(index, new MEX_StageReverb()
            {
                SSMID = (byte)SoundBank,
                Reverb = (byte)ReverbValue1,
                Unknown = (byte)ReverbValue2,
            });

            // save playlist 
            sd.StagePlaylists.Set(index, Playlist.ToMexPlaylist());

            // save items
            var itemEntries = new ushort[Items.Count];
            for (int i = 0; i < itemEntries.Length; i++)
            {
                itemEntries[i] = (ushort)(MexDefaultData.BaseItemCount + gen.MexItems.Count);
                gen.MexItems.Add(Items[i].ToMexItem());
            }
            sd.StageItemLookup.Set(index, new MEX_ItemLookup() { Entries = itemEntries });

            // save functions
            var stage = new MEX_Stage()
            {
                StageInternalID = index,
                StageFileName = FileName,
                GOBJFunctionsPointer = (int)MapDescPointer,
                MovingCollisionPointCount = MovingCollisionCount,
                OnStageGo = OnStageGo,
                OnStageInit = OnStageInit,
                OnStageLoad = OnStageLoad,
                OnUnknown1 = OnGo,
                OnUnknown2 = OnUnknown2,
                OnUnknown3 = OnTouchLine,
                OnUnknown4 = OnUnknown4,
                UnknownValue = UnknownValue,
            };
            stage._s.SetInt32(44, (int)MovingCollisionPointer);
            gen.Data.StageFunctions.Set(index, stage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mxdt"></param>
        /// <param name="i"></param>
        internal void FromMxDt(MEX_Data mxdt, int index)
        {
            var sd = mxdt.StageData;

            // load stage structs
            Name = sd.StageNames[index].Value;
            CollisionMaterials = (uint)sd.CollisionTable[index].CollisionFunction;

            // load sound bank indices
            SoundBank = sd.ReverbTable[index].SSMID;
            ReverbValue1 = sd.ReverbTable[index].Reverb;
            ReverbValue2 = sd.ReverbTable[index].Unknown;

            // load playlist 
            Playlist.FromMexPlayList(sd.StagePlaylists[index]);

            // load items
            Items.Clear();
            foreach (var i in sd.StageItemLookup[index].Entries)
            {
                var item = new MexItem();
                item.FromMexItem(mxdt.ItemTable.MEXItems[i - MexDefaultData.BaseItemCount]);
                Items.Add(item);
            }

            // load functions
            var sf = mxdt.StageFunctions[index];
            FileName = sf.StageFileName;
            MapDescPointer = (uint)sf.GOBJFunctionsPointer;
            MovingCollisionCount = sf.MovingCollisionPointCount;
            MovingCollisionPointer = (uint)sf._s.GetInt32(44);
            OnStageGo = sf.OnStageGo;
            OnStageInit = sf.OnStageInit;
            OnStageLoad = sf.OnStageLoad;
            OnGo = sf.OnUnknown1;
            OnUnknown2 = sf.OnUnknown2;
            OnTouchLine = sf.OnUnknown3;
            OnUnknown4 = sf.OnUnknown4;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dol"></param>
        /// <param name="index"></param>
        public void FromDOL(MexDOL dol, uint index)
        {
            Name = MexDefaultData.Stage_Names[index].Item1;
            Location = MexDefaultData.Stage_Names[index].Item2;
            SeriesID = MexDefaultData.Stage_Series[index];

            // load stage data
            var functionPointer = dol.GetStruct<uint>(0x803DFEDC, index);

            if (functionPointer != 0)
            {
                MEX_Stage stage = new()
                {
                    _s = new HSDRaw.HSDStruct(dol.GetData(functionPointer, 0x34))
                };

                FileName = dol.GetStruct<string>(functionPointer + 0x08, 0);

                MapDescPointer = (uint)stage.GOBJFunctionsPointer;
                MovingCollisionCount = stage.MovingCollisionPointCount;
                MovingCollisionPointer = (uint)stage._s.GetInt32(44);
                OnStageGo = stage.OnStageGo;
                OnStageInit = stage.OnStageInit;
                OnStageLoad = stage.OnStageLoad;
                OnGo = stage.OnUnknown1;
                OnUnknown2 = stage.OnUnknown2;
                OnTouchLine = stage.OnUnknown3;
                OnUnknown4 = stage.OnUnknown4;
                UnknownValue = stage.UnknownValue;
            }
            
            // load additional data
            SoundBank = dol.GetStruct<byte>(0x803BB6B0 + 0x00, index, 0x03);
            ReverbValue1 = dol.GetStruct<byte>(0x803BB6B0 + 0x01, index, 0x03);
            ReverbValue2 = dol.GetStruct<byte>(0x803BB6B0 + 0x02, index, 0x03);

            // collision materials
            CollisionMaterials = dol.GetStruct<uint>(0x803BF248 + 0x04, index, 0x08);

            // install additional stage files
            if (index == 16) //  (Pokemon Stadium)
            {
                AdditionalFiles.Add("GrPs1.dat");
                AdditionalFiles.Add("GrPs2.dat");
                AdditionalFiles.Add("GrPs3.dat");
                AdditionalFiles.Add("GrPs4.dat");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public class StagePackOptions
        {
            [Category("Options")]
            [DisplayName("Include File")]
            public bool ExportFiles { get; set; }

            [Category("Options")]
            [DisplayName("Include SoundBank")]
            public bool ExportSound { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="workspace"></param>
        /// <param name="stage"></param>
        /// <param name="options"></param>
        public static void ToPackage(Stream s, MexWorkspace workspace, MexStage stage, StagePackOptions options)
        {
            using var zip = new ZipWriter(s);

            // write stage
            zip.WriteAsJson("stage.json", stage);

            // write assets
            stage.Assets.ToPackage(workspace, zip);
            
            if (options.ExportFiles)
            {
                // write files
                if (stage.FileName != null)
                    zip.TryWriteFile(workspace, stage.FileName, stage.FileName);

                // write additional files
                foreach (var f in stage.AdditionalFiles)
                    zip.TryWriteFile(workspace, f, f);
            }

            // write playlist music??

            if (options.ExportSound)
            {
            // write soundbank
                if (stage.SoundBank != 55)
                {
                    using var ms = new MemoryStream();
                    MexSoundGroup.ToPackage(workspace.Project.SoundGroups[stage.SoundBank], ms);
                    zip.Write("sound.zip", ms.ToArray());
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="workspace"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public static MexInstallerError? FromPackage(Stream s, MexWorkspace workspace, out MexStage? stage)
        {
            stage = null;
            using ZipArchive zip = new(s);

            // load group entry
            {
                var entry = zip.GetEntry("stage.json");
                if (entry == null)
                    return new MexInstallerError("\"stage.json\" was not found in zip");

                // parse group entry
                stage = MexJsonSerializer.Deserialize<MexStage>(entry.Extract());
                if (stage == null)
                    return new MexInstallerError("Error parsing \"stage.json\"");

                // init stage data
                stage.SoundBank = 55;

                // init playlist
                stage.Playlist = new MexPlaylist()
                {
                    Entries = 
                    {
                        new ()
                        {
                            MusicID = 0,
                            ChanceToPlay = 50,
                        }
                    }
                };

                // load assets
                stage.Assets.FromPackage(workspace, zip);
            }

            // load files
            if (stage.FileName != null)
            {
                var stage_file = zip.GetEntry(stage.FileName);
                if (stage_file != null)
                {
                    var fullPath = workspace.GetFilePath(stage.FileName);
                    fullPath = workspace.FileManager.GetUniqueFilePath(fullPath);
                    workspace.FileManager.Set(fullPath, stage_file.Extract());
                    stage.FileName = Path.GetFileName(fullPath);
                }
            }

            // additional files
            foreach (var f in stage.AdditionalFiles)
            {
                var stage_file = zip.GetEntry(f);
                if (stage_file != null)
                {
                    var fullPath = workspace.GetFilePath(f);
                    fullPath = workspace.FileManager.GetUniqueFilePath(fullPath);
                    workspace.FileManager.Set(fullPath, stage_file.Extract());
                }
            }

            // load soundbank
            {
                var entry = zip.GetEntry("sound.zip");

                if (entry != null)
                {
                    using var ms = new MemoryStream(entry.Extract());
                    MexSoundGroup.FromPackage(workspace, ms, out MexSoundGroup? group);
                    if (group != null)
                    {
                        stage.SoundBank = workspace.Project.AddSoundGroup(group);
                    }
                    else
                    {
                        stage.SoundBank = 55;
                    }
                }
                else
                {
                    stage.SoundBank = 55;
                }
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        public void Delete(MexWorkspace ws)
        {
            if (FileName != null)
                ws.FileManager.Remove(ws.GetFilePath(FileName));

            foreach (var f in AdditionalFiles)
                ws.FileManager.Remove(ws.GetFilePath(f));

            Assets.Delete(ws);
        }
    }
}
