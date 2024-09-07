using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using mexLib;
using mexLib.Types;
using mexLib.Utilties;
using MexManager.Tools;
using MexManager.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace MexManager.Views;

public partial class FighterView : UserControl
{
    /// <summary>
    /// 
    /// </summary>
    public FighterView()
    {
        InitializeComponent();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddFighterMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // add new fighter
        var addfighter = Global.Workspace?.Project.AddNewFighter(new MexFighter());
        if (addfighter == null)
        {
            await MessageBox.Show("Failed to add new fighter\nNo workspace is currently loaded.", "Add Fighter Error", MessageBox.MessageBoxButtons.Ok);
        }
        else if (addfighter == false)
        {
            await MessageBox.Show("Failed to add new fighter", "Add Fighter Error", MessageBox.MessageBoxButtons.Ok);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveFighterMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            FighterList.SelectedItem is MexFighter fighter)
        {
            var res =
                await MessageBox.Show(
                    $"Are you sure you want to\nremove \"{fighter.Name}\"?",
                    "Remove Fighter",
                    MessageBox.MessageBoxButtons.YesNoCancel);

            if (res != MessageBox.MessageBoxResult.Yes)
                return;

            if (!Global.Workspace.Project.RemoveFighter(FighterList.SelectedIndex))
            {
                await MessageBox.Show($"Could not remove \"{fighter.Name}\"\nYou cannot remove base game fighters", "Remove Fighter Error", MessageBox.MessageBoxButtons.Ok);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ImportFighterMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // TODO: import fighter zip
        if (Global.Workspace == null)
        {
            await MessageBox.Show("Please open a workspace", "Workspace Error", MessageBox.MessageBoxButtons.Ok);
        }

        var file = await FileIO.TryOpenFile("Import Fighter", "", FileIO.FilterJson);
        if (file != null)
        {
            var mario = MexJsonSerializer.Deserialize<MexFighter>(file);
            if (mario == null)
                return;
            var addfighter = Global.Workspace?.Project.AddNewFighter(mario);
            if (addfighter == null || addfighter == false)
            {
                await MessageBox.Show("Failed to add new fighter", "Add Fighter Error", MessageBox.MessageBoxButtons.Ok);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ExportFighterMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // TODO: export fighter zip
        if (FighterList.SelectedItem is MexFighter fighter)
        {
            var file = await FileIO.TrySaveFile("Export Fighter", "", FileIO.FilterJson);
            if (file != null)
            {
                File.WriteAllText(file, MexJsonSerializer.Serialize(fighter));
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddFighterItemMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (FighterList.SelectedItem is MexFighter fighter)
        {
            fighter.Items.Add(new MexItem());
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveFighterItemMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel model &&
            model.SelectedFighter is MexFighter fighter &&
            model.SelectedFighterItem is MexItem item)
        {
            var res = await MessageBox.Show(
                    $"Are you sure you want\nto remove\"{item.Name}\"?",
                    "Remove Item",
                    MessageBox.MessageBoxButtons.YesNoCancel);

            if (res == MessageBox.MessageBoxResult.Yes)
            {
                var selected = ItemList.SelectedIndex;
                fighter.Items.Remove(item);
                ItemList.SelectedIndex = selected;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ImportCostumeMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.SelectedFighter is MexFighter fighter)
        {
            var zipPath = await FileIO.TryOpenFile("Import Costume", "",
            [
                new ("Supported Formats")
                {
                    Patterns = [ "*.zip", "*.dat" ],
                },
            ]);

            if (zipPath == null) return;

            switch (Path.GetExtension(zipPath))
            {
                case ".zip":
                    {
                        var costume = MexCostume.FromZip(Global.Workspace, zipPath, out string log);

                        if (!string.IsNullOrEmpty(log))
                            await MessageBox.Show(log, "Import Log", MessageBox.MessageBoxButtons.Ok);

                        if (costume != null)
                            fighter.Costumes.Add(costume);
                    }
                    break;
                case ".dat":
                    {
                        var costume = MexCostume.FromDATFile(Global.Workspace, zipPath, out string log);

                        if (!string.IsNullOrEmpty(log))
                            await MessageBox.Show(log, "Import Log", MessageBox.MessageBoxButtons.Ok);

                        if (costume != null)
                            fighter.Costumes.Add(costume);
                    }
                    break;
            }

        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ExportCostumeMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.SelectedFighterCostume is MexCostume costume)
        {
            var zipPath = await FileIO.TrySaveFile("Export Costume", $"{costume.Name}.zip", FileIO.FilterZip);

            if (zipPath == null) return;

            costume.PackToZip(Global.Workspace, zipPath);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveCostumeMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.SelectedFighter is MexFighter fighter &&
            model.SelectedFighterCostume is MexCostume costume)
        {
            var res = await MessageBox.Show($"Are you sure you want\nto remove \"{costume.Name}\"?", "Remove Costume", MessageBox.MessageBoxButtons.YesNoCancel);

            if (res != MessageBox.MessageBoxResult.Yes)
                return;

            fighter.Costumes.Remove(costume);
            //costume.RemoveAssets(Global.Workspace);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DuplicateCostumeMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null && 
            DataContext is MainViewModel model &&
            model.SelectedFighter is MexFighter fighter &&
            model.SelectedFighterCostume is MexCostume costume)
        {
            fighter.Costumes.Add(new MexCostume()
            {
                File = new MexCostumeFile()
                {
                    FileName = costume.File.FileName,
                    JointSymbol = costume.File.JointSymbol,
                    MaterialSymbol = costume.File.MaterialSymbol,
                },
                KirbyFile = new MexCostumeFile()
                {
                    FileName = costume.KirbyFile.FileName,
                    JointSymbol = costume.KirbyFile.JointSymbol,
                    MaterialSymbol = costume.KirbyFile.MaterialSymbol,
                },
                VisibilityIndex = costume.VisibilityIndex,
            });
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveUpCostumeMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.SelectedFighter is MexFighter fighter &&
            model.SelectedFighterCostume is MexCostume costume)
        {
            int index = fighter.Costumes.IndexOf(costume);

            // Ensure the item isn't the last one in the collection
            if (index > 0)
            {
                // Swap the item with the one below it
                (fighter.Costumes[index - 1], fighter.Costumes[index]) = (fighter.Costumes[index], fighter.Costumes[index - 1]);

                CostumeList.SelectedIndex = index - 1;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveDownCostumeMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.SelectedFighter is MexFighter fighter &&
            model.SelectedFighterCostume is MexCostume costume)
        {
            int index = fighter.Costumes.IndexOf(costume);

            // Ensure the item isn't the last one in the collection
            if (index < fighter.Costumes.Count - 1)
            {
                // Swap the item with the one below it
                (fighter.Costumes[index + 1], fighter.Costumes[index]) = (fighter.Costumes[index], fighter.Costumes[index + 1]);

                CostumeList.SelectedIndex = index + 1;
            }
        }
    }
}