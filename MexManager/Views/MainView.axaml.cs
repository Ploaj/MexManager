using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using MeleeMedia.Audio;
using mexLib.Types;
using MexManager.Extensions;
using MexManager.Tools;
using MexManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace MexManager.Views;

public class MusicIndexConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return null;

        if (values[0] is not MexMusic music)
            return null;

        if (values[1] is not ObservableCollection<MexMusic> list)
            return null;

        return $"{list.IndexOf(music):D3}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
public partial class MainView : UserControl
{
    private MainViewModel? Context => this.DataContext as MainViewModel;

    public static AudioView? GlobalAudio { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        ExitMenuItem.Click += (sender, e) =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        };

        GlobalAudio = GlobalAudioView;

        DataContextChanged += (s, e) =>
        {
            SoundGroup.DataContext = Context?.SoundViewModel;

            if (Global.LaunchArgs.Length > 0)
            {
                string filePath = Global.LaunchArgs[0];
                if (Path.GetExtension(filePath) == ".mexproj")
                    Context?.OpenWorkspace(filePath);
                Global.LaunchArgs = [];
            }
        };
    }
    /// <summary>
    /// 
    /// </summary>
    private static async void OpenEditConfig()
    {
        await ConfigWindow.ShowDialog();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnNewClick(object sender, RoutedEventArgs e)
    {
        if (Context == null)
            return;

        // check if workspace currently open
        if (Global.Workspace != null)
        {
            MessageBox.MessageBoxResult rst = await MessageBox.Show(
                "Save changes to current workspace?",
                "New Workspace",
                MessageBox.MessageBoxButtons.YesNoCancel);

            if (rst == MessageBox.MessageBoxResult.Yes)
            {
                Global.SaveWorkspace();
            }
            else if (rst == MessageBox.MessageBoxResult.Cancel)
            {
                return;
            }
        }

        // validate melee iso path
        if (string.IsNullOrEmpty(App.Settings.MeleePath) ||
            !File.Exists(App.Settings.MeleePath))
        {
            MessageBox.MessageBoxResult rst = await MessageBox.Show(
                "Please set a \"Melee ISO Path\" in Config",
                "New Workspace Error",
                MessageBox.MessageBoxButtons.Ok);

            if (rst == MessageBox.MessageBoxResult.Ok)
            {
                OpenEditConfig();
            }

            return;
        }

        // Start async operation to open the dialog.
        string? file = await FileIO.TrySaveFile("Save Workspace", "project.mexproj", FileIO.FilterMexProject);

        // check if file was found
        if (file == null)
            return;

        // create new workspace
        mexLib.MexWorkspace? workspace = Global.CreateWorkspace(file);

        if (workspace == null)
            await MessageBox.Show("Unable to create workspace", "Create Workspace", MessageBox.MessageBoxButtons.Ok);

        Context.UpdateWorkspace();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CreateFromMexFileSystem_Click(object? sender, RoutedEventArgs e)
    {
        if (Context == null)
            return;

        await MessageBox.Show("Note: if you have access to the project's \".mexproj\" file,\nplease use that instead of this importer.", "m-ex Import", MessageBox.MessageBoxButtons.Ok);

        // check if workspace currently open
        if (Global.Workspace != null)
        {
            MessageBox.MessageBoxResult rst = await MessageBox.Show(
                "Save changes to current workspace?",
                "New Workspace",
                MessageBox.MessageBoxButtons.YesNoCancel);

            if (rst == MessageBox.MessageBoxResult.Yes)
            {
                Global.SaveWorkspace();
            }
            else if (rst == MessageBox.MessageBoxResult.Cancel)
            {
                return;
            }
        }

        // get dol source
        string? mexdol = await FileIO.TryOpenFile("Create from m-ex File System", "main.dol",
        [
            new("m-ex Project")
            {
                Patterns = ["*.dol"],
            },
        ]);

        // check if file was found
        if (mexdol == null)
            return;

        //// Start async operation to open the dialog.
        //var file = await FileIO.TrySaveFile("New Workspace", "project.mexproj", FileIO.FilterMexProject);

        //// check if file was found
        //if (file == null)
        //    return;

        // create new workspace
        mexLib.MexWorkspace? workspace = Global.CreateWorkspaceFromMex(mexdol);

        if (workspace == null)
            await MessageBox.Show("Unable to create workspace", "Create Workspace", MessageBox.MessageBoxButtons.Ok);

        Context.UpdateWorkspace();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnOpenClick(object sender, RoutedEventArgs e)
    {
        if (Context == null)
            return;

        string? file = await FileIO.TryOpenFile("Open Workspace", "", FileIO.FilterMexProject);

        if (file != null)
        {
            Context.OpenWorkspace(file);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnConfigClick(object sender, RoutedEventArgs e)
    {
        OpenEditConfig();
    }
    /// <summary>
    /// 
    /// </summary>
    public void PlaySelectedMusic()
    {
        if (Global.Workspace != null &&
            MusicList.SelectedItem is MexMusic music)
        {
            string hps = Global.Workspace.GetFilePath($"audio/{music.FileName}");

            if (Global.Files.Exists(hps))
            {
                GlobalAudioView.LoadHPS(Global.Files.Get(hps));
                GlobalAudioView.Play();
            }
            else
            {
                MessageBox.Show($"Could not find \"{music.FileName}\"", "File not found", MessageBox.MessageBoxButtons.Ok);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void MusicPlayButton_Click(object? sender, RoutedEventArgs args)
    {
        PlaySelectedMusic();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void MusicImportButton_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace == null)
            return;

        string? file = await FileIO.TryOpenFile("Import Music", "", FileIO.FilterMusic);

        if (file != null)
        {
            DSP hps = new();
            if (hps.FromFile(file))
            {
                string fileName = Path.GetFileNameWithoutExtension(file) + ".hps";
                string? path = Global.Workspace?.GetFilePath("audio/" + fileName);

                if (Global.Files.Exists(path))
                {
                    MessageBox.MessageBoxResult res = await MessageBox.Show($"\"{fileName}\" already exists\nWould you like to overwrite it?", "Import Music Error", MessageBox.MessageBoxButtons.YesNoCancel);

                    if (res != MessageBox.MessageBoxResult.Yes)
                        return;
                }

                if (path != null)
                {
                    using (MemoryStream s = new())
                    {
                        HPS.WriteDSPAsHPS(hps, s);
                        Global.Files.Set(path, s.ToArray());
                    }
                    Global.Workspace?.Project.Music.Add(new MexMusic()
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        FileName = fileName,
                    });
                }
            }
            else
            {
                await MessageBox.Show($"Failed to import file\n{file}", "Import Music Error", MessageBox.MessageBoxButtons.Ok);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void MusicExportButton_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace == null)
            return;

        if (MusicList.SelectedItem is MexMusic music)
        {
            string? path = Global.Workspace?.GetFilePath("audio/" + music.FileName);

            if (!Global.Files.Exists(path))
            {
                await MessageBox.Show($"Could not find music file\n{music.FileName}", "Export Music Error", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            string? file = await FileIO.TrySaveFile("Export Music", "", FileIO.FilterMusicExport);

            if (path != null && file != null)
            {
                switch (Path.GetExtension(file).ToLower())
                {
                    case ".hps":
                        File.WriteAllBytes(file, Global.Files.Get(path));
                        break;
                    case ".wav":
                        DSP dsp = HPS.ToDSP(Global.Files.Get(path));
                        File.WriteAllBytes(file, dsp.ToWAVE().ToFile());
                        break;
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void MusicDeleteButton_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace == null)
            return;

        if (MusicList.SelectedItem is MexMusic music)
        {
            // check if mex music
            if (Global.Workspace.Project.Music.IndexOf(music) <= 97)
            {
                await MessageBox.Show("Unable to remove vanilla music tracks", "Remove Music", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            // check if sure
            MessageBox.MessageBoxResult sure = await MessageBox.Show($"Are you sure you want to remove\n{music.Name}", "Remove Music", MessageBox.MessageBoxButtons.YesNoCancel);
            if (sure != MessageBox.MessageBoxResult.Yes)
                return;

            // try to remove music
            if (!Global.Workspace.Project.RemoveMusic(music))
            {
                await MessageBox.Show("Failed to remove music", "Music Removal Error", MessageBox.MessageBoxButtons.Ok);
            }
            else
            {
                // check to delete music file
                MessageBox.MessageBoxResult res = await MessageBox.Show($"Would you like to delete\n{music.FileName} as well?", "Music Removal", MessageBox.MessageBoxButtons.YesNoCancel);
                if (res == MessageBox.MessageBoxResult.Yes)
                {
                    Global.Files.Remove(Global.Workspace.GetFilePath($"audio/{music.FileName}"));
                }

                System.Collections.IEnumerable source = MusicList.ItemsSource;
                MusicList.ItemsSource = null;
                MusicList.ItemsSource = source;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void MusicEditButton_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            MusicList.SelectedItem is MexMusic music)
        {
            Global.StopMusic();

            string path = Global.Workspace.GetFilePath($"audio/{music.FileName}");

            if (!Global.Files.Exists(path))
            {
                await MessageBox.Show($"Music file not found\n{music.FileName}", "File not Found", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            // load dsp
            DSP dsp = HPS.ToDSP(Global.Files.Get(path));

            // create editor popup
            AudioLoopEditor popup = new();
            popup.SetAudio(dsp);
            if (App.MainWindow != null)
            {
                await popup.ShowDialog(App.MainWindow);

                if (popup.Result == AudioLoopEditor.AudioEditorResult.SaveChanges)
                {
                    DSP? newdsp = popup.ApplyChanges();

                    if (newdsp != null)
                    {
                        using MemoryStream m = new();
                        HPS.WriteDSPAsHPS(newdsp, m);
                        Global.Files.Set(path, m.ToArray());
                    }
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void MusicList_DoubleClicked(object? sender, TappedEventArgs args)
    {
        PlaySelectedMusic();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void MusicList_AddNewMusic(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace == null)
            return;

        Global.Workspace.Project.Music.Add(new MexMusic());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void SeriesList_AddNew(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace == null)
            return;

        MexSeries series = new()
        {
            Name = "New Series"
        };
        Global.Workspace.Project.Series.Add(series);
        SeriesList.SelectedItem = series;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void SeriesList_Remove(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace == null)
            return;

        if (SeriesList.SelectedItem is MexSeries series)
        {
            MessageBox.MessageBoxResult ask = await MessageBox.Show($"Are you sure you want\nto remove \"{series.Name}\"?",
                "Remove Series",
                MessageBox.MessageBoxButtons.YesNoCancel);

            if (ask != MessageBox.MessageBoxResult.Yes)
                return;

            int currentIndex = SeriesList.SelectedIndex;
            Global.Workspace.Project.RemoveSeries(Global.Workspace, series);

            SeriesList.RefreshList();
            SeriesList.SelectedIndex = currentIndex;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void AddCode_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace == null)
            return;

        MexCode code = new();
        Global.Workspace.Project.Codes.Add(code);
        CodesList.SelectedItem = code;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void RemoveCode_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace == null)
            return;

        if (CodesList.SelectedItem is MexCode code)
        {
            MessageBox.MessageBoxResult ask = await MessageBox.Show($"Are you sure you want\nto remove \"{code.Name}\"?",
                "Remove Code",
                MessageBox.MessageBoxButtons.YesNoCancel);

            if (ask != MessageBox.MessageBoxResult.Yes)
                return;

            int currentIndex = CodesList.SelectedIndex;
            Global.Workspace.Project.Codes.Remove(code);
            CodesList.RefreshList();
            CodesList.SelectedIndex = currentIndex;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SeriesGenerateIconButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            SeriesList.SelectedItem is MexSeries series)
        {
            mexLib.Utilties.ObjFile? obj = series.ModelAsset.GetOBJFile(Global.Workspace);

            if (obj == null)
                return;

            ObjRasterizer raster = new(series.ModelAsset);
            byte[] png = raster.SaveDrawingToPng(80, 64);

            using MemoryStream stream = new(png);
            series.IconAsset.SetFromImageFile(Global.Workspace, stream);

            SeriesList.SelectedItem = null;
            SeriesList.SelectedItem = series;
        }
    }
}
