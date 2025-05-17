using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;

namespace MexManager.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Set the working directory to the directory of the executable
            string appDirectory = AppContext.BaseDirectory;
            Environment.CurrentDirectory = appDirectory;

            // set launch args
            Global.LaunchArgs = args;

            // build app
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            File.WriteAllText("crash.log", e.ToString());
        }

    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
