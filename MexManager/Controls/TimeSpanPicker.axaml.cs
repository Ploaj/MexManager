using Avalonia;
using Avalonia.Controls;
using System;

namespace MexManager.Controls;

public partial class TimeSpanPicker : UserControl
{
    public static readonly StyledProperty<TimeSpan> TimeSpanProperty =
            AvaloniaProperty.Register<TimeSpanPicker, TimeSpan>(nameof(TimeSpan));

    public delegate void TimeSpanChange();
    public TimeSpanChange? OnTimeSpanChange;

    public TimeSpan TimeSpan
    {
        get => GetValue(TimeSpanProperty);
        set
        {
            SetValue(TimeSpanProperty, value);
            UpdateControls(value);
        }
    }

    private bool SkipUpdate = false;
    public void SetWithNoUpdate(TimeSpan ts)
    {
        SkipUpdate = true;
        TimeSpan = ts;
        SkipUpdate = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public TimeSpanPicker()
    {
        InitializeComponent();

        // Handle the ValueChanged event for each NumericUpDown
        HoursUpDown.PropertyChanged += TimeComponentChanged;
        MinutesUpDown.PropertyChanged += TimeComponentChanged;
        SecondsUpDown.PropertyChanged += TimeComponentChanged;
        MilliSecondsUpDown.PropertyChanged += TimeComponentChanged;

        // Initialize controls with the current TimeSpan value
        UpdateControls(TimeSpan);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimeComponentChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (SkipUpdate)
            return;

        if (e.Property == NumericUpDown.ValueProperty &&
            HoursUpDown.Value != null &&
            MinutesUpDown.Value != null &&
            SecondsUpDown.Value != null &&
            MilliSecondsUpDown.Value != null)
        {
            TimeSpan = new TimeSpan(
                0,
                (int)HoursUpDown.Value,
                (int)MinutesUpDown.Value,
                (int)SecondsUpDown.Value,
                (int)MilliSecondsUpDown.Value);

            // Update the controls with the latest value to ensure consistency
            UpdateControls(TimeSpan);

            OnTimeSpanChange?.Invoke();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeSpan"></param>
    public void UpdateControls(TimeSpan timeSpan)
    {
        HoursUpDown.Value = timeSpan.Hours;
        MinutesUpDown.Value = timeSpan.Minutes;
        SecondsUpDown.Value = timeSpan.Seconds;
        MilliSecondsUpDown.Value = timeSpan.Milliseconds;
    }
}