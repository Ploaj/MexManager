using HSDRaw;

namespace mexLib.Utilties
{
    public enum SemCode
    {
        Timer,
        Sound,
        LoopStart,
        LoopEnd,
        SetPriority,
        AddPriority,
        SetVolume,
        AddVolume,
        SetPanning,
        AddPanning,
        SetUnused1,
        AddUnused1,
        SetPitch,
        AddPitch,
        End,
        Stop,
        SetReverb,
        AddReverb,
        SetUnused2,
        AddUnused2,
        StarReverb,
        Unused3,

        ForcedTimer,

        Null = 0xFD,
    }

    public class SemCommand : MexReactiveObject
    {
        /// <summary>
        /// 
        /// </summary>
        public SemCode SemCode { get => _semCode; set { _semCode = value; Value = _value; OnPropertyChanged(); } }
        private SemCode _semCode;
        /// <summary>
        /// 
        /// </summary>
        public int MaxValue => SemCode switch
        {
            SemCode.Timer or SemCode.ForcedTimer or
            SemCode.Sound => 0xFFFFFF,
            SemCode.SetPriority or SemCode.AddPriority => 28,
            SemCode.SetPitch or SemCode.AddPitch => 2400,
            SemCode.SetVolume or SemCode.AddVolume or
            SemCode.SetPanning or SemCode.AddPanning or
            SemCode.SetUnused1 or SemCode.AddUnused1 or
            SemCode.SetReverb or SemCode.AddReverb or
            SemCode.SetUnused2 or SemCode.AddUnused2 or
            SemCode.StarReverb or
            SemCode.Unused3 => 255,
            SemCode.LoopStart or
            SemCode.LoopEnd or
            SemCode.Stop => 0,
            _ => 0
        };
        /// <summary>
        /// 
        /// </summary>
        public int MinValue => SemCode switch
        {
            SemCode.Timer or SemCode.ForcedTimer or
            SemCode.Sound => 0,
            SemCode.AddPriority or SemCode.SetPriority => 5,
            SemCode.AddPitch or SemCode.SetPitch => -10800,
            SemCode.SetVolume or SemCode.AddVolume or
            SemCode.SetPanning or SemCode.AddPanning or
            SemCode.SetUnused1 or SemCode.AddUnused1 or
            SemCode.SetReverb or SemCode.AddReverb or
            SemCode.SetUnused2 or SemCode.AddUnused2 or
            SemCode.StarReverb or
            SemCode.Unused3 => 0,
            SemCode.LoopStart or
            SemCode.LoopEnd or
            SemCode.Stop => 0,
            _ => 0
        };
        /// <summary>
        /// 
        /// </summary>
        public bool HasValue => SemCode switch
        {
            SemCode.LoopEnd or
            SemCode.Stop => false,
            _ => true
        };
        /// <summary>
        /// 
        /// </summary>
        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Clamp(value, MinValue, MaxValue);
                OnPropertyChanged();
            }
        }
        private int _value;

        /// <summary>
        /// 
        /// </summary>
        public SemCommand()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="semCode"></param>
        /// <param name="value"></param>
        public SemCommand(SemCode semCode, int value)
        {
            _semCode = semCode;
            _value = value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="copyfrom"></param>
        public SemCommand(SemCommand copyfrom)
        {
            _semCode = copyfrom._semCode;
            _value = copyfrom._value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CanHoldTime(int time) => SemCode switch
        {
            SemCode.SetVolume or SemCode.AddVolume or SemCode.SetPriority or SemCode.AddPriority or
            SemCode.SetPanning or SemCode.AddPanning or SemCode.SetUnused1 or SemCode.AddUnused1 or
            SemCode.SetReverb or SemCode.AddReverb => time <= ushort.MaxValue,

            SemCode.SetPitch or SemCode.AddPitch => time <= 0xFF,

            _ => false
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="timer_value"></param>
        /// <returns></returns>
        public int Pack(Stream s, int timer_value)
        {
            if (SemCode == SemCode.ForcedTimer)
                s.WriteByte((byte)0);
            else
                s.WriteByte((byte)SemCode);

            // pack timer value
            switch (SemCode)
            {
                case SemCode.SetPriority:
                case SemCode.AddPriority:
                case SemCode.SetVolume:
                case SemCode.AddVolume:
                case SemCode.SetPanning:
                case SemCode.AddPanning:
                case SemCode.SetUnused1:
                case SemCode.AddUnused1:
                case SemCode.SetReverb:
                case SemCode.AddReverb:
                    if (timer_value < ushort.MaxValue)
                    {
                        s.WriteByte((byte)((timer_value >> 8) & 0xFF));
                        s.WriteByte((byte)(timer_value & 0xFF));
                        timer_value = 0;
                    }
                    else
                    {
                        s.WriteByte(0);
                        s.WriteByte(0);
                    }

                    s.WriteByte((byte)(Value & 0xFF));
                    break;
                case SemCode.SetPitch:
                case SemCode.AddPitch:
                    if (timer_value < byte.MaxValue)
                    {
                        s.WriteByte((byte)(timer_value & 0xFF));
                        timer_value = 0;
                    }
                    else
                    {
                        s.WriteByte(0);
                    }

                    s.WriteByte((byte)((Value >> 8) & 0xFF));
                    s.WriteByte((byte)(Value & 0xFF));
                    break;
                case SemCode.Timer:
                case SemCode.ForcedTimer:
                case SemCode.Sound:
                case SemCode.LoopStart:
                case SemCode.LoopEnd:
                case SemCode.Stop:
                case SemCode.End:
                case SemCode.SetUnused2:
                case SemCode.AddUnused2:
                case SemCode.StarReverb:
                case SemCode.Unused3:
                default:
                    s.WriteByte((byte)((Value >> 16) & 0xFF));
                    s.WriteByte((byte)((Value >> 8) & 0xFF));
                    s.WriteByte((byte)(Value & 0xFF));
                    break;
            }

            return timer_value;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SemScript : MexReactiveObject
    {
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        private string _name = "SFX";

        public List<SemCommand> Script { get; set; } = new List<SemCommand>();

        /// <summary>
        /// 
        /// </summary>
        public SemScript()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="copyfrom"></param>
        public SemScript(SemScript copyfrom)
        {
            _name = copyfrom._name;
            Script.AddRange(copyfrom.Script.Select(e => new SemCommand(e)));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public SemScript(byte[] data)
        {
            if (data[^4] != 14 && 
                data[^4] != 15 &&
                data[^4] != 253)
            {
                throw new Exception();
            }

            // decompile commands
            for (int i = 0; i < data.Length; i += 4)
            {
                var command = (SemCode)data[i];
                int timer;
                int value;

                // read timer and values
                switch (command)
                {
                    case SemCode.Timer:
                        value = 0;
                        timer = ((data[i + 1] & 0xFF) << 16) | ((data[i + 2] & 0xFF) << 8) | (data[i + 3] & 0xFF);
                        break;
                    case SemCode.SetPriority:
                    case SemCode.AddPriority:
                    case SemCode.SetVolume:
                    case SemCode.AddVolume:
                    case SemCode.SetPanning:
                    case SemCode.AddPanning:
                    case SemCode.SetUnused1:
                    case SemCode.AddUnused1:
                    case SemCode.SetReverb:
                    case SemCode.AddReverb:
                        timer = ((data[i + 1] & 0xFF) << 8) | (data[i + 2] & 0xFF);
                        value = data[i + 3];
                        break;
                    case SemCode.SetPitch:
                    case SemCode.AddPitch:
                        timer = (data[i + 1] & 0xFF);
                        value = ((data[i + 2] & 0xFF) << 8) | (data[i + 3] & 0xFF);
                        break;
                    case SemCode.Sound:
                    case SemCode.LoopStart:
                    case SemCode.LoopEnd:
                    case SemCode.Stop:
                    case SemCode.End:
                    case SemCode.SetUnused2:
                    case SemCode.AddUnused2:
                    case SemCode.StarReverb:
                    case SemCode.Unused3:
                    default:
                        value = ((data[i + 1] & 0xFF) << 16) | ((data[i + 2] & 0xFF) << 8) | (data[i + 3] & 0xFF);
                        timer = 0;
                        break;
                }

                if (command == SemCode.Timer)
                {
                    Script.Add(new SemCommand(SemCode.ForcedTimer, timer));
                }
                else
                if (timer != 0)
                {
                    Script.Add(new SemCommand(SemCode.Timer, timer));
                }

                if (command != SemCode.Timer)
                {
                    Script.Add(new SemCommand(command, value));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundid"></param>
        public void RemoveSoundID(int soundid)
        {
            foreach (var v in Script)
            {
                if (v.SemCode == SemCode.Sound)
                {
                    if (v.Value == soundid)
                        v.Value = 0;
                    else
                    if (v.Value > soundid)
                        v.Value -= 1;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetFirstSoundID()
        {
            foreach (var v in Script)
            {
                if (v.SemCode == SemCode.Sound)
                {
                    return v.Value;
                }
            }
            return -1;
        }
        /// <summary>
        /// Adjusts all sound commands by the given amount
        /// </summary>
        public void AdjustSoundOffset(int amount)
        {
            foreach (var v in Script)
            {
                if (v.SemCode == SemCode.Sound)
                {
                    v.Value += amount;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void CleanScripts()
        {
            foreach (var v in Script)
            {
                if (v.SemCode == SemCode.ForcedTimer)
                    v.SemCode = SemCode.Timer;
            }
            Script.RemoveAll(e => e.SemCode == SemCode.Null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] Compile()
        {
            using var stream = new MemoryStream();

            // write commands
            int pending_timer_value = 0;
            for (int i = 0; i < Script.Count; i++)
            {
                if (Script[i].SemCode == SemCode.Timer &&
                    i + 1 < Script.Count &&
                    Script[i + 1].CanHoldTime(Script[i].Value))
                {
                    pending_timer_value = Script[i].Value;
                }
                else
                {
                    pending_timer_value = Script[i].Pack(stream, pending_timer_value);
                }
            }

            // make sure script ends
            if (Script.Count > 0)
            {
                var endScriptCode = Script[^1].SemCode;

                if (endScriptCode != SemCode.End ||
                    endScriptCode != SemCode.Stop ||
                    endScriptCode != SemCode.Null)
                {
                    new SemCommand(SemCode.End, 0).Pack(stream, pending_timer_value);
                }
            }

            return stream.ToArray();
        }
    }

    public static class SemFile
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="groups"></param>
        public static void Compile(Stream stream, IEnumerable<SemScript[]> groups)
        {
            var g = groups.ToList();

            using BinaryWriterExt w = new (stream);
            w.BigEndian = true;

            // write header
            w.Write(0);
            w.Write(0);
            w.Write(g.Count);
            int index = 0;
            foreach (var e in g)
            {
                w.Write(index);
                index += e.Length;
            }
            w.Write(index);

            // calculate and write offsets for script data
            long data_start = stream.Position + (4 * (index + 1));
            foreach (var e in g)
            {
                foreach (var s in e)
                {
                    w.Write((uint)data_start);

                    var temp = w.BaseStream.Position;
                    w.Seek((uint)data_start);
                    w.Write(s.Compile());
                    data_start = w.BaseStream.Position;
                    w.Seek((uint)temp);
                }
            }
            w.Write(0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IEnumerable<SemScript[]> Decompile(Stream stream)
        {
            using BinaryReaderExt r = new (stream);
            r.BigEndian = true;

            r.Seek(8); // skip header
            int count = r.ReadInt32();
            var counts = new int[count + 1];
            for (int i = 0; i < counts.Length; i++)
                counts[i] = r.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var start = counts[i];
                var end = counts[i + 1];

                var group = new SemScript[end - start];
                for (int j = start; j < end; j++)
                {
                    var data_start = r.ReadInt32();
                    var data_end = r.ReadInt32();
                    r.Position -= 4;

                    if (data_end == 0)
                        data_end = (int)stream.Length;

                    var data = r.GetSection((uint)data_start, data_end - data_start);

                    group[j - start] = new SemScript(data);
                }

                yield return group;
            }
        }
    }
}
