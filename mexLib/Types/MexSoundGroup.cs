using HSDRaw;
using HSDRaw.Common;
using HSDRaw.MEX;
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
        [DisplayName("File")]
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

        private ObservableCollection<MexSound> _sounds = new();
        [Browsable(false)]
        public ObservableCollection<MexSound> Sounds { get => _sounds; set { _sounds = value; OnPropertyChanged(); } }

        private ObservableCollection<SemScript>? _scripts = null;
        [JsonIgnore]
        [Browsable(false)]
        public ObservableCollection<SemScript>? Scripts { get => _scripts; set { _scripts = value; OnPropertyChanged(); } }

        private uint _bufferSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dol"></param>
        /// <param name="index"></param>
        public void FromDOL(MexDOL dol, uint index)
        {
            // SSMFiles
            FileName = dol.GetStruct<string>(0x803bbcfc, index);
            Name = Path.GetFileNameWithoutExtension(FileName).FirstCharToUpper();

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
        public uint ToMxDt(MexGenerator gen, int index)
        {
            MEX_SSMTable st = gen.Data.SSMTable;

            // getting the actual buffer size is "more correct"
            //using FileStream stream = new(gen.Workspace.GetFilePath("audio//us//" + FileName), FileMode.Open);
            //stream.Seek(0x04, SeekOrigin.Begin);
            //int bufferSize = ((stream.ReadByte() & 0xFF) << 24) | ((stream.ReadByte() & 0xFF) << 16) | ((stream.ReadByte() & 0xFF) << 8) | (stream.ReadByte() & 0xFF);

            //var bufferSize = (int)gen.Workspace.GetFileSize("audio//us//" + FileName);
            var bufferSize = _bufferSize;
            if (bufferSize % 0x20 != 0)
                bufferSize += 0x20 - (bufferSize % 0x20);

            st.SSM_SSMFiles.Set(index, new HSD_String(FileName));
            st.SSM_BufferSizes.Set(index, new MEX_SSMSizeAndFlags()
            {
                Flag = (int)Flags,
                SSMFileSize = (int)bufferSize,
            });
            st.SSM_LookupTable.Set(index, new MEX_SSMLookup()
            {
                EntireFlag = (int)GroupFlags,
            });

            return bufferSize;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mxdt"></param>
        /// <param name="index"></param>
        public void FromMxDt(MEX_Data mxdt, int index)
        {
            MEX_SSMTable st = mxdt.SSMTable;

            FileName = st.SSM_SSMFiles[index].Value;
            Flags = (uint)st.SSM_BufferSizes[index].Flag;
            _bufferSize = (uint)st.SSM_BufferSizes[index].SSMFileSize;
            GroupFlags = (uint)st.SSM_LookupTable[index].EntireFlag;
            Name = Path.GetFileNameWithoutExtension(FileName).FirstCharToUpper();
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
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] PackSSM(int start_offset)
        {
            SSM ssm = new()
            {
                Name = Name,
                StartIndex = start_offset,
                Sounds = Sounds.Select(e => e.DSP).ToArray(),
            };

            using MemoryStream stream = new();
            ssm.WriteToStream(stream, out int bs);
            _bufferSize = (uint)bs;
            return stream.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public void ImportSSM(MexWorkspace workspace, string fullPath, bool replace)
        {
            SSM ssm = new();
            ssm.Open(Path.GetFileName(fullPath), workspace.FileManager.GetStream(fullPath));
            _bufferSize = ssm.BufferSize;

            if (replace)
                Sounds.Clear();
            int index = 0;
            foreach (DSP? s in ssm.Sounds)
                Sounds.Add(new MexSound() { Name = $"Sound_{index++:D3}", DSP = s });

            OnPropertyChanged(nameof(Sounds));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        public void CopyFrom(MexSoundGroup group)
        {
            Flags = group.Flags;
            Group = group.Group;
            Type = group.Type;
            SubType = group.SubType;
            Mushroom = group.Mushroom;
            _bufferSize = group._bufferSize;

            if (Scripts != null &&
                group.Scripts != null)
            {
                Scripts.Clear();
                foreach (SemScript s in group.Scripts)
                    Scripts.Add(new SemScript(s));
            }

            Sounds.Clear();
            foreach (MexSound s in group.Sounds)
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
            using ZipWriter zip = new(stream);
            zip.WriteAsJson("group.json", group);
            if (group.Scripts != null)
            {
                using MemoryStream scriptStream = new();
                SemFile.Compile(scriptStream, new List<SemScript[]> { group.Scripts.ToArray() });
                zip.Write("scripts.sem", scriptStream.ToArray());

                zip.WriteAsJson("scripts.json", group.Scripts.Select(e => e.Name).ToArray());
            }
            zip.Write(group.FileName, group.PackSSM(0));
        }
        /// <summary>
        /// 
        /// </summary>
        public static MexInstallerError? FromPackage(MexWorkspace workspace, Stream packageStream, out MexSoundGroup? group)
        {
            group = null;

            using ZipArchive zip = new(packageStream);

            // load group entry
            ZipArchiveEntry? entry = zip.GetEntry("group.json");
            if (entry == null)
                return new MexInstallerError("\"group.json\" was not found in zip");

            // parse group entry
            group = MexJsonSerializer.Deserialize<MexSoundGroup>(entry.Extract());
            if (group == null)
                return new MexInstallerError("Error parsing \"group.json\"");

            // load sounds
            {
                ZipArchiveEntry? sound_entry = zip.GetEntry(group.FileName);
                if (sound_entry != null)
                {
                    SSM ssm = new();
                    using MemoryStream ms = new(sound_entry.Extract());
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
                ZipArchiveEntry? script_entry = zip.GetEntry("scripts.sem");
                ZipArchiveEntry? script_names_entry = zip.GetEntry("scripts.json");
                string[]? script_names = null;

                if (script_names_entry != null)
                {
                    script_names = MexJsonSerializer.Deserialize<string[]>(script_names_entry.Extract());
                }

                if (script_entry != null)
                {
                    using MemoryStream e = new(script_entry.Extract());
                    SemScript[] sem = SemFile.Decompile(e).ToArray()[0];
                    group.Scripts = new ObservableCollection<SemScript>();
                    for (int i = 0; i < sem.Length; i++)
                    {
                        // load name
                        if (script_names != null &&
                            i < script_names.Length)
                            sem[i].Name = script_names[i];

                        group.Scripts.Add(sem[i]);
                    }
                }
            }

            // create ssm path
            string ssmPath = workspace.FileManager.GetUniqueFilePath(workspace.GetFilePath($"audio/us/{group.FileName}"));
            group.FileName = Path.GetFileName(ssmPath);

            // save ssm file
            workspace.FileManager.Set(ssmPath, group.PackSSM(0));

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static MexSoundGroup? FromSPK(Stream s)
        {
            using BinaryReaderExt r = new (s);
            
            if (s.Length < 0x14)
                return null;

            if (new string(r.ReadChars(3)) != "SPK")
                return null;

            MexSoundGroup g = new ();

            int version = 1;
            if (r.ReadChar() == '2')
                version = 2;

            g.GroupFlags = r.ReadUInt32();
            g.Flags = r.ReadUInt32();

            var ssmSize = r.ReadInt32();
            g.Scripts = new System.Collections.ObjectModel.ObservableCollection<SemScript>();
            var script_count = r.ReadInt32();

            for (int i = 0; i < script_count; i++)
            {
                var start = r.ReadUInt32();
                var size = r.ReadInt32();
                var script_name = "New Script";
                if (version > 1)
                {
                    var temp = r.Position + 4;
                    r.Position = r.ReadUInt32();
                    script_name = r.ReadString(r.ReadChar());
                    r.Position = temp;
                }

                g.Scripts.Add(new SemScript(r.GetSection(start, size))
                {
                    Name = script_name
                });
            }

            g.FileName = r.ReadString(r.ReadChar());
            g.Name = g.FileName;
            {
                using MemoryStream ssmStream = new (r.ReadBytes(ssmSize));
                
                SSM ssm = new();
                ssm.Open(g.FileName, ssmStream);
                g._bufferSize = ssm.BufferSize;

                int index = 0;
                foreach (DSP? so in ssm.Sounds)
                    g.Sounds.Add(new MexSound() { Name = $"Sound_{index++:D3}", DSP = so });
            }

            return g;
        }
    }
}
