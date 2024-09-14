using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MeleeMedia.Audio;

namespace MexManager.Controls;

public partial class SoundScriptEditor : UserControl
{
    public SEMBankScript? Script { get => DataContext as SEMBankScript; }

    /// <summary>
    /// 
    /// </summary>
    public SoundScriptEditor()
    {
        InitializeComponent();
    }
}