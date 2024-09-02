using ReactiveUI;
using System.Windows.Input;
using mexLib;
using Avalonia.Media.Imaging;
using Avalonia;
using System.Collections.ObjectModel;
using System;
using MexManager.Views;

namespace MexManager.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand WorkspaceLoadedCommand { get; }

    private WriteableBitmap _bannerImage;
    public WriteableBitmap BannerImage
    {
        get => _bannerImage;
        set => this.RaisePropertyChanged();
    }

    private object _selectedFighter;

    public object SelectedFighter
    {
        get => _selectedFighter;
        set => this.RaiseAndSetIfChanged(ref _selectedFighter, value);
    }

    private object _selectedFighterItem;

    public object SelectedFighterItem
    {
        get => _selectedFighterItem;
        set => this.RaiseAndSetIfChanged(ref _selectedFighterItem, value);
    }

    private object _selectedMusic;
    public object SelectedMusic
    {
        get => _selectedMusic;
        set => this.RaiseAndSetIfChanged(ref _selectedMusic, value);
    }


    private ObservableCollection<MexFighter>? _fighters;
    public ObservableCollection<MexFighter>? Fighters
    {
        get => _fighters;
        set => this.RaiseAndSetIfChanged(ref _fighters, value);
    }

    private ObservableCollection<MexMusic>? _music;
    public ObservableCollection<MexMusic>? Music
    {
        get => _music;
        set => this.RaiseAndSetIfChanged(ref _music, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public MainViewModel()
    {
        SaveCommand = new RelayCommand(SaveMenuItem_Click, SaveMenuItem_CanExecute);
        CloseCommand = new RelayCommand(CloseMenuItem_Click, SaveMenuItem_CanExecute);
        WorkspaceLoadedCommand = new RelayCommand((e) => { }, SaveMenuItem_CanExecute);

        _bannerImage = new WriteableBitmap(new PixelSize(96, 32), new Vector(96, 96));
    }
    /// <summary>
    /// 
    /// </summary>
    public void UpdateWorkspace()
    {
        Fighters = Global.Workspace?.Project.Fighters;
        Music = Global.Workspace?.Project.Music;
        
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="meleePath"></param>
    public void CreateNewWorkspace(string path)
    {
        var workspace = Global.CreateWorkspace(path);

        if (workspace != null)
        {
            _bannerImage.FromRGB(workspace.GetBannerRGBA());
            BannerImage = _bannerImage;
        }
        else
        {
            MessageBox.Show(App.MainWindow, "Unable to create workspace", "Create Workspace", MessageBox.MessageBoxButtons.Ok);
        }

        UpdateWorkspace();
    }
    /// <summary>
    /// 
    /// </summary>
    public void OpenWorkspace(string path)
    {
        Global.LoadWorkspace(path);

        if (Global.Workspace != null)
        {
            _bannerImage.FromRGB(Global.Workspace.GetBannerRGBA());
        }

        UpdateWorkspace();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter"></param>
    public void CloseMenuItem_Click(object? parameter)
    {
        Global.CloseWorkspace();
        UpdateWorkspace();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter"></param>
    public static void SaveMenuItem_Click(object? parameter)
    {
        Global.SaveWorkspace();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static bool SaveMenuItem_CanExecute(object? parameter)
    {
        return Global.Workspace != null;
    }
}
