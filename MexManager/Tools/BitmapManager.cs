using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MexManager.Tools
{
    public static class BitmapManager
    {
        private static List<Bitmap> _bitmaps = new List<Bitmap>();

        public static Bitmap MeleeFighterImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/fighter_melee.png")));

        public static Bitmap MexFighterImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/fighter_melee.png")));

        //public static Bitmap GetStockIcon()
        //{

        //}

        public static void Free()
        {

        }
    }
}
