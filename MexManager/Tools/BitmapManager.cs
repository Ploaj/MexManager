using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace MexManager.Tools
{
    public static class BitmapManager
    {
        public static Bitmap MissingImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/fighter_melee.png")));

        public static Bitmap MeleeFighterImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/fighter_melee.png")));

        public static Bitmap MexFighterImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/fighter_mex.png")));

        public static Bitmap PlayIconImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/audio_play.png")));

        public static Bitmap PauseIconImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/audio_pause.png")));

        public static Bitmap Plus { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/icon_plus.png")));

        public static Bitmap Minus { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/icon_minus.png")));

        public static Bitmap ArrowUp { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/icon_dir_up.png")));

        public static Bitmap ArrowDown { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/icon_dir_down.png")));

    }
}
