using HSDRaw.GX;

namespace mexLib.Attributes
{
    public class MexTextureAssetAttribute : Attribute
    {
        public MexTextureAssetAttribute()
        {
        }
    }

    public class MexTextureFormatAttribute : Attribute
    {
        public GXTexFmt Format { get; set; }

        public GXTlutFmt TlutFormat { get; set; }

        public MexTextureFormatAttribute(GXTexFmt format, GXTlutFmt tlutFormat)
        {
            Format = format;
            TlutFormat = tlutFormat;
        }
        public MexTextureFormatAttribute(GXTexFmt format)
        {
            Format = format;
            TlutFormat = GXTlutFmt.RGB5A3;
        }
    }

    public class MexTextureSize : Attribute
    {
        public int Width { get; internal set; }

        public int Height { get; internal set; }

        public MexTextureSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
