using ReactiveUI;
using System.Windows.Input;
using System.Collections.ObjectModel;
using mexLib.Types;
using System.Diagnostics;
using MexManager.Views;
using GCILib;
using MexManager.Tools;
using System.ComponentModel;
using DynamicData;

namespace MexManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand WorkspaceLoadedCommand { get; }
    public ICommand LaunchCommand { get; }
    public ICommand EditBannerCommand { get; }
    public ICommand ExportISOCommand { get; }


    private ObservableCollection<string> _testData = [ "Hello", "World" ];
    public ObservableCollection<string> TestData
    {
        get => _testData;
        set => this.RaiseAndSetIfChanged(ref _testData, value);
    }

    public SoundGroupModel SoundViewModel { get; } = new SoundGroupModel();

    public TrophyViewModel TrophyViewModel { get; } = new TrophyViewModel();

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

    private object? _selectedKirbyCostume;

    public object? SelectedKirbyCostume
    {
        get => _selectedKirbyCostume;
        set => this.RaiseAndSetIfChanged(ref _selectedKirbyCostume, value);
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

    private object? _reservedAssets;
    public object? ReservedAssets
    {
        get => _reservedAssets;
        set => this.RaiseAndSetIfChanged(ref _reservedAssets, value);
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
        LaunchCommand = new RelayCommand(LaunchMenuItem_Click, IsDolphinPathSet);
        ExportISOCommand = new RelayCommand(ExportISO_Click, IsWorkSpaceLoaded);

        AddStagePageCommand = new RelayCommand(AddStagePage, null);
        DeleteStagePageCommand = new RelayCommand(DeleteStagePage, IsWorkSpaceLoaded);
        MoveLeftStagePageCommand = new RelayCommand(MoveStagePageLeft, IsWorkSpaceLoaded);
        MoveRightStagePageCommand = new RelayCommand(MoveStagePageRight, IsWorkSpaceLoaded);

        EditBannerCommand = new RelayCommand(async (s) =>
        {
            if (Global.Workspace == null)
                return;

            var bannerFilePath = Global.Workspace.GetFilePath("opening.bnr");

            if (!Global.Workspace.FileManager.Exists(bannerFilePath))
                return;

            var bannerFile = Global.Workspace.FileManager.Get(bannerFilePath);

            if (bannerFile == null) return;

            GCBanner banner = new(bannerFile);
            var popup = new BannerEditor();
            popup.SetBanner(banner);

            if (App.MainWindow != null)
            {
                await popup.ShowDialog(App.MainWindow);

                var newBanner = popup.GetBanner();

                if (popup.SaveChanges && newBanner != null)
                {
                    Global.Workspace.FileManager.Set(bannerFilePath, newBanner.GetData());
                }
            }
        }, IsWorkSpaceLoaded);
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
            StagePages = null;
            StageSelect = null;
            SoundViewModel.SoundGroups = null;
            ReservedAssets = null;
            TrophyViewModel.Trophies = null;
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
            StagePages = Global.Workspace.Project.StageSelects;
            if (StagePages.Count > 0)
                StageSelect = StagePages[0];
            SoundViewModel.SoundGroups = Global.Workspace.Project.SoundGroups;
            ReservedAssets = Global.Workspace.Project.ReservedAssets;
            TrophyViewModel.Trophies = Global.Workspace.Project.Trophies;
            if (TrophyViewModel.Trophies.Count > 0)
                TrophyViewModel.SelectedTrophy = TrophyViewModel.Trophies[0];
        }
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
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsDolphinPathSet(object? parameter)
    {
        return Global.Workspace != null && System.IO.File.Exists(App.Settings.DolphinPath);
    }
    /// <summary>
    /// 
    /// </summary>
    public static void LaunchMenuItem_Click(object? parameter)
    {
        Global.LaunchGameInDolphin();
    }
    /// <summary>
    /// 
    /// </summary>
    public static async void ExportISO_Click(object? parameter)
    {
        if (Global.Workspace == null ||
            App.MainWindow == null)
            return;

        var res = await MessageBox.Show("Save Changes before exporting?", "Save Changes", MessageBox.MessageBoxButtons.YesNoCancel);

        var file = await FileIO.TrySaveFile("Export ISO", "game.iso", FileIO.FilterISO);
        if (file == null)
            return;

        ProgressWindow progressWindow = new();

        BackgroundWorker backgroundWorker = new ()
        {
            WorkerReportsProgress = true,
        };

        backgroundWorker.DoWork += (s, e) =>
        {
            if (res == MessageBox.MessageBoxResult.Yes)
            {
                progressWindow.UpdateProgress(null, new ProgressChangedEventArgs(0, "Saving workspace..."));
                Global.SaveWorkspace();
            }
            else
            {
                progressWindow.UpdateProgress(null, new ProgressChangedEventArgs(0, "Begin building..."));
            }

            Global.Workspace.ExportISO(file, (r, t) =>
            {
                backgroundWorker.ReportProgress(t.ProgressPercentage, t.UserState);
            });
        };
        backgroundWorker.ProgressChanged += (s, e) =>
        {
            progressWindow.UpdateProgress(s, e);
        };

        // Start the BackgroundWorker task
        backgroundWorker.RunWorkerAsync();

        // Create and show the progress window
        await progressWindow.ShowDialog(App.MainWindow);

    }
}
