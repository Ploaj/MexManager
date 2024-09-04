using Avalonia.Controls;
using mexLib;
using mexLib.Utilties;
using MexManager.Tools;
using MexManager.ViewModels;
using System.IO;

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
}