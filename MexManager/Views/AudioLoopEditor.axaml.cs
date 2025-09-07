using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MeleeMedia.Audio;
using MexManager.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;

namespace MexManager.Views;

public partial class AudioLoopEditor : Window
{
    public enum AudioEditorResult
    {
        Canceled,
        SaveChanges,
    }

    public AudioEditorResult Result { get; internal set; } = AudioEditorResult.Canceled;

    private Line? endLine;
    private Line? loopLine;
    private Line? isDragging = null;

    private Rectangle? playbackOverlay;

    private readonly AudioPlayer player;

    public DSP? DSP { get; internal set; }

    private double currentPercentage;

    private double _endPercentage;
    private double EndPercentage
    {
        get => _endPercentage;
        set
        {
            if (value < LoopPercentage)
                value = LoopPercentage;

            _endPercentage = value;
            player.EndPercentage = value;
            double pos = value * WaveformCanvas.Bounds.Width;
            if (endLine != null && DSP != null)
            {
                endLine.StartPoint = new Point(pos, 0);
                endLine.EndPoint = new Point(pos, WaveformCanvas.Bounds.Height);

                EndTimeSpanPicker.SetWithNoUpdate(TimeSpan.FromMilliseconds(value * DSP.TotalMilliseconds));
            }
        }
    }

    private double LoopPercentage
    {
        get => DSP == null ? 0 : DSP.LoopPointMilliseconds / DSP.TotalMilliseconds;
        set
        {
            if (value > EndPercentage)
                value = EndPercentage;

            if (DSP != null &&
                loopLine != null &&
                LoopTimeSpanPicker != null)
            {
                // set dsp loop point
                DSP.LoopPointMilliseconds = value * DSP.TotalMilliseconds;
                player.LoopPointPercent = value;

                // update line
                double pos = value * WaveformCanvas.Bounds.Width;
                loopLine.StartPoint = new Point(pos, 0);
                loopLine.EndPoint = new Point(pos, WaveformCanvas.Bounds.Height);

                // update picker
                LoopTimeSpanPicker.SetWithNoUpdate(TimeSpan.FromMilliseconds(DSP.LoopPointMilliseconds));
            }
        }
    }

    public const int LINE_COUNT = 300;
    private readonly List<Line> leftLines = [];
    private readonly List<Line> rightLines = [];
    private WAVE? WavCache = null;
    private Size _lastSize;

    private Timer? positionUpdateTimer; // Timer for updating playback position

