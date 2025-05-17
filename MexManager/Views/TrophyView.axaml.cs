using Avalonia.Controls;
using Avalonia.Interactivity;
using mexLib.Types;
using MexManager.Tools;
using MexManager.ViewModels;
using System.IO;

namespace MexManager.Views;

public partial class TrophyView : UserControl
{
    /// <summary>
    /// 
    /// </summary>
    public TrophyView()
    {
        InitializeComponent();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    private async void ImportTrophyFromPackage(Stream stream)
    {
        if (Global.Workspace != null &&
            DataContext is TrophyViewModel model &&
            model.Trophies != null)
        {
            mexLib.Installer.MexInstallerError? error = MexTrophy.FromPackage(Global.Workspace, stream, out MexTrophy? trophy);

            if (error != null)
            {
                await MessageBox.Show(error.Message, "Trophy Import Error", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            if (trophy == null)
            {
                await MessageBox.Show("Failed to create trophy", "Trophy Import Error", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            // trophy.SortSeries = (short)model.SeriesOrder.Count;
            model.Trophies.Add(trophy);
            model.SeriesOrder.Insert(trophy.SortSeries + 1, trophy);
            model.UpdateSeriesOrder();
            model.SelectedTrophy = trophy;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ImportTrophy_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is TrophyViewModel model &&
            model.Trophies != null)
        {
            string? file = await FileIO.TryOpenFile("Import Trophy", "", FileIO.FilterZip);

            if (file == null)
                return;

            using FileStream stream = new(file, FileMode.Open);
            ImportTrophyFromPackage(stream);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ExportTrophy_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is TrophyViewModel model &&
            model.SelectedTrophy is MexTrophy trophy)
        {
            string? file = await FileIO.TrySaveFile("Export Trophy", trophy.Name + ".zip", FileIO.FilterZip);

            if (file == null)
                return;

            using FileStream stream = new(file, FileMode.Create);
            trophy.ToPackage(Global.Workspace, stream);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddTrophy_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TrophyViewModel model &&
            model.Trophies != null)
        {
            MexTrophy trophy = new()
            {
                SortSeries = (short)model.SeriesOrder.Count,
            };

            model.Trophies.Add(trophy);
            model.SeriesOrder.Add(trophy);
            model.SelectedTrophy = trophy;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveTrophy_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is TrophyViewModel model &&
            model.Trophies != null &&
            model.SelectedTrophy is MexTrophy trophy)
        {
            int index = model.Trophies.IndexOf(trophy);
            if (index <= 292)
                return;

            // heck are you sure want to delete
            MessageBox.MessageBoxResult res = await MessageBox.Show($"Are you sure you want\nto remove \"{trophy.Name}\"?", "Remove Trophy", MessageBox.MessageBoxButtons.YesNoCancel);
            if (res != MessageBox.MessageBoxResult.Yes)
                return;

            // check to delete file
            if (Global.Workspace.FileManager.Exists(Global.Workspace.GetFilePath(trophy.Data.File.File)))
            {
                res = await MessageBox.Show($"Delete \"{trophy.Data.File.File}\"?", "Delete Trophy File", MessageBox.MessageBoxButtons.YesNoCancel);
                if (res == MessageBox.MessageBoxResult.Yes)
                    Global.Workspace.FileManager.Remove(Global.Workspace.GetFilePath(trophy.Data.File.File));
            }

            if (trophy.HasUSData &&
                Global.Workspace.FileManager.Exists(Global.Workspace.GetFilePath(trophy.USData.File.File)))
            {
                res = await MessageBox.Show($"Delete \"{trophy.USData.File.File}\"?", "Delete Trophy File", MessageBox.MessageBoxButtons.YesNoCancel);
                if (res == MessageBox.MessageBoxResult.Yes)
                    Global.Workspace.FileManager.Remove(Global.Workspace.GetFilePath(trophy.USData.File.File));
            }

            model.Trophies.Remove(trophy);
            model.SeriesOrder.Remove(trophy);
            model.UpdateSeriesOrder();
            FilteredTrophyList.SelectedIndex = index;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DuplicateTrophy_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is TrophyViewModel model &&
            model.SelectedTrophy is MexTrophy source &&
            model.Trophies != null)
        {
            using MemoryStream mem = new();
            source.ToPackage(Global.Workspace, mem);
            mem.Position = 0;
            ImportTrophyFromPackage(mem);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveUpTrophy_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TrophyViewModel model &&
            model.Trophies != null &&
            model.SelectedTrophy is MexTrophy trophy)
        {
            int index = model.Trophies.IndexOf(trophy);

            if (index - 1 <= 292)
                return;

            (model.Trophies[index], model.Trophies[index - 1]) = (model.Trophies[index - 1], model.Trophies[index]);

            model.SelectedTrophy = trophy;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveDownTrophy_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TrophyViewModel model &&
            model.Trophies != null &&
            model.SelectedTrophy is MexTrophy trophy)
        {
            int index = model.Trophies.IndexOf(trophy);

            if (index + 1 >= model.Trophies.Count)
                return;

            (model.Trophies[index], model.Trophies[index + 1]) = (model.Trophies[index + 1], model.Trophies[index]);

            model.SelectedTrophy = trophy;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NumericUpDown_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (DataContext is TrophyViewModel model &&
            model.SelectedTrophy is MexTrophy trophy &&
            model.Trophies != null &&
            model.SeriesOrder != null)
        {
            short old_index = trophy.SortSeries;
            decimal? new_value = e.NewValue;

            new_value ??= 0;

            if (new_value == trophy.SortSeries)
                return;

            if (new_value >= model.Trophies.Count)
                new_value = model.Trophies.Count - 1;

            model.SeriesOrder.Move(old_index, (int)new_value);
            model.UpdateSeriesOrder();

            // reselect trophy
            model.SelectedTrophy = trophy;
        }
    }
}