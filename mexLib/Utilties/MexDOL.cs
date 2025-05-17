using HSDRaw;
using mexLib.Utilties;
using System.Runtime.InteropServices;
using System.Text;

namespace mexLib.MexScubber
{
    public class MexDOL
    {
        private readonly uint[] _sectionOffset = new uint[18];
        private readonly uint[] _sectionAddress = new uint[18];
        private readonly uint[] _sectionLengths = new uint[18];

        //private uint _bSSAddress;
        //private uint _bSSLength;

        private byte[] _data;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dol"></param>
        public MexDOL(byte[] dol)
        {
            _data = dol;
            using MemoryStream s = new(dol);
            using BinaryReaderExt r = new(s);
            {
                r.BigEndian = true;

                for (int i = 0; i < 18; i++)
                    _sectionOffset[i] = r.ReadUInt32();

                for (int i = 0; i < 18; i++)
                    _sectionAddress[i] = r.ReadUInt32();

                for (int i = 0; i < 18; i++)
                    _sectionLengths[i] = r.ReadUInt32();

                //_bSSAddress = r.ReadUInt32();
                //_bSSLength = r.ReadUInt32();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dolOffset"></param>
        /// <returns></returns>
        public uint ToAddr(uint dolOffset)
        {
            for (int i = 0; i < 18; i++)
            {
                if (_sectionOffset[i] == 0)
                    continue;

                if (dolOffset >= _sectionOffset[i] &&
                    dolOffset < _sectionOffset[i] + _sectionLengths[i])
                {
                    return dolOffset - _sectionOffset[i] + _sectionAddress[i];
                }
            }
            return 0x80000000;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="memAddr"></param>
        /// <returns></returns>
        public uint ToDol(uint memAddr)
        {
            for (int i = 0; i < 18; i++)
            {
                if (_sectionAddress[i] == 0)
                    continue;

                if (memAddr >= _sectionAddress[i] &&
                    memAddr < _sectionAddress[i] + _sectionLengths[i])
                {
                    return memAddr - _sectionAddress[i] + _sectionOffset[i];
                }
            }
            return 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetData(uint addr, int length)
        {
            // convert address to dol
            if ((addr & 0x80000000) != 0)
                addr = ToDol(addr);

            // copy section
            byte[] d = new byte[length];
            Array.Copy(_data, addr, d, 0, length);
            return d;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T GetStruct<T>(uint addr, uint index, int stride = -1)
        {
            // special case for strings
            if (typeof(T) == typeof(string))
            {
                uint off = GetStruct<uint>(addr, index, stride);
                return (T)(object)GetString(off);
            }

            // read struct
            int size = Marshal.SizeOf<T>();

            if (stride == -1)
                stride = size;

            byte[] byteArray = GetData((uint)(addr + index * stride), size);

            // Allocate unmanaged memory equal to the size of the struct
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                // Copy the byte array to the unmanaged memory
                Marshal.Copy(byteArray, 0, ptr, size);

                // Convert the unmanaged memory to the struct
                T? obj = Marshal.PtrToStructure<T>(ptr);

                // handle null and endianess swap
                if (obj == null)
                {
                    obj = Activator.CreateInstance<T>();
                }
                else
                {
                    obj = (T)EndianSwapUtilities.SwapEndianness(obj);
                }

                return obj;
            }
            finally
            {
                // Free the unmanaged memory
                Marshal.FreeHGlobal(ptr);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private string GetString(uint addr)
        {
            // convert address to dol
            if ((addr & 0x80000000) != 0)
                addr = ToDol(addr);

            StringBuilder b = new();

            while (addr < _data.Length
                && _data[addr] != 0x00)
            {
                b.Append((char)_data[addr++]);
            }

            return b.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ApplyPatch()
        {
            // check if patch already applied
            if (MexDolPatcher.CheckPatchApplied(_data))
                return true;

            // try to apply patch
            if (MexDolPatcher.TryApplyPatch(
                _data,
                out byte[] patched))
            {
                _data = patched;
                return true;
            }

            // patch apply failed
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public void Save(string filePath)
        {
            File.WriteAllBytes(filePath, _data);
        }
    }
}
