using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HSDRaw;
using HSDRaw.Common;
using HSDRaw.Tools.TriangleConverter;
using mexLib.Utilties;
using MexManager.ViewModels;
using MexManager.Views;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace MexManager;

public partial class App : Application
{
    public static ApplicationSettings Settings { get; private set; } = new ApplicationSettings();

    public static string AppPath { get; private set; } = AppDomain.CurrentDomain.BaseDirectory;

    public static TopLevel? TopLevel { get; private set; }

    public static Window? MainWindow { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Settings = ApplicationSettings.TryOpen();

        Global.Initialize();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            MainWindow = desktop.MainWindow;
            TopLevel = TopLevel.GetTopLevel(desktop.MainWindow);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
            TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
