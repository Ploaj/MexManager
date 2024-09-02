using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace MexManager.Tools
{
    public static class BitmapManager
    {
        public static Bitmap MeleeFighterImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/fighter_melee.png")));

        public static Bitmap MexFighterImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/fighter_mex.png")));

        public static Bitmap PlayIconImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/audio_play.png")));

        public static Bitmap PauseIconImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://MexManager/Assets/Common/audio_pause.png")));

    }
}
