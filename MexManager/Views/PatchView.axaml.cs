using Avalonia.Controls;
using Avalonia.Interactivity;
using HSDRaw;
using mexLib.Types;
using MexManager.Extensions;
using MexManager.Tools;

namespace MexManager.Views;

public partial class PatchView : UserControl
{
    /// <summary>
    /// 
    /// </summary>
    public PatchView()
    {
        InitializeComponent();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void AddCode_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace == null)
            return;

        System.Collections.Generic.IEnumerable<string> filePaths = await FileIO.TryOpenFiles("Export Patch", "", FileIO.FilterHSD);

        if (filePaths == null)
            return;

        foreach (string filePath in filePaths)
        {
            try
            {
                HSDRawFile f = new(filePath);

                foreach (HSDRootNode? r in f.Roots)
                {
                    MexCodePatch patch = new(r.Name, new mexLib.HsdObjects.HSDFunctionDat()
                    {
                        _s = r.Data._s
                    });
                    Global.Workspace.Project.Patches.Add(patch);
                    CodesList.SelectedItem = patch;
                }
            }
            catch
            {

            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void ExportCode_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace == null)
            return;

        if (CodesList.SelectedItem is MexCodePatch code)
        {
            string? filePath = await FileIO.TrySaveFile("Export Patch", $"{code.Name}.dat", FileIO.FilterHSD);

            if (filePath == null)
                return;

            HSDRawFile f = new();
            f.Roots.Add(new HSDRootNode()
            {
                Name = code.Name,
                Data = code.Function,
            });
            f.Save(filePath);
        }
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

        if (CodesList.SelectedItem is MexCodePatch code)
        {
            MessageBox.MessageBoxResult ask = await MessageBox.Show($"Are you sure you want\nto remove \"{code.Name}\"?",
                "Remove Patch",
                MessageBox.MessageBoxButtons.YesNoCancel);

            if (ask != MessageBox.MessageBoxResult.Yes)
                return;

            int currentIndex = CodesList.SelectedIndex;
            Global.Workspace.Project.Patches.Remove(code);
            CodesList.RefreshList();
            CodesList.SelectedIndex = currentIndex;
        }
    }
}