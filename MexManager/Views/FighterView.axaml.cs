using Avalonia;
using Avalonia.Controls;
using HSDRaw.MEX;
using mexLib;
using mexLib.Utilties;
using MexManager.Tools;
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
    private void AddFighterMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // add new fighter
        var addfighter = Global.Workspace?.Project.AddNewFighter(new MexFighter());
        if (addfighter == null)
        {
            MessageBox.Show(App.MainWindow, "Failed to add new fighter\nNo workspace is currently loaded.", "Add Fighter Error", MessageBox.MessageBoxButtons.Ok);
        }
        else if (addfighter == false)
        {
            MessageBox.Show(App.MainWindow, "Failed to add new fighter", "Add Fighter Error", MessageBox.MessageBoxButtons.Ok);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RemoveFighterMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var result = Global.Workspace?.Project.RemoveFighter(FighterList.SelectedIndex);
        if (result == null)
        {
            MessageBox.Show(App.MainWindow, "No workspace is currently loaded.", "Add Fighter Error", MessageBox.MessageBoxButtons.Ok);
        }
        else if (result == false)
        {
            MessageBox.Show(App.MainWindow, "Cannot remove fighter", "Add Fighter Error", MessageBox.MessageBoxButtons.Ok);
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
            MessageBox.Show(App.MainWindow, "Please open a workspace", "Workspace Error", MessageBox.MessageBoxButtons.Ok);
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
                MessageBox.Show(App.MainWindow, "Failed to add new fighter", "Add Fighter Error", MessageBox.MessageBoxButtons.Ok);
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
    private void RemoveFighterItemMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (FighterList.SelectedItem is MexFighter fighter &&
            ItemList.SelectedItem is MexItem item)
        {
            fighter.Items.Remove(item);
        }
    }
}