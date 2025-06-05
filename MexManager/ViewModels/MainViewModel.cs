using GCILib;
using mexLib.Installer;
using mexLib.Types;
using MexManager.Tools;
using MexManager.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MexManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand WorkspaceLoadedCommand { get; }
    public ICommand LaunchCommand { get; }
    public ICommand EditBannerCommand { get; }
    public ICommand ExportISOCommand { get; }
    public ICommand UpdateCommand { get; }


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

    private MexBuildInfo? _buildInfo;
    public MexBuildInfo? BuildInfo
    {
        get => _buildInfo;
        set
        {
            this.RaiseAndSetIfChanged(ref _buildInfo, value);
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
        UpdateCommand = new RelayCommand(Update_Click, UpdateReady);

        _ = Updater.CheckLatest(() =>
        {
            ((RelayCommand)UpdateCommand).RaiseCanExecuteChanged();
        });

        AddStagePageCommand = new RelayCommand(AddStagePage, null);
        DeleteStagePageCommand = new RelayCommand(DeleteStagePage, IsWorkSpaceLoaded);
        MoveLeftStagePageCommand = new RelayCommand(MoveStagePageLeft, IsWorkSpaceLoaded);
        MoveRightStagePageCommand = new RelayCommand(MoveStagePageRight, IsWorkSpaceLoaded);

        EditBannerCommand = new RelayCommand(async (s) =>
        {
            if (Global.Workspace == null)
                return;

            string bannerFilePath = Global.Workspace.GetFilePath("opening.bnr");

            if (!Global.Workspace.FileManager.Exists(bannerFilePath))
                return;

            byte[] bannerFile = Global.Workspace.FileManager.Get(bannerFilePath);

            if (bannerFile == null) return;

            GCBanner banner = new(bannerFile);
            BannerEditor popup = new();
            popup.SetBanner(banner);

            if (App.MainWindow != null)
            {
                await popup.ShowDialog(App.MainWindow);

                GCBanner? newBanner = popup.GetBanner();

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
    public async void UpdateWorkspace()
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
            BuildInfo = null;
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
            BuildInfo = Global.Workspace.Project.Build;

            // check to fix move logic pointers
            if (Global.Workspace.Project.Fighters.Count > 0 &&
                Global.Workspace.Project.Fighters[0].Functions.MoveLogicPointer == 0)
            {
                var mes = await MessageBox.Show("It looks like some fighters are missing move logic pointers.\nWould you like to fix them now?", "Fix Move Logic", MessageBox.MessageBoxButtons.YesNoCancel);
                if (mes == MessageBox.MessageBoxResult.Yes)
                    MexInstaller.CorrectFixMoveLogicPointers(Global.Workspace);
            }
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
    public async void ExportISO_Click(object? parameter)
    {
        if (Global.Workspace == null ||
            App.MainWindow == null)
            return;

        MessageBox.MessageBoxResult res = await MessageBox.Show("Save Changes before exporting?", "Save Changes", MessageBox.MessageBoxButtons.YesNoCancel);

        if (res == MessageBox.MessageBoxResult.Cancel)
            return;

        string? file = await FileIO.TrySaveFile("Export ISO", "game.iso", FileIO.FilterISO);
        if (file == null)
            return;

        // unselect fighter for so the move stream isn't loaded
        SelectedFighter = null;

        ProgressWindow progressWindow = new();

        BackgroundWorker backgroundWorker = new()
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter"></param>
    public async void Update_Click(object? parameter)
    {
        var res = await MessageBox.Show($"Would you like to update MexManager to latest?\n\n{Updater.Version}\n\n{Updater.LatestRelease?.Body}", 
            "Update MexManager",
            MessageBox.MessageBoxButtons.YesNoCancel);

        if (res != MessageBox.MessageBoxResult.Yes)
            return;

        // save changes if workspace is loaded
        if (Global.Workspace != null)
        {
            MessageBox.MessageBoxResult res2 = await MessageBox.Show("Save Changes to current project?", "Save Changes", MessageBox.MessageBoxButtons.YesNoCancel);

            switch (res2)
            {
                case MessageBox.MessageBoxResult.Yes:
                    Global.SaveWorkspace();
                    break;
                case MessageBox.MessageBoxResult.Cancel:
                    return;
            }
        }

        // perform update
        Updater.Update();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static bool UpdateReady(object? parameter)
    {
        return Updater.UpdateReady;
    }
}
