using Avalonia.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MexManager;

public partial class ProgressWindow : Window
{
    public class ProcessViewModel : INotifyPropertyChanged
    {
        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Completed));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string ProgressText
        {
            get
            {
                if (Completed)
                    return "Process Completed!";
                else
                    return "Please wait for process to complete...";
            }
        }

        public bool Completed => ProgressValue >= 100;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ProgressWindow()
    {
        InitializeComponent();
        DataContext = new ProcessViewModel();
    }

    public delegate void UpdateProgressHandler(BackgroundWorker w);
    /// <summary>
    /// 
    /// </summary>
    public static async Task<bool> DisplayProgress(UpdateProgressHandler e)
    {
        if (App.MainWindow == null)
            return false;

        ProgressWindow progressWindow = new();

        BackgroundWorker backgroundWorker = new()
        {
            WorkerReportsProgress = true,
        };

        backgroundWorker.DoWork += (s, k) =>
        {
            e.Invoke(backgroundWorker);
        };
        backgroundWorker.ProgressChanged += progressWindow.UpdateProgress;

        // Start the BackgroundWorker task
        backgroundWorker.RunWorkerAsync();

        // Create and show the progress window
        await progressWindow.ShowDialog(App.MainWindow);

        return true;
    }

    private readonly StringBuilder _logBuilder = new();

    public void AppendLog(string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        _logBuilder.AppendLine($"[{timestamp}] {message}");

        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            LogBox.Text = _logBuilder.ToString();
            LogBox.CaretIndex = LogBox.Text.Length; // Auto-scroll
        });
    }

    public void UpdateProgress(object? sender, ProgressChangedEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (e.UserState is string s)
                AppendLog(s);

            if (e.ProgressPercentage >= 0)
                ProgressBar.Value = e.ProgressPercentage;
        });
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}