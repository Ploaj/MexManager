using HSDRaw.Common;
using MeleeMedia.Audio;
using mexLib.Attributes;
using mexLib.MexScubber;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public enum MexSoundGroupGroup
    {
        Null,
        Constant,
        NarratorName,
        Menu,
        Fighter,
        Stage,
        Ending,
    }

    public enum MexSoundGroupType
    {
        Menu,
        Ending,
        Melee,
        Unused,
        Narrator,
        Constant,
    }

    public enum MexSoundGroupSubType
    {
        NarratorConstant,
        Special, // persists after fighter is unloaded?
        Stage,
        Fighter,
        Narrator,
        Constant,
    }

    public class MexSoundGroup : MexReactiveObject
    {
        private string _name = "";
        [Category("General"), DisplayName("Name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        [Category("General"), DisplayName("File")]
        [MexFilePathValidator(MexFilePathType.Audio)]
        [ReadOnly(true)]
        public string FileName { get; set; } = "";

        [Browsable(false)]
        public uint GroupFlags { get; set; }

        [JsonIgnore]
        public MexSoundGroupGroup Group
        {
            get => (MexSoundGroupGroup)(GroupFlags >> 24 & 0xFF);
            set => GroupFlags = GroupFlags & ~0xFF000000 | ((uint)value & 0xFF) << 24;
        }

        [JsonIgnore]
        public MexSoundGroupType Type
        {
            get => (MexSoundGroupType)(GroupFlags >> 16 & 0xFF);
            set => GroupFlags = (uint)(GroupFlags & ~0x00FF0000 | ((uint)value & 0xFF) << 16);
        }

        [JsonIgnore]
        public MexSoundGroupSubType SubType
        {
            get => (MexSoundGroupSubType)(GroupFlags >> 8 & 0xFF);
            set => GroupFlags = (uint)(GroupFlags & ~0x0000FF00 | ((uint)value & 0xFF) << 8);
        }

        [JsonIgnore]
        [DisplayName("Mushroom Script Offset")]
        public byte Mushroom
        {
            get => (byte)(GroupFlags & 0xFF);
            set => GroupFlags = (uint)(GroupFlags & ~0x000000FF | (uint)value & 0xFF);
        }

        [DisplayHex]
        [Browsable(false)]
        public uint Flags { get; set; } = 0;

        private ObservableCollection<MexSound> _sounds = new ();
        [Browsable(false)]
        public ObservableCollection<MexSound> Sounds { get => _sounds; set { _sounds = value; OnPropertyChanged(); } }

        private ObservableCollection<SEMBankScript>? _scripts = null;
        [JsonIgnore]
        [Browsable(false)]
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

            var bufferSize = (int)gen.Workspace.GetFileSize("audio//us//" + FileName);
            if (bufferSize % 0x20 != 0)
                bufferSize += 0x20 - (bufferSize % 0x20);

            st.SSM_SSMFiles.Set(index, new HSD_String(FileName));
            st.SSM_BufferSizes.Set(index, new HSDRaw.MEX.MEX_SSMSizeAndFlags()
            {
                Flag = (int)Flags,
                SSMFileSize = bufferSize,
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
        /// <summary>
        /// Removes all unused null codes from sound scripts
        /// </summary>
        internal void CleanScripts()
        {
            if (Scripts == null)
                return;

            foreach (var s in Scripts)
            {
                s.Codes.RemoveAll(e => e.Code == SEM_CODE.NULL);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public void ImportSSM(MexWorkspace workspace, string fullPath)
        {
            var ssm = new SSM();
            ssm.Open(Path.GetFileName(fullPath), workspace.FileManager.GetStream(fullPath));

            Sounds.Clear();
            int index = 0;
            foreach (var s in ssm.Sounds)
                Sounds.Add(new MexSound() { Name = $"Sound_{index++:D3}", DSP = s });

            OnPropertyChanged(nameof(Sounds));
        }
    }
}
