using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using mexLib;
using mexLib.AssetTypes;
using MexManager.Tools;
using System.IO;
using System.Linq;

namespace MexManager.Controls;

public partial class TextureAssetEditor : UserControl
{
    /// <summary>
    /// 
    /// </summary>
    public TextureAssetEditor()
    {
        InitializeComponent();
        MainPanel.AddHandler(DragDrop.DragEnterEvent, ImageDragEnter);
        MainPanel.AddHandler(DragDrop.DragOverEvent, ImageDragOver);
        MainPanel.AddHandler(DragDrop.DropEvent, ImageDrop);

        DataContextChanged += (s, e) =>
        {
            UpdateImage();
        };
    }
    /// <summary>
    /// 
    /// </summary>
    private void ImportImage(string filePath)
    {
        if (Global.Workspace != null &&
            DataContext is MexTextureAsset asset)
        {
            using FileStream stream = new(filePath, FileMode.Open);
            asset.SetFromImageFile(Global.Workspace, stream);
            UpdateImage();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private void UpdateImage()
    {
        if (DataContext is MexTextureAsset asset &&
            Global.Workspace != null &&
            asset.GetSourceImage(Global.Workspace) is MexImage tex)
        {
            TexturePreview.Source = tex.ToBitmap();
        }
        else
        {
            TexturePreview.Source = null;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImageDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImageDragOver(object? sender, DragEventArgs e)
    {
        // Check if the data is file data
        if (e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImageDrop(object? sender, DragEventArgs e)
    {
        // Get the dropped file names
        if (e.Data.Contains(DataFormats.Files))
        {
            System.Collections.Generic.IEnumerable<Avalonia.Platform.Storage.IStorageItem>? fileNames = e.Data.GetFiles();
            if (fileNames == null)
                return;

            foreach (string? f in fileNames.Select(e => e.Path.AbsolutePath))
            {
                if (f.EndsWith(".png"))
                {
                    ImportImage(f);
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
    private async void ImportButton_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null)
        {
            // get file to import
            string? file = await FileIO.TryOpenFile("Texture File", "", FileIO.FilterPng);

            // no file to import
            if (file == null)
                return;

            // import from file
            ImportImage(file);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void ExportButton_Click(object? sender, RoutedEventArgs args)
    {
        if (DataContext is MexTextureAsset asset &&
            Global.Workspace != null)
        {
            string? file = await FileIO.TrySaveFile("Texture File", "", FileIO.FilterPng);
            if (file != null)
            {
                asset.GetSourceImage(Global.Workspace)?.ToBitmap().Save(file);
            }
        }
    }
}