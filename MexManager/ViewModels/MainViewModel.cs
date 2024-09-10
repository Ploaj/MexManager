using ReactiveUI;
using System.Windows.Input;
using mexLib;
using Avalonia.Media.Imaging;
using Avalonia;
using System.Collections.ObjectModel;
using System;
using MexManager.Views;
using mexLib.Types;
using MexManager.Controls;

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

    private object? _selectedFighterCostume;

    public object? SelectedFighterCostume
    {
        get => _selectedFighterCostume;
        set => this.RaiseAndSetIfChanged(ref _selectedFighterCostume, value);
    }

    private object? _selectedMusic;
    public object? SelectedMusic
    {
        get => _selectedMusic;
        set => this.RaiseAndSetIfChanged(ref _selectedMusic, value);
    }

    private object? _selectedStage;
    public object? SelectedStage
    {
        get => _selectedStage;
        set => this.RaiseAndSetIfChanged(ref _selectedStage, value);
    }

    private object? _selectedStageItem;
    public object? SelectedStageItem
    {
        get => _selectedStageItem;
        set => this.RaiseAndSetIfChanged(ref _selectedStageItem, value);
    }

    private object? _selectedSeries;
    public object? SelectedSeries
    {
        get => _selectedSeries;
        set => this.RaiseAndSetIfChanged(ref _selectedSeries, value);
    }

    private ObservableCollection<MexSeries>? _series;
    public ObservableCollection<MexSeries>? Series
    {
        get => _series;
        set => this.RaiseAndSetIfChanged(ref _series, value);
    }

    private ObservableCollection<MexFighter>? _fighters;
    public ObservableCollection<MexFighter>? Fighters
    {
        get => _fighters;
        set => this.RaiseAndSetIfChanged(ref _fighters, value);
    }

    private ObservableCollection<MexStage>? _stages;
    public ObservableCollection<MexStage>? Stages
    {
        get => _stages;
        set => this.RaiseAndSetIfChanged(ref _stages, value);
    }

    private ObservableCollection<MexMusic>? _music;
    public ObservableCollection<MexMusic>? Music
    {
        get => _music;
        set => this.RaiseAndSetIfChanged(ref _music, value);
    }

    private MexCharacterSelect? _characterSelect;
    public MexCharacterSelect? CharacterSelect
    {
        get => _characterSelect;
        set => this.RaiseAndSetIfChanged(ref _characterSelect, value);
    }

    private object? _selectedCSSIcon;
    public object? SelectedCSSIcon
    {
        get => _selectedCSSIcon;
        set => this.RaiseAndSetIfChanged(ref _selectedCSSIcon, value);
    }

    private bool _autoApplyCSSTemplate = true;
    public bool AutoApplyCSSTemplate
    {
        get => _autoApplyCSSTemplate;
        set
        {
            this.RaiseAndSetIfChanged(ref _autoApplyCSSTemplate, value);
        }
    }

    private MexStageSelect? _stageSelect;
    public MexStageSelect? StageSelect
    {
        get => _stageSelect;
        set => this.RaiseAndSetIfChanged(ref _stageSelect, value);
    }

    private object? _selectedSSSIcon;
    public object? SelectedSSSIcon
    {
        get => _selectedSSSIcon;
        set => this.RaiseAndSetIfChanged(ref _selectedSSSIcon, value);
    }

    private object? _selectedCode;
    public object? SelectedCode
    {
        get => _selectedCode;
        set => this.RaiseAndSetIfChanged(ref _selectedCode, value);
    }

    private ObservableCollection<MexCode>? _codes;
    public ObservableCollection<MexCode>? Codes
    {
        get => _codes;
        set => this.RaiseAndSetIfChanged(ref _codes, value);
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

    /// <summary>
    /// 
    /// </summary>
    public MainViewModel()
    {
        SaveCommand = new RelayCommand(SaveMenuItem_Click, IsWorkSpaceLoaded);
        CloseCommand = new RelayCommand(CloseMenuItem_Click, IsWorkSpaceLoaded);
        WorkspaceLoadedCommand = new RelayCommand((e) => { }, IsWorkSpaceLoaded);
    }
    /// <summary>
    /// 
    /// </summary>
    public void UpdateWorkspace()
    {
        if (Global.Workspace == null)
        {
            Fighters = null;
            Stages = null;
            Music = null;
            MenuPlaylist = null;
            Series = null;
            Codes = null;
            CharacterSelect = null;
            StageSelect = null;
        }
        else
        {
            Fighters = Global.Workspace.Project.Fighters;
            Stages = Global.Workspace.Project.Stages;
            Music = Global.Workspace.Project.Music;
            MenuPlaylist = Global.Workspace.Project.MenuPlaylist;
            Series = Global.Workspace.Project.Series;
            Codes = Global.Workspace.Project.Codes;
            CharacterSelect = Global.Workspace.Project.CharacterSelect;
            StageSelect = Global.Workspace.Project.StageSelects[0];
        }
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
    public static bool IsWorkSpaceLoaded(object? parameter)
    {
        return Global.Workspace != null;
    }
}
