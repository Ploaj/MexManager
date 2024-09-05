using HSDRaw;

namespace mexLib.Utilties
{
    public static class ArchiveTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsValidHSDFile(Stream file)
        {
            if (file == null)
                return false;

            // check header length
            if (file.Length <= 0x20)
                return false;

            // check filesize
            file.Position = 0;
            var size = ((file.ReadByte() & 0xFF) << 24) | ((file.ReadByte() & 0xFF) << 16) | ((file.ReadByte() & 0xFF) << 8) | (file.ReadByte() & 0xFF);

            if (file.Length != size)
                return false;

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hsdFile"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetSymbols(Stream hsdFile)
        {
            using var f = new BinaryReaderExt(hsdFile);
            f.BigEndian = true;

            f.Position = 0;
            var size = f.ReadInt32();
            var reloc = f.ReadInt32() + 0x20;
            var reloc_count = f.ReadInt32();
            var symbol_count = f.ReadInt32();

            var string_table_offset = reloc + reloc_count * 4 + symbol_count * 8;

            for (int i = 0; i < symbol_count; i++)
            {
                f.Position = (uint)(reloc + reloc_count * 4 + i * 8) + 4;
                var string_off = f.ReadInt32();
                var symbol = f.ReadString(string_table_offset + string_off, -1);
                yield return symbol;
            }
        }
    }
}
