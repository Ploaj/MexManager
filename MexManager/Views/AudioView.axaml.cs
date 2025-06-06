using Avalonia;
using Avalonia.Controls;
using MeleeMedia.Audio;
using MexManager.ViewModels;

namespace MexManager.Views;

public partial class AudioView : UserControl
{
    /// <summary>
    /// 
    /// </summary>
    public AudioView()
    {
        InitializeComponent();
        DataContext = new AudioPlayerModel();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hps"></param>
    public void LoadDSP(DSP dsp)
    {
        if (DataContext is AudioPlayerModel model)
        {
            model.LoadDSP(dsp);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hps"></param>
    public void LoadHPS(byte[] hps)
    {
        DSP dsp = HPS.ToDSP(hps);
        dsp.LoopSound = true;
        LoadDSP(dsp);
    }
    /// <summary>
    /// 
    /// </summary>
    public void Play()
    {
        if (DataContext is AudioPlayerModel model)
        {
            model.PlaySound();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public void Stop()
    {
        if (DataContext is AudioPlayerModel model)
        {
            model.StopSound();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Slider_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (DataContext is AudioPlayerModel model &&
            e.NewValue is double d)
        {
            model.SeekPercentage(d / PlaybackSlider.Maximum);
        }
    }
}