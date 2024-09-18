using Avalonia.Controls;
using Avalonia.Interactivity;
using mexLib;
using mexLib.Attributes;
using mexLib.Types;
using mexLib.Utilties;
using MexManager.Extensions;
using MexManager.Tools;
using MexManager.ViewModels;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;

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
    private void AddFighterMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Global.Workspace != null)
        {
            Global.Workspace.Project.AddNewFighter(new MexFighter()
            {
                Name = "NewFighter"
            });
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

            if (!Global.Workspace.Project.RemoveFighter(Global.Workspace, FighterList.SelectedIndex))
            {
                await MessageBox.Show($"Could not remove \"{fighter.Name}\"\nYou cannot remove base game fighters", "Remove Fighter Error", MessageBox.MessageBoxButtons.Ok);
            }
        }
        FighterList.RefreshList();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ImportFighterMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Global.Workspace == null)
            return;

        var file = await FileIO.TryOpenFile("Import Fighter", "", FileIO.FilterZip);
        if (file != null)
        {
            using var stream = new FileStream(file, FileMode.Open);

            var res = MexFighter.FromPackage(Global.Workspace, stream, out MexFighter fighter);

            if (res == null)
            {
                var addfighter = Global.Workspace?.Project.AddNewFighter(fighter);
                FighterList.RefreshList();
                FighterList.SelectedItem = fighter;
            }
            else
            {
                await MessageBox.Show(res.Message, "Import Fighter Error", MessageBox.MessageBoxButtons.Ok);
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
        if (Global.Workspace != null &&
            FighterList.SelectedItem is MexFighter fighter)
        {
            var file = await FileIO.TrySaveFile("Export Fighter", fighter.Name + ".zip", FileIO.FilterZip);
            if (file != null)
            {
                var options = new MexFighter.FighterPackOptions();

                if (!await PropertyGridPopup.ShowDialog("Fighter Export Options", "Export Fighter", options))
                    return;

                using var stream = new FileStream(file, FileMode.Create);
                fighter.ToPackage(Global.Workspace, stream, options);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void DuplicateFighterMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            FighterList.SelectedItem is MexFighter fighter)
        {
            var options = new MexFighter.FighterPackOptions()
            {
                ExportFiles = false,
                ExportCostumes = false,
                ExportMedia = false,
                ExportSoundBank = false,
            };

            if (!await PropertyGridPopup.ShowDialog("Fighter Duplicate Options", "Duplicate Fighter", options))
                return;

            using var stream = new MemoryStream();
            fighter.ToPackage(Global.Workspace, stream, options);
            stream.Position = 0;
            MexFighter.FromPackage(Global.Workspace, stream, out MexFighter? newfighgter);
            if (newfighgter != null)
            {
                Global.Workspace.Project.AddNewFighter(newfighgter);
                FighterList.SelectedItem = newfighgter;
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
                        using var stream = new FileStream(zipPath, FileMode.Open);
                        StringBuilder log = new ();
                        var costume = MexCostume.FromZip(Global.Workspace, stream, log);

                        if (log.Length != 0)
                            await MessageBox.Show(log.ToString(), "Import Log", MessageBox.MessageBoxButtons.Ok);

                        foreach (var c in costume)
                            fighter.Costumes.Add(c);
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

            using var stream = new FileStream(zipPath, FileMode.Create);
            costume.PackToZip(Global.Workspace, stream);
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
            // ask are you sure
            var res = await MessageBox.Show($"Are you sure you want\nto remove \"{costume.Name}\"?", "Remove Costume", MessageBox.MessageBoxButtons.YesNoCancel);
            if (res != MessageBox.MessageBoxResult.Yes)
                return;

            // ask to delete files
            res = await MessageBox.Show($"Would you like to delete\n\"{costume.File.FileName}\" as well?", "Delete Costume File", MessageBox.MessageBoxButtons.YesNoCancel);
            if (res == MessageBox.MessageBoxResult.Yes)
            {
                costume.DeleteFiles(Global.Workspace);
            }

            // delete assets
            costume.DeleteAssets(Global.Workspace);

            // finally remove costume
            fighter.Costumes.Remove(costume);
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
                File = new MexCostumeVisibilityFile()
                {
                    FileName = costume.File.FileName,
                    JointSymbol = costume.File.JointSymbol,
                    MaterialSymbol = costume.File.MaterialSymbol,
                    VisibilityIndex = costume.File.VisibilityIndex,
                },
                KirbyFile = new MexCostumeFile()
                {
                    FileName = costume.KirbyFile.FileName,
                    JointSymbol = costume.KirbyFile.JointSymbol,
                    MaterialSymbol = costume.KirbyFile.MaterialSymbol,
                },
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FighterList_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        CostumeList.SelectedIndex = 0;
        ItemList.SelectedIndex = 0;
    }
}