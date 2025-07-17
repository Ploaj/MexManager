using HSDRaw;

namespace mexLib.HsdObjects
{
    public class HSDFunctionDat : HSDAccessor
    {
        public override int TrimmedSize => 0x20;

        public byte[] Code { get => _s.GetBuffer(0x00); set => _s.SetBuffer(0x00, value); } //x00

        public HSDArrayAccessor<HSDFunctionRelocation> RelocationTable { get => _s.GetReference<HSDArrayAccessor<HSDFunctionRelocation>>(0x04); set => _s.SetReference(0x04, value); }

        public int RelocationCount { get => _s.GetInt32(0x08); set => _s.SetInt32(0x08, value); }

        public HSDArrayAccessor<HSDFunctionTable> FunctionTable { get => _s.GetReference<HSDArrayAccessor<HSDFunctionTable>>(0x0C); set => _s.SetReference(0x0C, value); }

        public int FunctionCount { get => _s.GetInt32(0x10); set => _s.SetInt32(0x10, value); }

        public int CodeLength { get => _s.GetInt32(0x14); set => _s.SetInt32(0x14, value); }

        public int DebugCount { get => _s.GetInt32(0x18); set => _s.SetInt32(0x18, value); }

        public HSDArrayAccessor<HSDFunctionDebug> DebugTable { get => _s.GetReference<HSDArrayAccessor<HSDFunctionDebug>>(0x1C); set => _s.SetReference(0x1C, value); }
    }

    public class HSDFunctionItemDat : HSDAccessor
    {
        public int Count { get => _s.GetInt32(0x00); set => _s.SetInt32(0x00, value); }

        public void SetItem(int index, HSDFunctionDat dat)
        {
            if (_s.Length == 0)
                _s.Resize(4);
            Count = Math.Max(Count, index + 1);
            _s.Resize(Count * 4 + 4);
            _s.SetReference(index * 4 + 4, dat);
        }
    }

    public class HSDFunctionRelocation : HSDAccessor
    {
        public override int TrimmedSize => 0x8;

        public byte Type { get => _s.GetByte(0x00); set => _s.SetByte(0x00, value); }

        public uint CodeOffset { get => (uint)(_s.GetInt32(0x00) & 0xFFFFFF); set => _s.SetInt32(0x00, (Type << 24) | ((int)value & 0xFFFFFF)); }

        public uint Address { get => (uint)_s.GetInt32(0x04); set => _s.SetInt32(0x04, (int)value); }
    }

    public class HSDFunctionTable : HSDAccessor
    {
        public override int TrimmedSize => 0x8;

        public uint Address { get => (uint)(_s.GetInt32(0x00)); set => _s.SetInt32(0x00, (int)value); }

        public uint CodeOffset { get => (uint)_s.GetInt32(0x04); set => _s.SetInt32(0x04, (int)value); }
    }

    public class HSDFunctionDebug : HSDAccessor
    {
        public override int TrimmedSize => 0xC;

        public uint CodeStartOffset { get => (uint)(_s.GetInt32(0x00)); set => _s.SetInt32(0x00, (int)value); }

        public uint CodeEndOffset { get => (uint)_s.GetInt32(0x04); set => _s.SetInt32(0x04, (int)value); }

        public string Symbol { get => _s.GetString(0x08); set => _s.SetString(0x08, value, true); }
    }

}
