using Avalonia.Controls;
using Avalonia.Interactivity;
using mexLib.Types;
using MexManager.Extensions;
using MexManager.Tools;
using MexManager.ViewModels;
using System.IO;

namespace MexManager.Views;

public partial class StageView : UserControl
{
    /// <summary>
    /// 
    /// </summary>
    public StageView()
    {
        InitializeComponent();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void StageAddMenuItem_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.Stages != null)
        {
            var stage = new MexStage()
            {
                Playlist = new MexPlaylist()
                {
                    Entries =
                    [
                        new ()
                            {
                                MusicID = 0,
                                ChanceToPlay = 50,
                            }
                    ]

                }
            };

            if (Global.Workspace.Project.AddStage(stage) != -1)
            {
                StagesList.RefreshList();
                StagesList.SelectedItem = stage;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void StageRemoveMenuItem_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            StagesList.SelectedIndex != -1 &&
            model.SelectedStage is MexStage stage)
        {
            if (StagesList.SelectedIndex <= 70)
            {
                await MessageBox.Show(
                    $"Base game stages cannot be removed",
                    "Remove Stage Error",
                    MessageBox.MessageBoxButtons.Ok);
                return;
            }

            var res =
                await MessageBox.Show(
                    $"Are you sure you want to\nremove \"{stage.Name}\"?",
                    "Remove Stage",
                    MessageBox.MessageBoxButtons.YesNoCancel);

            if (res != MessageBox.MessageBoxResult.Yes)
                return;

            var sel = StagesList.SelectedIndex;
            if (Global.Workspace.Project.RemoveStage(Global.Workspace, StagesList.SelectedIndex))
            {
                StagesList.RefreshList();
                StagesList.SelectedIndex = sel;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void StageGenerateBanner_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.SelectedStage is MexStage stage)
        {
            var tex = Tools.StageBannerGenerator.DrawTextToImageAsync(stage.Location, stage.Name);
            stage.Assets.BannerAsset.SetFromMexImage(Global.Workspace, tex);
            StageAssetPropertyGrid.DataContext = null;
            StageAssetPropertyGrid.DataContext = stage.Assets;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void StageImportMenuItem_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null)
        {
            var file = await FileIO.TryOpenFile("Import Stage", "", FileIO.FilterZip);

            if (file == null)
                return;

            using var fs = new FileStream(file, FileMode.Open);
            var res = MexStage.FromPackage(fs, Global.Workspace, out MexStage? stage);
            if (res == null)
            {
                if (stage != null)
                {
                    StagesList.SelectedIndex = Global.Workspace.Project.AddStage(stage);
                }
            }
            else
            {
                await MessageBox.Show(res.Message, "Import Stage Failed", MessageBox.MessageBoxButtons.Ok);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void StageExportMenuItem_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.SelectedStage is MexStage stage)
        {
            var options = new MexStage.StagePackOptions()
            {
                ExportFiles = true,
                ExportSound = true,
            };

            if (!await PropertyGridPopup.ShowDialog("Stage Export Options", "Export Stage", options))
                return;

            var file = await FileIO.TrySaveFile("Export Stage File", stage.Name, FileIO.FilterZip);
            if (file == null)
                return;

            using var fs = new FileStream(file, FileMode.Create);
            MexStage.ToPackage(fs, Global.Workspace, stage, options);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void StageDuplicateMenuItem_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.SelectedStage is MexStage stage)
        {
            var options = new MexStage.StagePackOptions();
            if (!await PropertyGridPopup.ShowDialog("Stage Duplicate Options", "Duplicate Stage", options))
                return;

            using var fs = new MemoryStream();
            MexStage.ToPackage(fs, Global.Workspace, stage, options);
            fs.Position = 0;

            var res = MexStage.FromPackage(fs, Global.Workspace, out MexStage? duplicate);
            if (duplicate != null)
            {
                StagesList.SelectedIndex = Global.Workspace.Project.AddStage(duplicate);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void StageAddItemMenuItem_Click(object? sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel model &&
            model.SelectedStage is MexStage stage)
        {
            stage.Items.Add(new MexItem());
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void StageRemoveItemMenuItem_Click(object? sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel model &&
            model.SelectedStage is MexStage stage &&
            model.SelectedStageItem is MexItem item)
        {
            var res = await MessageBox.Show(
                $"Are you sure you want\nto remove\"{item.Name}\"?",
                "Remove Item",
                MessageBox.MessageBoxButtons.YesNoCancel);

            if (res == MessageBox.MessageBoxResult.Yes)
            {
                var selected = StageItemList.SelectedIndex;
                stage.Items.Remove(item);
                StageItemList.SelectedIndex = selected;
            }
        }
    }
}