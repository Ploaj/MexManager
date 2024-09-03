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

    private object? _selectedFighter;

    public object? SelectedFighter
    {
        get => _selectedFighter;
        set => this.RaiseAndSetIfChanged(ref _selectedFighter, value);
    }

    private object? _selectedFighterItem;

    public object? SelectedFighterItem
    {
        get => _selectedFighterItem;
        set => this.RaiseAndSetIfChanged(ref _selectedFighterItem, value);
    }

    private object? _selectedMusic;
    public object? SelectedMusic
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

    private MexPlaylist? _menuPlaylist;
    public MexPlaylist? MenuPlaylist
    {
        get => _menuPlaylist;
        set
        {
            this.RaiseAndSetIfChanged(ref _menuPlaylist, value);
        }
    }

    private int _test;
    public int Test
    {
        get => _test;
        set
        {
            this.RaiseAndSetIfChanged(ref _test, value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public MainViewModel()
    {
        SaveCommand = new RelayCommand(SaveMenuItem_Click, SaveMenuItem_CanExecute);
        CloseCommand = new RelayCommand(CloseMenuItem_Click, SaveMenuItem_CanExecute);
        WorkspaceLoadedCommand = new RelayCommand((e) => { }, SaveMenuItem_CanExecute);
    }
    /// <summary>
    /// 
    /// </summary>
    public void UpdateWorkspace()
    {
        Fighters = Global.Workspace?.Project.Fighters;
        Music = Global.Workspace?.Project.Music;
        MenuPlaylist = Global.Workspace?.Project.MenuPlaylist;
        Test = 5;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="meleePath"></param>
    public void CreateNewWorkspace(string path)
    {
        var workspace = Global.CreateWorkspace(path);

        if (workspace == null)
        {
            MessageBox.Show("Unable to create workspace", "Create Workspace", MessageBox.MessageBoxButtons.Ok);
        }

        UpdateWorkspace();
    }
    /// <summary>
    /// 
    /// </summary>
    public void OpenWorkspace(string path)
    {
        Global.LoadWorkspace(path);
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
