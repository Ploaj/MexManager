using HSDRaw;

namespace mexLib.HsdObjects
{
    public class SISOffset : HSDAccessor
    {
        //// 0x002 0x374 - description
        //// 0x128 0x37A - source1
        //// 0x24E 0x380 - source2
        ///
        public override int TrimmedSize => 0x18;

        public int Desciption { get => _s.GetInt32(0x00); set => _s.SetInt32(0x00, value); }
        public int DesciptionAlt { get => _s.GetInt32(0x04); set => _s.SetInt32(0x04, value); }
        public int Src1 { get => _s.GetInt32(0x08); set => _s.SetInt32(0x08, value); }
        public int Src1Alt { get => _s.GetInt32(0x0C); set => _s.SetInt32(0x0C, value); }
        public int Src2 { get => _s.GetInt32(0x10); set => _s.SetInt32(0x10, value); }
        public int Src2Alt { get => _s.GetInt32(0x14); set => _s.SetInt32(0x14, value); }
    }
}
