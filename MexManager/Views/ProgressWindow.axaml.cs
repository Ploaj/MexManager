using Avalonia.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

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
                    return "ISO Generated!";
                else
                    return "Creating ISO Please Wait...";
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

    private readonly StringBuilder _logBuilder = new ();

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

            ProgressBar.Value = e.ProgressPercentage;
        });
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}