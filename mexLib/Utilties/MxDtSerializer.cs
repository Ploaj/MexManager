using HSDRaw;
using mexLib.Attributes;
using System.Reflection;
using System.Runtime.InteropServices;

namespace mexLib.Utilties
{
    public class MxDtSerializer
    {
        private HSDAccessor accessor;

        /// <summary>
        /// 
        /// </summary>
        public MxDtSerializer()
        {
            accessor = new HSDAccessor();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        public void Serialize(object? o, uint index)
        {
            //if (o == null)
            //    return;

            //var type = o.GetType();

            //foreach (var p in type.GetProperties())
            //{
            //    var attr = p.GetCustomAttribute<MxDtSourceAttribute>();

            //    if (attr == null)
            //        continue;

            //    var data = p.GetValue(o);

            //    if (data == null)
            //        return;

            //    SetData(accessor, attr.Path, 0, index, data);
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="path_index"></param>
        /// <param name="index"></param>
        /// <param name="data"></param>
        private void SetData(HSDAccessor a, int[] path, int path_index, uint index, object data)
        {
            if (path_index == path.Length)
            {
                EndianSwapUtilities.SwapEndianness(data);

                var type = data.GetType();
                var size = Marshal.SizeOf(data);
                var bytes = GetBytes(data);
                var offset = (int)index * size;

                if (a._s.Length <= offset + size)
                    a._s.Resize(offset + size);
                a._s.SetSubData(bytes, offset, size, 0x00);
            }
            else
            {
                int offset = path[path_index];
                if (a._s.Length < offset + 4)
                {
                    a._s.Resize(offset + 4);
                }
                var next = a._s.GetCreateReference<HSDAccessor>(offset);

                SetData(next, path, path_index + 1, index, data);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] GetBytes(object obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                // Copy the struct into unmanaged memory.
                Marshal.StructureToPtr(obj, ptr, true);
                // Copy the unmanaged memory to the byte array.
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(ptr);
            }

            return arr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public void Save(string filePath)
        {
            HSDRawFile f = new HSDRawFile();
            f.Roots.Add(new HSDRootNode()
            {
                Name = "mexData",
                Data = accessor
            });
            f.Save(filePath);
        }
    }
}
