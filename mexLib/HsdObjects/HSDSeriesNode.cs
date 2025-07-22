using HSDRaw;
using HSDRaw.MEX.Sounds;

namespace mexLib.HsdObjects
{
    public class HSDSeriesNode : HSDAccessor
    {
        public override int TrimmedSize => 0x20;

        public int Version { get => _s.GetInt32(0x00); set => _s.SetInt32(0x00, value); }

        public int SeriesCount { get => _s.GetInt32(0x04); set => _s.SetInt32(0x04, value); }

        public HSDNullPointerArrayAccessor<HSDSeries> Series { get => _s.GetCreateReference<HSDNullPointerArrayAccessor<HSDSeries>>(0x08); }

        public HSDArrayAccessor<HSDSeriesLookup> StageLookup { get => _s.GetCreateReference<HSDArrayAccessor<HSDSeriesLookup>>(0x0C); }
    }

    public class HSDSeries : HSDAccessor
    {
        public override int TrimmedSize => 0x20;

        public int SeriesID { get => _s.GetInt32(0x00); set => _s.SetInt32(0x00, value); }

        public string SeriesName { get => _s.GetString(0x04); set => _s.SetString(0x04, value); }

        public MEX_Playlist Playlist { get => _s.GetReference<MEX_Playlist>(0x08); set => _s.SetReference(0x08, value); }
    }

    public class HSDSeriesLookup : HSDAccessor
    {
        public override int TrimmedSize => 4;

        public ushort ExternalID { get => _s.GetUInt16(0x00); set => _s.SetUInt16(0x00, value); }

        public ushort SeriesID { get => _s.GetUInt16(0x02); set => _s.SetUInt16(0x02, value); }
    }
}
