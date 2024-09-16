using Avalonia;
using Avalonia.Controls;
using System.ComponentModel;

namespace MexManager;

public partial class ProgressWindow : Window
{
    public ProgressWindow()
    {
        InitializeComponent();
    }

    public void UpdateProgress(object sender, ProgressChangedEventArgs e)
    {
        // Update the ProgressBar value
        ProgressBar.Value = e.ProgressPercentage;
    }
}