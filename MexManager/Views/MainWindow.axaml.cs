using Avalonia.Controls;

namespace MexManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.WindowState = WindowState.Maximized;

        Closed += (s, e) => Logger.Shutdown();
    }
}
