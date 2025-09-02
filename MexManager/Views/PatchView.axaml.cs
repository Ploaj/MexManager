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

        var filePaths = await FileIO.TryOpenFiles("Export Patch", "", FileIO.FilterHSD);

        if (filePaths == null)
            return;

        foreach (var filePath in filePaths)
        {
            try
            {
                var f = new HSDRawFile(filePath);

                foreach (var r in f.Roots)
                {
                    MexCodePatch patch = new (r.Name, new mexLib.HsdObjects.HSDFunctionDat()
                    {
                        _s = r.Data._s
                    });
                    Global.Workspace.Project.Patches.Add(patch);
                    CodesList.SelectedItem = patch;
                }
            } catch
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
            var filePath = await FileIO.TrySaveFile("Export Patch", $"{code.Name}.dat", FileIO.FilterHSD);

            if (filePath == null)
                return;

            var f = new HSDRawFile();
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