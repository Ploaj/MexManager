using HSDRaw.MEX.Sounds;
using HSDRaw.MEX.Stages;
using HSDRaw.MEX;
using mexLib.MexScubber;
using System.ComponentModel;
using System.Collections.ObjectModel;
using mexLib.Attributes;
using HSDRaw.Common;
using mexLib.Installer;
using System.Text.Json.Serialization;
using HSDRaw;

namespace mexLib
{
    public class MexStage
    {
        public MEX_Stage Stage { get; set; } = new MEX_Stage();

        public MEX_StageReverb Reverb { get; set; } = new MEX_StageReverb();

        public MEX_StageCollision Collision { get; set; } = new MEX_StageCollision();

        public ObservableCollection<MexItem> Items { get; set; } = new ObservableCollection<MexItem>();

        public MexPlaylist Playlist { get; set; } = new MexPlaylist();

        [Browsable(false)]
        [JsonIgnore]
        public int InternalID { get => Stage.StageInternalID; set { Stage.StageInternalID = value; Collision.InternalID = value; } }

        [Category("0 - General"), DisplayName("Name"), Description("")]
        public string Name { get; set; } = "";

        [Category("0 - General"), DisplayName("File Path"), Description("")]
        [JsonIgnore]
        public string? FileName { get => Stage.StageFileName; set => Stage.StageFileName = string.IsNullOrEmpty(value) ? null : value; }

        [Category("0 - General"), DisplayName("Unknown Stage Value"), Description("")]
        [JsonIgnore]
        public int UnknownValue { get => Stage.UnknownValue; set => Stage.UnknownValue = value; }


        [Category("1 - Sound"), DisplayName("Sound Bank"), Description("")]
        [MexLink(MexLinkType.Sound)]
        [JsonIgnore]
        public int SoundBank { get => Reverb.SSMID; set => Reverb.SSMID = (byte)value; }

        [Category("1 - Sound"), DisplayName("Reverb"), Description("")]
        [JsonIgnore]
        public int ReverbValue { get => Reverb.Reverb; set => Reverb.Reverb = (byte)value; }

        [Category("1 - Sound"), DisplayName("Unknown Sound Data"), Description("")]
        [JsonIgnore]
        public int Unknown { get => Reverb.Unknown; set => Reverb.Unknown = (byte)value; }

        // TODO: expose this data
        //[Category("2 - Extra"), DisplayName("Moving Collision Points"), Description("")]
        //public MEX_MovingCollisionPoint[] MovingCollisions
        //{
        //    get => Stage.MovingCollisionPoint?.Array;
        //    set
        //    {
        //        if (value == null)
        //            return;

        //        Stage.MovingCollisionPointCount = value.Length;
        //        if (value == null || value.Length == 0)
        //        {
        //            Stage.MovingCollisionPoint = null;
        //        }
        //        else
        //        {
        //            Stage.MovingCollisionPoint = new HSDRaw.HSDArrayAccessor<MEX_MovingCollisionPoint>();
        //            Stage.MovingCollisionPoint.Array = value;
        //        }
        //    }
        //}

        [Category("3 - Functions"), DisplayName(""), Description("")]
        [DisplayHex]
        public uint MapGOBJPointer { get => (uint)Stage._s.GetInt32(0x04); set => Stage._s.SetInt32(44, unchecked((int)value)); }

        [Category("3 - Functions"), DisplayName(""), Description("")]
        [DisplayHex]
        public uint MovingCollisionPointer { get => (uint)Stage._s.GetInt32(44); set => Stage._s.SetInt32(44, unchecked((int)value)); }

        [Category("3 - Functions"), DisplayName("OnStageInit"), Description("")]
        [DisplayHex]
        [JsonIgnore]
        public uint OnStageInit { get => Stage.OnStageInit; set => Stage.OnStageInit = value; }

        [Category("3 - Functions"), DisplayName("OnStageLoad"), Description("")]
        [DisplayHex]
        [JsonIgnore]
        public uint OnStageLoad { get => Stage.OnStageLoad; set => Stage.OnStageLoad = value; }

        [Category("3 - Functions"), DisplayName("OnStageGo"), Description("Executes when GO begins in match")]
        [DisplayHex]
        [JsonIgnore]
        public uint OnStageGo { get => Stage.OnStageGo; set => Stage.OnStageGo = value; }

        [Category("3 - Functions"), DisplayName("OnUnknown1"), Description("")]
        [DisplayHex]
        [JsonIgnore]
        public uint OnUnknown1 { get => Stage.OnUnknown1; set => Stage.OnUnknown1 = value; }

        [Category("3 - Functions"), DisplayName("OnUnknown2"), Description("")]
        [DisplayHex]
        [JsonIgnore]
        public uint OnUnknown2 { get => Stage.OnUnknown2; set => Stage.OnUnknown2 = value; }

        [Category("3 - Functions"), DisplayName("OnUnknown3"), Description("")]
        [DisplayHex]
        [JsonIgnore]
        public uint OnUnknown3 { get => Stage.OnUnknown3; set => Stage.OnUnknown3 = value; }

        [Category("3 - Functions"), DisplayName("OnUnknown4"), Description("")]
        [DisplayHex]
        [JsonIgnore]
        public uint OnUnknown4 { get => Stage.OnUnknown4; set => Stage.OnUnknown4 = value; }


        //public bool IsMEXStage
        //{
        //    get
        //    {
        //        return MEX.Stages.IndexOf(this) > 70;
        //    }
        //}

        public override string ToString() => Name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="index"></param>
        public void ToMxDt(MexGenerator gen, int index)
        {
            var sd = gen.Data.StageData;

            // adjust internal id
            InternalID = index;

            // set stage structs
            sd.StageNames.Set(index, new HSD_String(Name));
            sd.CollisionTable.Set(index, Collision);

            // save sound bank indices
            //Reverb.SSMID = (byte)SoundBank;
            sd.ReverbTable.Set(index, Reverb);

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
            gen.Data.StageFunctions.Set(index, Stage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dol"></param>
        /// <param name="index"></param>
        public void FromDOL(MexDOL dol, uint index)
        {
            Name = MexDefaultData.Stage_Names[index];

            // load stage data
            var functionPointer = dol.GetStruct<uint>(0x803DFEDC, index);
            Stage._s.SetData(dol.GetData(functionPointer, 0x34));
            FileName = dol.GetStruct<string>(functionPointer + 0x08, 0);

            // load additional data
            Reverb._s.SetData(dol.GetData(0x803BB6B0 + 3 * index, 0x03));
            Collision._s.SetData(dol.GetData(0x803BF248 + 0x08 * index, 0x08));

            // get soundbank index
            SoundBank = Reverb.SSMID;
        }
    }
}
