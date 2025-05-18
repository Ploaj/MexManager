using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MeleeMedia.Video;
using mexLib;
using MexManager.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MexManager.Views;
public class VideoPlayerContext : MexReactiveObject
{
    public bool IsVideoLoaded { get => _isVideoLoaded; set { _isVideoLoaded = value; OnPropertyChanged(); } }
    private bool _isVideoLoaded;

    public bool IsPlaying { get => _isPlaying; set { _isPlaying = value; OnPropertyChanged(); } }
    private bool _isPlaying;

    public int Progress { get => _progress; set { _progress = value; OnPropertyChanged(); } }
    private int _progress;
}

public partial class MTHEditor : UserControl
{
    private MTHReader? _reader;

    private readonly Queue<Bitmap> frameBuffer = new();

    private readonly DispatcherTimer _timer;

    private int _frameIndex = 0;

    private readonly int _bufferSize = 10;
    private readonly int _preloadThreshold = 4;

    private readonly VideoPlayerContext Context;

    private string? _filePath;

    //private readonly Bitmap? _previewBitmap;

    /// <summary>
    /// 
    /// </summary>
    public MTHEditor()
    {
        InitializeComponent();

        Context = new VideoPlayerContext();
        DataContext = Context;

        _timer = new DispatcherTimer();
        _timer.Tick += Timer_Tick;

        DataContextChanged += (s, a) =>
        {
            Update();

            if (DataContext == null)
            {
                SetVideo("");
            }
        };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        NextFrame(null, new RoutedEventArgs());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_reader is not null)
        {
            _reader?.Dispose();
            _reader = null;
        }

        // Clean up the timer
        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Update()
    {
        if (Global.Workspace == null)
            return;

        if (DataContext is not string text)
            return;

        string path = Global.Workspace.GetFilePath(text);

        if (!Global.Workspace.FileManager.Exists(path))
        {
            VideoPanel.Source = null;
            return;
        }

        SetVideo(path);
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
            Bitmap? oldFrame = VideoPanel.Source as Bitmap;
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
            using MemoryStream ms = new(_reader.ReadFrame().ToJPEG());
            Bitmap bitmap = new(ms);

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
    /// <param name="frame"></param>
    private void Seek(int frame)
    {
        if (_reader == null)
            return;

        // clear current frame buffer
        while (frameBuffer.Count > 0)
            frameBuffer.Dequeue().Dispose();

        // Load initial frames into the buffer
        _reader.Seek(frame);
        for (int i = 0; i < Math.Min(_bufferSize, _reader.FrameCount); i++)
        {
            using MemoryStream ms = new(_reader.ReadFrame().ToJPEG());
            frameBuffer.Enqueue(new Bitmap(ms));
        }

        NextFrame(null, new RoutedEventArgs());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    public void SetVideo(string filePath)
    {
        if (Global.Workspace == null)
            return;

        _timer.Stop();

        _reader?.Dispose();

        if (string.IsNullOrEmpty(filePath) ||
            !File.Exists(filePath))
        {
            _reader = null;
            frameBuffer.Clear();
            Context.IsVideoLoaded = false;
            _filePath = filePath;
            return;
        }

        Stream? stream = Global.Workspace.FileManager.GetStream(filePath);
        _reader = new MTHReader(stream);

        // clear frame buffers
        foreach (Bitmap v in frameBuffer)
            v.Dispose();
        frameBuffer.Clear();

        // Load initial frames into the buffer
        for (int i = 0; i < Math.Min(_bufferSize, _reader.FrameCount); i++)
        {
            using MemoryStream ms = new(_reader.ReadFrame().ToJPEG());
            frameBuffer.Enqueue(new Bitmap(ms));
        }

        _frameIndex = 0;
        NextFrame(null, new RoutedEventArgs());

        _timer.Interval = TimeSpan.FromSeconds(1.0 / 60); // mth has framerate, but game runs at 60

        Context.IsVideoLoaded = true;
        _filePath = filePath;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void PreviousFrame(object? sender, RoutedEventArgs args)
    {
        if (_reader == null)
            return;

        if (_frameIndex == 0)
            _frameIndex = _reader.FrameCount;

        _frameIndex -= 2;

        if (_frameIndex < 0)
            _frameIndex = _reader.FrameCount - 1;

        Seek(_frameIndex);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void ExportFrames(object? sender, RoutedEventArgs args)
    {
        if (_reader == null)
            return;

        string? file = await FileIO.TrySaveFile("", "", FileIO.FilterJpeg);

        if (file != null)
        {
            string? path = Path.GetDirectoryName(file);
            string fileName = Path.GetFileName(file);

            if (path != null && fileName != null)
            {
                _reader.Seek(0);
                for (int i = 0; i < _reader.FrameCount; i++)
                {
                    THP frame = _reader.ReadFrame();
                    File.WriteAllBytes(Path.Combine(path, $"fileName_{i:D3}.jpg"), frame.ToJPEG());
                }
                Seek(0);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void ImportFrames(object? sender, RoutedEventArgs args)
    {
        if (_reader == null ||
            _filePath == null ||
            Global.Workspace == null)
            return;

        string? folder = await FileIO.TryOpenFolder("Import Frames From Folder");

        if (folder == null)
            return;

        List<string> toImport = [];
        foreach (string f in Directory.GetFiles(folder))
        {
            string ext = Path.GetExtension(f);

            if (ext.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
                ext.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase))
            {
                toImport.Add(f);
            }
        }

        if (toImport.Count == 0)
        {
            await MessageBox.Show("No frames found to import", "Import Video Frames", MessageBox.MessageBoxButtons.Ok);
            return;
        }

        MemoryStream fstream = new();
        MTHWriter mth = new(fstream, _reader.Width, _reader.Height, _reader.FrameRate);

        _timer.Stop();
        _reader?.Dispose();
        _reader = null;

        foreach (string f in toImport)
            mth.WriteFrame(THP.FromJPEG(File.ReadAllBytes(f)));
        mth.Dispose();

        byte[] file = fstream.ToArray();

        frameBuffer.Clear(); // clear old frames
        Global.Workspace.FileManager.Set(_filePath, file); // set new file
        SetVideo(_filePath); // load new video
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void ExportCurrentFrame(object? sender, RoutedEventArgs args)
    {
        if (_reader == null)
            return;

        int _currentFrame = _frameIndex;

        if (_currentFrame == 0)
            _currentFrame = _reader.FrameCount;

        _currentFrame -= 1;

        if (_currentFrame < 0)
            _currentFrame = _reader.FrameCount - 1;

        _reader.Seek(_currentFrame);

        string? file = await FileIO.TrySaveFile("", $"frame{_currentFrame:D3}.jpg", FileIO.FilterJpeg);

        if (file != null)
        {
            THP frame = _reader.ReadFrame();
            File.WriteAllBytes(file, frame.ToJPEG());
        }
    }
}