    public AudioLoopEditor()
    {
        InitializeComponent();

        InitCanvas();

        WaveformCanvas.LayoutUpdated += (s, e) =>
        {
            Size currentSize = WaveformCanvas.Bounds.Size;
            if (_lastSize != currentSize)
            {
                _lastSize = currentSize;
                OnRedraw(s, e);
            }
        };

        player = new AudioPlayer();

        LoopTimeSpanPicker.OnTimeSpanChange += () =>
        {
            if (DSP != null)
                LoopPercentage = LoopTimeSpanPicker.TimeSpan.TotalMilliseconds / DSP.TotalMilliseconds;
        };

        EndTimeSpanPicker.OnTimeSpanChange += () =>
        {
            if (DSP != null)
                EndPercentage = EndTimeSpanPicker.TimeSpan.TotalMilliseconds / DSP.TotalMilliseconds;
        };

        DSP = null;

        Closing += (s, e) =>
        {
            Resized -= OnRedraw;
            positionUpdateTimer?.Stop();
            positionUpdateTimer?.Dispose();
            player.Dispose();
        };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public DSP? ApplyChanges()
    {
        if (DSP != null)
        {
            foreach (DSPChannel? c in DSP.Channels)
            {
                int newSize = (int)(EndPercentage * c.Data.Length);// * dsp.Channels[0].LoopStart / 2f * 1.75f;

                if (newSize != c.Data.Length)
                {
                    byte[] newData = new byte[newSize];
                    Array.Copy(c.Data, newData, newSize);
                    c.Data = newData;
                }
            }

            return DSP;
        }
        return null;
    }
    /// <summary>
    /// 
    /// </summary>
    private void InitializeTimer()
    {
        if (positionUpdateTimer != null)
            return;

        positionUpdateTimer = new Timer(30);
        positionUpdateTimer.Elapsed += async (sender, e) =>
        {
            // Update the current position based on the sound player's playback position
            currentPercentage = player.Percentage;

            // Update the UI on the main thread
            if (Dispatcher.UIThread != null)
                await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        UpdateOverlay();
                    });
        };
        positionUpdateTimer.Start();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void PlayButton_Click(object? sender, RoutedEventArgs args)
    {
        player.Play();

        if (player.State == OpenTK.Audio.OpenAL.ALSourceState.Playing)
        {
            PlayPauseImage.Source = BitmapManager.PauseIconImage;
        }
        else
        {
            PlayPauseImage.Source = BitmapManager.PlayIconImage;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dsp"></param>
    public void SetAudio(DSP dsp)
    {
        DSP = dsp;
        WavCache = dsp.ToWAVE();
        player.LoadDSP(dsp);

        //
        EnableLoopCheckBox.IsChecked = dsp.LoopSound;

        // initialize dsp
        EndPercentage = 1;
        LoopPercentage = (DSP.LoopPointMilliseconds / DSP.TotalMilliseconds);

        // redraw
        OnRedraw(null, EventArgs.Empty);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnRedraw(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Redraw " + WaveformCanvas.Bounds.Width);

        EndPercentage = _endPercentage;
        LoopPercentage = DSP == null ? 0 : DSP.LoopPointMilliseconds / DSP.TotalMilliseconds;
        UpdateOverlay();
        DrawWaveform();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void InitCanvas()
    {
        WaveformCanvas.Children.Clear();

        InitWaveForm(WaveformCanvas);

        // initial scrubbers
        InitializeScrubberLine(WaveformCanvas);

        // intialize timer
        InitializeTimer();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="canvas"></param>
    private void InitializeScrubberLine(Canvas canvas)
    {
        playbackOverlay = new Rectangle()
        {
            Fill = new SolidColorBrush(Color.FromArgb(64, 0, 0, 255)), // Semi-transparent blue
            [Canvas.TopProperty] = 0,
            [Canvas.LeftProperty] = 0
        };
        canvas.Children.Add(playbackOverlay);

        // loop line
        loopLine = new Line
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, canvas.Bounds.Height),
            Stroke = Brushes.White,
            StrokeThickness = 2
        };
        canvas.Children.Add(loopLine);

        // end line
        endLine = new Line
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, canvas.Bounds.Height),
            Stroke = Brushes.Gray,
            StrokeThickness = 2
        };
        canvas.Children.Add(endLine);

        // Handle mouse events
        canvas.PointerPressed += Canvas_PointerPressed;
        canvas.PointerMoved += Canvas_PointerMoved;
        canvas.PointerReleased += Canvas_PointerReleased;
        canvas.PointerExited += (s, e) =>
        {
            loopLine.StrokeThickness = 2;
            endLine.StrokeThickness = 2;
            pointerDown = false;
        };
    }
    /// <summary>
    /// 
    /// </summary>
    private void UpdateOverlay()
    {
        if (playbackOverlay == null)
            return;

        double canvasWidth = WaveformCanvas.Bounds.Width;

        // Calculate the width of the overlay based on the current position and total duration
        double positionPercentage = currentPercentage;
        double overlayWidth = canvasWidth * positionPercentage;

        playbackOverlay.Width = overlayWidth;
        playbackOverlay.Height = WaveformCanvas.Bounds.Height;
    }
    /// <summary>
    /// 
    /// </summary>
    private bool pointerDown;
    private void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (loopLine == null)
            return;

        if (endLine == null)
            return;

        pointerDown = true;

        // Check if the mouse is near the scrubber line
        Point pointerPosition = e.GetPosition(WaveformCanvas);

        bool endLineHoverd = Math.Abs(pointerPosition.X - endLine.StartPoint.X) < 8;
        bool loopLineHoverd = Math.Abs(pointerPosition.X - loopLine.StartPoint.X) < 8;

        if (endLineHoverd)
        {
            if (loopLineHoverd && EndPercentage >= 0.95)
            {
                isDragging = loopLine;
            }
            else
            {
                isDragging = endLine;
            }
        }
        else
        if (loopLineHoverd)
        {
            isDragging = loopLine;
        }
        else
        {
            UpdateProgress(pointerPosition);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Canvas_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (loopLine == null || endLine == null)
            return;

        Point pointerPosition = e.GetPosition(WaveformCanvas);

        // Check if the mouse is near the scrubber line
        if (Math.Abs(pointerPosition.X - loopLine.StartPoint.X) < 8 && isDragging == null)
        {
            // Change the cursor to indicate the line can be dragged
            WaveformCanvas.Cursor = new Cursor(StandardCursorType.SizeWestEast);
            loopLine.StrokeThickness = 4;
        }
        else
        // Check if the mouse is near the scrubber line
        if (Math.Abs(pointerPosition.X - endLine.StartPoint.X) < 8 && isDragging == null)
        {
            // Change the cursor to indicate the line can be dragged
            WaveformCanvas.Cursor = new Cursor(StandardCursorType.SizeWestEast);
            endLine.StrokeThickness = 4;
        }
        else
        {
            // Reset the cursor to default if not near the line
            if (isDragging == null)
            {
                UpdateProgress(pointerPosition);
                WaveformCanvas.Cursor = new Cursor(StandardCursorType.Arrow);
            }
            loopLine.StrokeThickness = 2;
            endLine.StrokeThickness = 2;
        }

        if (isDragging != null)
        {
            MoveScrubberLine(pointerPosition.X);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointerPosition"></param>
    private void UpdateProgress(Point pointerPosition)
    {
        if (pointerDown)
        {
            double xPosition = pointerPosition.X;
            if (xPosition < 0) xPosition = 0;
            if (xPosition > WaveformCanvas.Bounds.Width) xPosition = WaveformCanvas.Bounds.Width;

            double percent = xPosition / WaveformCanvas.Bounds.Width;

            if (Math.Abs(percent - player.Percentage) > 0.001)
            {
                bool playing = player.State == OpenTK.Audio.OpenAL.ALSourceState.Playing;
                player.SeekPercentage(percent);
                if (playing)
                    player.Play();
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (loopLine == null)
            return;

        pointerDown = false;
        isDragging = null;

        // Calculate the percentage of the canvas
        double scrubPercentage = loopLine.StartPoint.X / WaveformCanvas.Bounds.Width;
        Console.WriteLine($"Scrub Percentage: {scrubPercentage:P2}");

        // Use scrubPercentage to update playback or processing logic
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="xPosition"></param>
    private void MoveScrubberLine(double xPosition)
    {
        if (xPosition < 0) xPosition = 0;
        if (xPosition > WaveformCanvas.Bounds.Width) xPosition = WaveformCanvas.Bounds.Width;

        if (isDragging == loopLine)
            LoopPercentage = xPosition / WaveformCanvas.Bounds.Width;

        if (isDragging == endLine)
            EndPercentage = xPosition / WaveformCanvas.Bounds.Width;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="leftChannel"></param>
    /// <param name="rightChannel"></param>
    /// <param name="bitsPerSample"></param>
    /// <param name="sampleRate"></param>
    /// <param name="canvas"></param>
    private void InitWaveForm(Canvas canvas)
    {
        double canvasHeight = canvas.Bounds.Height;
        double midY = canvasHeight / 2.0;

        for (int x = 0; x < LINE_COUNT; x++)
        {
            // Draw the lines representing the left and right channel waveforms
            Line leftLine = new()
            {
                StartPoint = new Point(x, midY),
                EndPoint = new Point(x, midY),
                Stroke = Brushes.Blue,
                StrokeThickness = 1
            };

            Line rightLine = new()
            {
                StartPoint = new Point(x, midY),
                EndPoint = new Point(x, midY),
                Stroke = Brushes.Red,
                StrokeThickness = 1
            };

            canvas.Children.Add(leftLine);
            canvas.Children.Add(rightLine);

            leftLines.Add(leftLine);
            rightLines.Add(rightLine);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="leftChannel"></param>
    /// <param name="rightChannel"></param>
    /// <param name="bitsPerSample"></param>
    /// <param name="sampleRate"></param>
    /// <param name="canvas"></param>
    private void DrawWaveform()
    {
        if (WavCache == null)
            return;

        if (WavCache.Channels.Count <= 0)
            return;

        int bitsPerSample = WavCache.BitsPerSample;
        short[] leftChannel = WavCache.Channels[0];
        short[] rightChannel = WavCache.Channels.Count > 1 ? WavCache.Channels[1] : WavCache.Channels[0];
        double canvasWidth = WaveformCanvas.Bounds.Width;
        double canvasHeight = WaveformCanvas.Bounds.Height;

        if (canvasWidth == 0 || leftChannel.Length == 0 || leftLines.Count == 0)
            return;

        double midY = canvasHeight / 2.0;
        double scaleFactor = (midY / (Math.Pow(2, bitsPerSample - 1) - 1));

        int totalSamples = leftChannel.Length;
        int count = leftLines.Count;
        double samplesPerPixel = totalSamples / count;

        for (int x = 0; x < count; x++)
        {
            int startSampleIndex = (int)(x * samplesPerPixel);
            int endSampleIndex = Math.Min((int)((x + 1) * samplesPerPixel), totalSamples);

            float maxLeftSample = 0;
            float maxRightSample = 0;

            // Find the maximum amplitude in this sample range for both channels
            for (int i = startSampleIndex; i < endSampleIndex; i++)
            {
                maxLeftSample = Math.Max(maxLeftSample, Math.Abs((int)leftChannel[i]));
                maxRightSample = Math.Max(maxRightSample, Math.Abs((int)rightChannel[i]));
            }

            double leftY = midY - maxLeftSample * scaleFactor;
            double rightY = midY + maxRightSample * scaleFactor;

            double xpos = (x / (count - 1.0)) * canvasWidth;

            // Draw the lines representing the left and right channel waveforms
            leftLines[x].StartPoint = new Point(xpos, midY);
            leftLines[x].EndPoint = new Point(xpos, leftY);

            rightLines[x].StartPoint = new Point(xpos, midY);
            rightLines[x].EndPoint = new Point(xpos, rightY);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (EnableLoopCheckBox != null && EnableLoopCheckBox.IsChecked is bool check)
        {
            player.EnableLoop = check;

            if (loopLine != null)
                loopLine.Stroke = check ? Brushes.White : Brushes.DarkGray;

            if (DSP != null)
                foreach (DSPChannel? c in DSP.Channels)
                {
                    c.LoopFlag = (short)(check ? 1 : 0);
                }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Result = AudioEditorResult.SaveChanges;
        this.Close();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MenuItem_Click_1(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace == null)
            return;

        string? file = await FileIO.TryOpenFile("Import Music", "", FileIO.FilterMusic);

        if (file != null)
        {
            DSP hps = new();
            if (hps.FromFile(file))
            {
                SetAudio(hps);
            }
            else
            {
                await MessageBox.Show($"Failed to import file\n{file}", "Import Music Error", MessageBox.MessageBoxButtons.Ok);
            }
        }
    }
    public class GenSilence
    {
        [DisplayName("Time in Seconds")]
        [Description("1 = 1 second 0.5 = half a second")]
        public float Time { get; set; } = 0.05f;
    }
    private readonly static GenSilence SilenceSetting = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GenerateSilence_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Global.Workspace == null)
            return;

        if (DSP == null)
            return;

        bool res = await PropertyGridPopup.ShowDialog("Generate Silence", "OK", SilenceSetting);

        if (!res)
            return;

        if (SilenceSetting.Time <= 0)
            return;

        WAVE wave = DSP.ToWAVE();
        int gen = (int)(wave.Frequency * SilenceSetting.Time);

        List<short[]> newChannels = [];
        foreach (short[]? c in wave.Channels)
        {
            short[] d = new short[c.Length + gen];
            Array.Copy(c, d, c.Length);
            newChannels.Add(d);
        }
        wave.Channels.Clear();
        wave.Channels.AddRange(newChannels);

        DSP.FromWAVE(wave.ToFile());

        SetAudio(DSP);
    }
}