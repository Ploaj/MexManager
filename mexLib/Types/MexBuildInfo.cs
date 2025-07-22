using GCILib;
using HSDRaw.MEX;
using mexLib.AssetTypes;
using PropertyModels.ComponentModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public class MexBuildInfo : MexReactiveObject
    {
        private string _name = "m-ex";
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private byte _majorVersion = 1;
        [Range(0, 255)]
        public byte MajorVersion
        {
            get => _majorVersion;
            set
            {
                if (_majorVersion != value)
                {
                    _majorVersion = value;
                    OnPropertyChanged();
                }
            }
        }

        private byte _minorVersion = 0;
        public byte MinorVersion
        {
            get => _minorVersion;
            set
            {
                if (_minorVersion != value)
                {
                    _minorVersion = value;
                    OnPropertyChanged();
                }
            }
        }

        private byte _patchVersion = 0;
        public byte PatchVersion
        {
            get => _patchVersion;
            set
            {
                if (_patchVersion != value)
                {
                    _patchVersion = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _crashMessage = "";
        [MultilineText]
        public string CrashMessage
        {
            get => _crashMessage;
            set
            {
                if (_crashMessage != value)
                {
                    _crashMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _savefile = "SuperSmashBros011029033";
        public string SaveFile
        {
            get => _savefile;
            set
            {
                if (_savefile != value)
                {
                    _savefile = value;
                    OnPropertyChanged();
                }
            }
        }


        [Browsable(false)]
        [JsonInclude]
        public string? Banner { get => BannerAsset.AssetFileName; internal set => BannerAsset.AssetFileName = value; }

        [JsonIgnore]
        [Browsable(false)]
        public MexTextureAsset BannerAsset { get; set; } = new MexTextureAsset()
        {
            AssetPath = "banner",
            Width = 192,
            Height = 64,
            Format = HSDRaw.GX.GXTexFmt.RGB5A3,
            TlutFormat = HSDRaw.GX.GXTlutFmt.RGB5A3,
        };

        private string _shortName = "";
        public string ShortName
        {
            get => _shortName;
            set
            {
                if (_shortName != value)
                {
                    _shortName = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _longName = "";
        public string LongName
        {
            get => _longName;
            set
            {
                if (_longName != value)
                {
                    _longName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _shortMaker = "";
        public string ShortMaker
        {
            get => _shortMaker;
            set
            {
                if (_shortMaker != value)
                {
                    _shortMaker = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _longMaker = "";
        public string LongMaker
        {
            get => _longMaker;
            set
            {
                if (_longMaker != value)
                {
                    _longMaker = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadBanner(MexWorkspace ws)
        {
            if (Banner == null)
            {
                string bannerFilePath = ws.GetFilePath("opening.bnr");

                if (!ws.FileManager.Exists(bannerFilePath))
                    return;

                byte[] bannerFile = ws.FileManager.Get(bannerFilePath);

                if (bannerFile == null) return;

                GCBanner banner = new(bannerFile);

                ShortName = banner.MetaData.ShortName;
                ShortMaker = banner.MetaData.ShortMaker;
                LongName = banner.MetaData.LongName;
                LongMaker = banner.MetaData.LongMaker;
                Description = banner.MetaData.Description;

                byte[] pixels = banner.GetBannerImageRGBA8();
                SwapRedAndGreen(ref pixels);
                var img = new MexImage(pixels, 96, 32, HSDRaw.GX.GXTexFmt.RGB5A3, HSDRaw.GX.GXTlutFmt.IA8);
                BannerAsset.SetFromMexImage(ws, img);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        public void SaveBanner(MexWorkspace ws)
        {
            var img = BannerAsset.GetTexFile(ws);
            if (img == null)
                return;

            GCBanner banner = new()
            {
                MetaData = new GCBanner.MetaFooter()
                {
                    Description = Description,
                    ShortName = ShortName,
                    ShortMaker = ShortMaker,
                    LongName = LongName,
                    LongMaker = LongMaker,
                }
            };

            byte[] pixels = img.GetBgra();
            SwapRedAndGreen(ref pixels);
            banner.SetBannerImageRGBA8(pixels);

            ws.FileManager.Set(ws.GetFilePath("opening.bnr"), banner.GetData());
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        public void FromMxDt(MEX_Data d)
        {
            if (d.MetaData != null &&
                d.MetaData.BuildInfo != null)
            {
                var b = d.MetaData.BuildInfo;
                Name = b.BuildName;
                MajorVersion = b.MajorVersion;
                MinorVersion = b.MinorVersion;
                PatchVersion = b.PatchVersion;
                SaveFile = b.SaveFile;
                CrashMessage = b.CrashMessage.Replace("\\r\\n", Environment.NewLine).Replace("\\t", "\t");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MEX_BuildData ToMxDt()
        {
            return new MEX_BuildData()
            {
                BuildName = Name,
                MajorVersion = MajorVersion,
                MinorVersion = MinorVersion,
                PatchVersion = PatchVersion,
                SaveFile = SaveFile,
                CrashMessage = CrashMessage,
            };
        }
    }
}
