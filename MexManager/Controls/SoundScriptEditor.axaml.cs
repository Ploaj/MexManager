using Avalonia.Controls;
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