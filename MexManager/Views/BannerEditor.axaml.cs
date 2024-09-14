using Avalonia;
using Avalonia.Controls;
using GCILib;
using mexLib;
using mexLib.Utilties;
using MexManager.Tools;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace MexManager.Views;

public partial class BannerEditor : Window
{
    public class BannerProxy
    {
        [DisplayName("Short Name")]
        [StringLength(32, ErrorMessage = "The Short Name value cannot exceed 31 characters.")]
        [Required(ErrorMessage = "Value Required")]
        public string ShortName { get; set; } = "";

        [DisplayName("Long Name")]
        [StringLength(64, ErrorMessage = "The Long Name value cannot exceed 64 characters.")]
        [Required(ErrorMessage = "Value Required")]
        public string LongName { get; set; } = "";

        [DisplayName("Short Maker")]
        [StringLength(32, ErrorMessage = "The Short Maker value cannot exceed 31 characters.")]
        [Required(ErrorMessage = "Value Required")]
        public string ShortMaker { get; set; } = "";

        [DisplayName("Long Maker")]
        [StringLength(64, ErrorMessage = "The Long Maker value cannot exceed 64 characters.")]
        [Required(ErrorMessage = "Value Required")]
        public string LongMaker { get; set; } = "";

        [StringLength(128, ErrorMessage = "The Description value cannot exceed 128 characters.")]
        [Required(ErrorMessage = "Value Required")]
        public string Description { get; set; } = "";

        [Browsable(false)]
        public MexImage? Image { get; set; }

        private readonly GCBanner? _banner;

        /// <summary>
        /// 
        /// </summary>
        public BannerProxy()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="banner"></param>
        public BannerProxy(GCBanner banner)
        {
            ShortName = banner.MetaData.ShortName;
            ShortMaker = banner.MetaData.ShortMaker;
            LongName = banner.MetaData.LongName;
            LongMaker = banner.MetaData.LongMaker;
            Description = banner.MetaData.Description;
            var pixels = banner.GetBannerImageRGBA8();
            SwapRedAndGreen(ref pixels);
            Image = new MexImage(pixels, 96, 32, HSDRaw.GX.GXTexFmt.RGB5A3, HSDRaw.GX.GXTlutFmt.IA8);
            _banner = banner;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public GCBanner? ToBanner()
        {
            if (_banner == null)
                return null;

            _banner.MetaData = new GCBanner.MetaFooter()
            {
                Description = Description,
                ShortName = ShortName,
                ShortMaker = ShortMaker,
                LongName = LongName,
                LongMaker = LongMaker,
            };
            if (Image != null)
            {
                var pixels = Image.GetBGRA();
                SwapRedAndGreen(ref pixels);
                _banner.SetBannerImageRGBA8(pixels);
            }
            return _banner;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rgba"></param>
        private static void SwapRedAndGreen(ref byte[] rgba)
        {
            for (int i = 0; i < rgba.Length; i += 4)
            {
                (rgba[i], rgba[i + 2]) = (rgba[i + 2], rgba[i]);
            }
        }
    }

    public bool SaveChanges { get; internal set; } = false;

    private BannerProxy? Banner;

    /// <summary>
    /// 
    /// </summary>
    public BannerEditor()
    {
        InitializeComponent();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="banner"></param>
    public void SetBanner(GCBanner banner)
    {
        Banner = new BannerProxy(banner);
        BannerPropertyGrid.DataContext = Banner;
        BannerImage.Source = Banner.Image?.ToBitmap();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GCBanner? GetBanner()
    {
        return Banner?.ToBanner();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SaveChanges = true;
        Close();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ExportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Banner == null)
            return;

        var file = await FileIO.TrySaveFile("Export Banner", "banner.png", FileIO.FilterPng);

        if (file == null)
            return;

        using var bmp = Banner.Image?.ToBitmap();
        bmp?.Save(file);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ImportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Banner == null)
            return;

        var file = await FileIO.TryOpenFile("Import Banner", "", FileIO.FilterPng);

        if (file == null)
            return;

        if (Banner.Image != null)
        {
            using var stream = new FileStream(file, FileMode.Open);
            Banner.Image = ImageConverter.FromPNG(stream, HSDRaw.GX.GXTexFmt.RGB5A3, HSDRaw.GX.GXTlutFmt.IA8);
            Banner.Image = ImageConverter.Resize(Banner.Image, 96, 32);
            BannerImage.Source = Banner.Image.ToBitmap();
        }
    }
}