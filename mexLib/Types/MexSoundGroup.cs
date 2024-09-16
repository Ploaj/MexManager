using HSDRaw.Common;
using MeleeMedia.Audio;
using mexLib.Attributes;
using mexLib.Installer;
using mexLib.MexScubber;
using mexLib.Utilties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Compression;
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
        [Category("General")]
        [DisplayName("Name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        [Category("General")]
        [DisplayName("FileName")]
        [ReadOnly(true)]
        public string FileName { get; set; } = "";

        [Browsable(false)]
        public uint GroupFlags { get; set; }

        [Category("General")]
        [JsonIgnore]
        public MexSoundGroupGroup Group
        {
            get => (MexSoundGroupGroup)(GroupFlags >> 24 & 0xFF);
            set => GroupFlags = GroupFlags & ~0xFF000000 | ((uint)value & 0xFF) << 24;
        }

        [Category("General")]
        [JsonIgnore]
        public MexSoundGroupType Type
        {
            get => (MexSoundGroupType)(GroupFlags >> 16 & 0xFF);
            set => GroupFlags = (uint)(GroupFlags & ~0x00FF0000 | ((uint)value & 0xFF) << 16);
        }

        [Category("General")]
        [JsonIgnore]
        public MexSoundGroupSubType SubType
        {
            get => (MexSoundGroupSubType)(GroupFlags >> 8 & 0xFF);
            set => GroupFlags = (uint)(GroupFlags & ~0x0000FF00 | ((uint)value & 0xFF) << 8);
        }

        [Category("General")]
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
            return string.IsNullOrEmpty(Name) ? FileName : Name;
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
        /// <returns></returns>
        public byte[] PackSSM()
        {
            var ssm = new SSM()
            {
                Name = Name,
                Sounds = Sounds.Select(e => e.DSP).ToArray(),
            };

            using var stream = new MemoryStream();
            ssm.WriteToStream(stream, out int bs);
            return stream.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public void ImportSSM(MexWorkspace workspace, string fullPath, bool replace)
        {
            var ssm = new SSM();
            ssm.Open(Path.GetFileName(fullPath), workspace.FileManager.GetStream(fullPath));

            if (replace)
                Sounds.Clear();
            int index = 0;
            foreach (var s in ssm.Sounds)
                Sounds.Add(new MexSound() { Name = $"Sound_{index++:D3}", DSP = s });

            OnPropertyChanged(nameof(Sounds));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        public void CopyFrom(MexSoundGroup group)
        {
            if (Scripts != null &&
                group.Scripts != null)
            {
                Scripts.Clear();
                foreach (var s in group.Scripts)
                {
                    Scripts.Add(new SEMBankScript()
                    {
                        SFXID = s.SFXID,
                        Name = s.Name,
                        Codes = s.Codes.Select(e => new SEMCode(e.Pack())).ToList()
                    });
                }
            }

            Sounds.Clear();
            foreach (var s in group.Sounds)
            {
                Sounds.Add(new MexSound()
                {
                    Name = s.Name,
                    DSP = CloneDSP(s.DSP),
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static DSP? CloneDSP(DSP? source)
        {
            if (source == null) return null;

            return new DSP()
            {
                Frequency = source.Frequency,
                LoopSound = source.LoopSound,
                LoopPointMilliseconds = source.LoopPointMilliseconds,
                Channels = source.Channels.Select(e => new DSPChannel()
                {
                    Format = e.Format,
                    COEF = e.COEF,
                    Data = (byte[])e.Data.Clone(),
                    LoopFlag = e.LoopFlag,
                    Gain = e.Gain,
                    InitialPredictorScale = e.InitialPredictorScale,
                    InitialSampleHistory1 = e.InitialSampleHistory1,
                    InitialSampleHistory2 = e.InitialSampleHistory2,
                    LoopPredictorScale = e.LoopPredictorScale,
                    LoopSampleHistory1 = e.LoopSampleHistory1,
                    LoopSampleHistory2 = e.LoopSampleHistory2,
                    LoopStart = e.LoopStart,
                    NibbleCount = e.NibbleCount,
                }).ToList()
            };
        }
        /// <summary>
        /// 
        /// </summary>
        public static void ToPackage(MexSoundGroup group, Stream stream)
        {
            using var zip = new ZipWriter(stream);
            zip.WriteAsJson("group.json", group);
            if (group.Scripts != null)
            {
                using var scriptStream = new MemoryStream();
                SEM.SaveSEMFile(scriptStream, new List<SEMBank>() { new () { Scripts = group.Scripts.ToArray() } });
                zip.Write("scripts.sem", scriptStream.ToArray());

                zip.WriteAsJson("scripts.json", group.Scripts.Select(e => e.Name).ToArray());
            }
            zip.Write(group.FileName, group.PackSSM());
        }
        /// <summary>
        /// 
        /// </summary>
        public static MexInstallerError? FromPackage(MexWorkspace workspace, Stream packageStream, out MexSoundGroup? group)
        {
            group = null;

            using ZipArchive zip = new (packageStream);

            // load group entry
            var entry = zip.GetEntry("group.json");
            if (entry == null)
                return new MexInstallerError("\"group.json\" was not found in zip");

            // parse group entry
            group = MexJsonSerializer.Deserialize<MexSoundGroup>(entry.Extract());
            if (group == null)
                return new MexInstallerError("Error parsing \"group.json\"");

            // load sounds
            {
                var sound_entry = zip.GetEntry(group.FileName);
                if (sound_entry != null)
                {
                    var ssm = new SSM();
                    using var ms = new MemoryStream(sound_entry.Extract());
                    ssm.Open(group.Name, ms);

                    for (int i = 0; i < ssm.Sounds.Length; i++)
                    {
                        if (i < group.Sounds.Count)
                        {
                            group.Sounds[i].DSP = ssm.Sounds[i];
                        }
                        else
                        {
                            group.Sounds.Add(new MexSound()
                            {
                                Name = $"Sound_{i:D3}",
                                DSP = ssm.Sounds[i],
                            });
                        }
                    }
                }
            }

            // load scripts
            {
                var script_entry = zip.GetEntry("scripts.sem");
                var script_names_entry = zip.GetEntry("scripts.json");
                string[]? script_names = null;
                
                if (script_names_entry != null)
                {
                    script_names = MexJsonSerializer.Deserialize<string[]>(script_names_entry.Extract());
                }

                if (script_entry != null)
                {
                    using var e = new MemoryStream(script_entry.Extract());
                    var sem = SEM.ReadSEMFile(e)[0];
                    group.Scripts = new ObservableCollection<SEMBankScript>();
                    for (int i = 0; i < sem.Scripts.Length; i++)
                    {
                        // load name
                        if (script_names != null &&
                            i < script_names.Length)
                            sem.Scripts[i].Name = script_names[i];

                        group.Scripts.Add(sem.Scripts[i]);
                    }
                }
            }

            // create ssm path
            var ssmPath = workspace.FileManager.GetUniqueFilePath(workspace.GetFilePath($"audio\\us\\{group.FileName}"));
            group.FileName = Path.GetFileName(ssmPath);

            // save ssm file
            workspace.FileManager.Set(ssmPath, group.PackSSM());

            return null;
        }
    }
}
