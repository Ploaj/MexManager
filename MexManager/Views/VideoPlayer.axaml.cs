using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MeleeMedia.Video;
using mexLib;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MexManager;

public class VideoPlayerContext : MexReactiveObject
{
    public bool IsPlaying { get => _isPlaying; set { _isPlaying = value; OnPropertyChanged(); } }
    private bool _isPlaying;

    public int Progress { get => _progress; set { _progress = value; OnPropertyChanged(); } }
    private int _progress;
}

public partial class VideoPlayer : Window
{
    private MTHReader? _reader;

    private readonly Queue<Bitmap> frameBuffer = new ();

    private readonly DispatcherTimer _timer;

    private int _frameIndex = 0;

    private readonly int _bufferSize = 10;
    private readonly int _preloadThreshold = 4;

    private VideoPlayerContext Context;

    /// <summary>
    /// 
    /// </summary>
    public VideoPlayer()
    {
        InitializeComponent();

        Context = new VideoPlayerContext();
        DataContext = Context;

        _timer = new DispatcherTimer();
        _timer.Tick += (s, e) => {
            NextFrame(null, new RoutedEventArgs());
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
    private void NextFrame(object? sender, RoutedEventArgs args)
    {
        if (_reader == null)
            return;

        // Display the next frame
        if (frameBuffer.Count > 0)
        {
            // Dispose of the previous frame if necessary
            var oldFrame = VideoPanel.Source as Bitmap;
            oldFrame?.Dispose();

            VideoPanel.Source = frameBuffer.Dequeue();
        }

        // Preload more frames if needed
        if (frameBuffer.Count < _preloadThreshold)
        {
            Task.Run(() => PreloadFrames());
        }

        _frameIndex = (_frameIndex + 1) % _reader.FrameCount;
        Context.Progress = (int)((_frameIndex / (double)_reader.FrameCount) * 100);
    }
    /// <summary>
    /// 
    /// </summary>
    private void PreloadFrames()
    {
        if (_reader == null)
            return;

        for (int i = 0; i < _bufferSize - frameBuffer.Count; i++)
        {
            // Calculate the next frame to load
            using var ms = new MemoryStream(_reader.ReadFrame().ToJPEG());
            var bitmap = new Bitmap(ms);

            // Lock to prevent issues when updating the buffer
            lock (frameBuffer)
            {
                frameBuffer.Enqueue(bitmap);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void PlayPause(object? sender, RoutedEventArgs args)
    {
        Context.IsPlaying = !Context.IsPlaying;

        if (Context.IsPlaying)
        {
            _timer.Start();
        }
        else
        {
            _timer.Stop();
        }
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

        // Load initial frames into the buffer
        for (int i = 0; i < Math.Min(_bufferSize, _reader.FrameCount); i++)
        {
            using var ms = new MemoryStream(_reader.ReadFrame().ToJPEG());
            frameBuffer.Enqueue(new Bitmap(ms));
        }

        NextFrame(null, new RoutedEventArgs());

        _timer.Interval = TimeSpan.FromSeconds(1.0 / 60); // mth has framerate, but game runs at 60
    }
}