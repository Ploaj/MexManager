using HSDRaw.Common;
using HSDRaw.Melee.Pl;
using MeleeMedia.Audio;
using mexLib.MexScubber;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace mexLib
{
    public enum MexSoundbankType
    {
        Null,
        Constant,
        NarratorName,
        Menu,
        Fighter,
        Stage,
        Minigame
    }

    public class MexSoundbank
    {
        [Category("General"), DisplayName("Filename"), Description("Name of the ssm file")]
        public string FileName { get; set; } = "";

        [Browsable(false)]
        public uint GroupFlags { get; set; }

        [JsonIgnore]
        public MexSoundbankType Type
        {
            get => (MexSoundbankType)((GroupFlags >> 24) & 0xFF);
            set => GroupFlags = (GroupFlags & ~0xFF000000) | (((uint)value & 0xFF) << 24);
        }

        [JsonIgnore]
        public byte GroupFlag1
        {
            get => (byte)((GroupFlags >> 16) & 0xFF);
            set => GroupFlags = (uint)((GroupFlags & ~0x00FF0000) | (((uint)value & 0xFF) << 16));
        }

        [JsonIgnore]
        public byte GroupFlag2
        {
            get => (byte)((GroupFlags >> 8) & 0xFF);
            set => GroupFlags = (uint)((GroupFlags & ~0x0000FF00) | (((uint)value & 0xFF) << 8));
        }

        [JsonIgnore]
        public byte GroupFlag3
        {
            get => (byte)(GroupFlags & 0xFF);
            set => GroupFlags = (uint)((GroupFlags & ~0x000000FF) | ((uint)value & 0xFF));
        }

        /// <summary>
        /// 
        /// </summary>
        public uint Flags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public SEMBank ScriptBank { get; internal set; } = new SEMBank();

        //[Browsable(false), YamlIgnore]
        //public bool IsMEXSound
        //{
        //    get
        //    {
        //        return MEX.SoundBanks.IndexOf(this) > 55;
        //    }
        //}


        public void FromDOL(MexDOL dol, uint index)
        {
            // SSMFiles
            FileName = dol.GetStruct<string>(0x803bbcfc, index);

            // SSM_BufferSizes
            Flags = dol.GetStruct<uint>(0x803BC4E4 + 0x04, index, 8);

            // SSM_LookupTable
            GroupFlags = dol.GetStruct<uint>(0x803BB5D0, index);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="index"></param>
        public void ToMxDt(MexGenerator gen, int index)
        {
            var st = gen.Data.SSMTable;

            st.SSM_SSMFiles.Set(index, new HSD_String(FileName));
            st.SSM_BufferSizes.Set(index, new HSDRaw.MEX.MEX_SSMSizeAndFlags()
            {
                Flag = (int)Flags,
                SSMFileSize = (int)gen.Workspace.GetFileSize("audio//us//" + FileName),
            });
            st.SSM_LookupTable.Set(index, new HSDRaw.MEX.MEX_SSMLookup()
            {
                EntireFlag = (int)GroupFlags,
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FileName;
        }
    }
}
