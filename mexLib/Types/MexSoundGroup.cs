using HSDRaw.Common;
using MeleeMedia.Audio;
using mexLib.Attributes;
using mexLib.MexScubber;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mexLib.Types
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

    public class MexSoundGroup : MexReactiveObject
    {
        private string _name;
        [Category("General"), DisplayName("Name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        [Category("General"), DisplayName("Filename")]
        public string FileName { get; set; } = "";

        [Browsable(false)]
        public uint GroupFlags { get; set; }

        [JsonIgnore]
        public MexSoundbankType Type
        {
            get => (MexSoundbankType)(GroupFlags >> 24 & 0xFF);
            set => GroupFlags = GroupFlags & ~0xFF000000 | ((uint)value & 0xFF) << 24;
        }

        [JsonIgnore]
        public byte GroupFlag1
        {
            get => (byte)(GroupFlags >> 16 & 0xFF);
            set => GroupFlags = (uint)(GroupFlags & ~0x00FF0000 | ((uint)value & 0xFF) << 16);
        }

        [JsonIgnore]
        public byte GroupFlag2
        {
            get => (byte)(GroupFlags >> 8 & 0xFF);
            set => GroupFlags = (uint)(GroupFlags & ~0x0000FF00 | ((uint)value & 0xFF) << 8);
        }

        [JsonIgnore]
        [DisplayName("Mushroom Script Offset")]
        public byte GroupFlag3
        {
            get => (byte)(GroupFlags & 0xFF);
            set => GroupFlags = (uint)(GroupFlags & ~0x000000FF | (uint)value & 0xFF);
        }

        [DisplayHex]
        public uint Flags { get; set; }

        private ObservableCollection<DSP>? _sounds = null;
        [JsonIgnore]
        public ObservableCollection<DSP>? Sounds { get => _sounds; set { _sounds = value; OnPropertyChanged(); } }

        private ObservableCollection<SEMBankScript>? _scripts = null;
        [JsonIgnore]
        public ObservableCollection<SEMBankScript>? Scripts { get => _scripts; set { _scripts = value; OnPropertyChanged(); } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dol"></param>
        /// <param name="index"></param>
        public void FromDOL(MexDOL dol, uint index)
        {
            // SSMFiles
            FileName = dol.GetStruct<string>(0x803bbcfc, index);
            Name = Path.GetFileNameWithoutExtension(FileName);

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
