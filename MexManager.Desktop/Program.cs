using System;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.ReactiveUI;
using GCILib.BZip2;
using mexLib.MexScubber;
using mexLib.Utilties;

namespace MexManager.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    //[STAThread]
    //public static void Main(string[] args)
    //{
    //    //var dol = new MexDOL(System.IO.File.ReadAllBytes(@"C:\Users\ploaj\Desktop\Modlee\MexManager\sys\main.dol"));

    //    //System.Diagnostics.Debug.WriteLine(dol.ToAddr(0x3C79D0).ToString("X8"));


    //}
}
