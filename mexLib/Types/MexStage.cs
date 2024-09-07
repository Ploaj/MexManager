using HSDRaw.MEX.Stages;
using HSDRaw.MEX;
using mexLib.MexScubber;
using System.ComponentModel;
using System.Collections.ObjectModel;
using mexLib.Attributes;
using HSDRaw.Common;
using mexLib.Installer;
using HSDRaw;

namespace mexLib.Types
{
    public partial class MexStage
    {
        [Category("0 - General"), DisplayName("Name")]
        public string Name { get; set; } = "";

        [Category("0 - General"), DisplayName("Series")]
        [MexLink(MexLinkType.Series)]
        public int SeriesID { get; set; } = 0;

        [Category("0 - General"), DisplayName("File Path")]
        [MexFilePathValidator(MexFilePathType.Files)]
        public string? FileName { get => _fileName; set => _fileName = string.IsNullOrEmpty(value) ? null : value; }

        private string? _fileName;

        [Category("0 - General"), DisplayName("Collision Materials")]
        [DisplayHex]
        public uint CollisionMaterials { get; set; }

        [Category("1 - Sound"), DisplayName("Sound Bank")]
        [MexLink(MexLinkType.Sound)]
        public int SoundBank { get; set; }

        [Category("1 - Sound"), DisplayName("Reverb 1")]
        public int ReverbValue1 { get; set; }

        [Category("1 - Sound"), DisplayName("Reverb 2")]
        public int ReverbValue2 { get; set; }

        [Category("2 - Functions"), DisplayName("")]
        [DisplayHex]
        public uint MapDescPointer { get; set; }

        [Category("2 - Functions"), DisplayName("")]
        [DisplayHex]
        public uint MovingCollisionPointer { get; set; }

        [Category("2 - Functions"), DisplayName("")]
        [DisplayHex]
        public int MovingCollisionCount { get; set; }

        [Category("2 - Functions"), DisplayName("OnStageInit")]
        [DisplayHex]
        public uint OnStageInit { get; set; }

        [Category("2 - Functions"), DisplayName("OnStageLoad")]
        [DisplayHex]
        public uint OnStageLoad { get; set; }

        [Category("2 - Functions"), DisplayName("OnStageGo"), Description("Executes when GO begins in match")]
        [DisplayHex]
        public uint OnStageGo { get; set; }

        [Category("2 - Functions"), DisplayName("OnUnknown1")]
        [DisplayHex]
        public uint OnGo { get; set; }

        [Category("2 - Functions"), DisplayName("OnUnknown2")]
        [DisplayHex]
        public uint OnUnknown2 { get; set; }

        [Category("2 - Functions"), DisplayName("OnUnknown3")]
        [DisplayHex]
        public uint OnTouchLine { get; set; }

        [Category("2 - Functions"), DisplayName("OnUnknown4")]
        [DisplayHex]
        public uint OnUnknown4 { get; set; }

        // rainbow cruise is 4 and pichu target test is 0, but I think this is unused
        [Browsable(false)]
        public int UnknownValue { get; set; } = 1;

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
        /// <param name="dol"></param>
        /// <param name="index"></param>
        public void FromDOL(MexDOL dol, uint index)
        {
            Name = MexDefaultData.Stage_Names[index];
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
        }
    }
}
