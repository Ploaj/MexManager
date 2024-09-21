using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MeleeMedia.Video;
using System;
using System.Collections.Generic;
using System.IO;

namespace MexManager;

public partial class VideoPlayer : Window
{
    private MTHReader? _reader;

    private List<Bitmap> frames = new List<Bitmap>();

    private readonly DispatcherTimer _timer;

    private int _frameIndex = 0;

    /// <summary>
    /// 
    /// </summary>
    public VideoPlayer()
    {
        InitializeComponent();

        _timer = new DispatcherTimer();
        _timer.Tick += (sender, e) =>
        {
            // Switch to the next frame
            _frameIndex = (_frameIndex + 1) % frames.Count;
            VideoPanel.Source = frames[_frameIndex];
        };

        Closed += (s, e) =>
        {
            _reader?.Dispose();
            _timer.Stop();
        };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    public void SetVideo(string filePath)
    {
        _timer.Stop();

        var stream = new FileStream(filePath, FileMode.Open);
        _reader = new MTHReader(stream);

        for (int i = 0; i < _reader.FrameCount; i++)
        {
            using var ms = new MemoryStream(_reader.ReadFrame().ToJPEG());
            frames.Add(new Bitmap(ms));
        }

        VideoPanel.Source = frames[0];

        _timer.Interval = TimeSpan.FromSeconds(1.0 / _reader.FrameRate / 2);
        _timer.Start();
    }
}