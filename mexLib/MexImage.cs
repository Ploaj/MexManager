using GCILib;
using HSDRaw.Common;
using HSDRaw.GX;
using HSDRaw.Tools;
using mexLib.Utilties;

namespace mexLib
{
    public class MexImage
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public byte[] ImageData { get; internal set; }

        public byte[] PaletteData { get; internal set; }

        public GXTexFmt Format { get; internal set; }

        public GXTlutFmt TlutFormat { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rgba"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fmt"></param>
        /// <param name="tlutFmt"></param>
        public MexImage(byte[] rgba, int width, int height, GXTexFmt fmt, GXTlutFmt tlutFmt)
        {
            Width = width;
            Height = height;
            Format = fmt;
            TlutFormat = tlutFmt;

            byte[] image = GXImageConverter.EncodeImage(rgba, width, height, fmt, tlutFmt, out byte[] pal);

            ImageData = image;
            if (GXImageConverter.IsPalettedFormat(fmt))
            {
                PaletteData = pal;
            }
            else
            {
                PaletteData = Array.Empty<byte>();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fmt"></param>
        /// <param name="tlutFmt"></param>
        public MexImage(int width, int height, GXTexFmt fmt, GXTlutFmt tlutFmt)
        {
            Width = width;
            Height = height;
            Format = fmt;
            TlutFormat = tlutFmt;

            ImageData = new byte[GXImageConverter.GetImageSize(fmt, width, height)];
            if (GXImageConverter.IsPalettedFormat(fmt))
            {
                if (fmt == GXTexFmt.CI4)
                {
                    PaletteData = new byte[16 * 2];
                }
                else
                {
                    PaletteData = new byte[256 * 2];
                }
            }
            else
            {
                PaletteData = Array.Empty<byte>();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tobj"></param>
        public MexImage(HSD_TOBJ tobj) : this(8, 8, GXTexFmt.I8, GXTlutFmt.IA8)
        {
            if (tobj.ImageData != null)
            {
                Width = tobj.ImageData.Width;
                Height = tobj.ImageData.Height;
                Format = tobj.ImageData.Format;
                ImageData = tobj.ImageData.ImageData;
            }

            if (tobj.TlutData != null)
            {
                TlutFormat = tobj.TlutData.Format;
                PaletteData = tobj.TlutData.TlutData;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static MexImage FromByteArray(byte[] data)
        {
            MexImage tex = new(8, 8, GXTexFmt.I8, GXTlutFmt.IA8);
            using MemoryStream s = new(data);
            tex.Open(s);
            return tex;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBGRA()
        {
            return GXImageConverter.DecodeTPL(Format, Width, Height, ImageData, TlutFormat, PaletteData.Length / 2, PaletteData);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool Open(Stream stream)
        {
            if (stream.ReadByte() != 'T' || stream.ReadByte() != 'E' || stream.ReadByte() != 'X')
                return false;

            stream.ReadByte(); // compression flag

            Format = (GXTexFmt)stream.ReadByte();
            TlutFormat = (GXTlutFmt)stream.ReadByte();
            stream.ReadByte(); // padding
            stream.ReadByte(); // padding

            Width = BitConverter.ToInt32(stream.ReadBytes(4), 0);
            Height = BitConverter.ToInt32(stream.ReadBytes(4), 0);

            BitConverter.ToInt32(stream.ReadBytes(4), 0);
            uint img_length = BitConverter.ToUInt32(stream.ReadBytes(4), 0);
            BitConverter.ToInt32(stream.ReadBytes(4), 0);
            uint pal_length = BitConverter.ToUInt32(stream.ReadBytes(4), 0);

            ImageData = stream.ReadBytes(img_length);
            PaletteData = stream.ReadBytes(pal_length);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="compress"></param>
        public void Save(string filePath, bool compress = false)
        {
            using FileStream stream = new(filePath, FileMode.Create);
            Write(stream, compress);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="compress"></param>
        public byte[] ToByteArray(bool compress = false)
        {
            using MemoryStream stream = new();
            Write(stream, compress);
            return stream.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="compress"></param>
        public void Write(Stream stream, bool compress = false)
        {
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'E');
            stream.WriteByte((byte)'X');
            stream.WriteByte(compress ? (byte)1 : (byte)0);

            stream.WriteByte((byte)Format);
            stream.WriteByte((byte)TlutFormat);
            stream.WriteByte(0);
            stream.WriteByte(0);

            stream.Write(BitConverter.GetBytes(Width), 0, 4);
            stream.Write(BitConverter.GetBytes(Height), 0, 4);

            stream.Write(BitConverter.GetBytes(0x20), 0, 4);
            stream.Write(BitConverter.GetBytes(ImageData.Length), 0, 4);
            stream.Write(BitConverter.GetBytes(0x20 + ImageData.Length), 0, 4);
            stream.Write(BitConverter.GetBytes(PaletteData.Length), 0, 4);

            stream.Write(ImageData, 0, ImageData.Length);
            stream.Write(PaletteData, 0, PaletteData.Length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal byte[] ToPNG()
        {
            return ImageConverter.ConvertBgraToPng(GXImageConverter.DecodeTPL(Format, Width, Height, ImageData, TlutFormat, PaletteData.Length / 2, PaletteData),
                Width,
                Height);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal HSD_TOBJ ToTObj()
        {
            HSD_TOBJ tobj = new()
            {
                MagFilter = GXTexFilter.GX_LINEAR,
                GXTexGenSrc = GXTexGenSrc.GX_TG_TEX0,
                RepeatS = 1,
                RepeatT = 1,
                DiffuseLightmap = true,
                CoordType = COORD_TYPE.UV,
                ColorOperation = COLORMAP.MODULATE,
                AlphaOperation = ALPHAMAP.MODULATE,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                SX = 1,
                SY = 1,
                SZ = 1,
                Blending = 1,
                ImageData = new HSD_Image()
                {
                    Width = (short)Width,
                    Height = (short)Height,
                    Format = Format,
                    ImageData = ImageData,
                },
                TlutData = GXImageConverter.IsPalettedFormat(Format) ?
                new HSD_Tlut()
                {
                    Format = TlutFormat,
                    ColorCount = (short)(PaletteData.Length / 2),
                    TlutData = PaletteData,
                } : null
            };
            tobj.Optimize();
            return tobj;
        }
    }
}
