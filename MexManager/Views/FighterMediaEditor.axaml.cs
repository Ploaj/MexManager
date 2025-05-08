using Avalonia.Controls;
using Avalonia.Media.Imaging;
using MeleeMedia.Video;
using MexManager.Tools;
using System.IO;

namespace MexManager.Views;

public partial class FighterMediaEditor : UserControl
{
    private Bitmap? _previewBitmap;

    /// <summary>
    /// 
    /// </summary>
    public FighterMediaEditor()
    {
        InitializeComponent();

        DataContextChanged += (s, a) =>
        {
            Update();
        };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Update()
    {
        if (Global.Workspace == null)
            return;

        if (DataContext is not string filename ||
            !Global.Workspace.FileManager.Exists(Global.Workspace.GetFilePath(filename)))
        {
            _previewBitmap?.Dispose();
            _previewBitmap = null;
            PreviewImage.Source = null;
            return;
        }

        var path = Global.Workspace.GetFilePath(filename);
        var thp = new THP(Global.Workspace.FileManager.Get(path));
        UpdatePreview(thp);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="thp"></param>
    private void UpdatePreview(THP thp)
    {
        using var jpg = new MemoryStream(thp.ToJPEG());
        _previewBitmap?.Dispose();
        _previewBitmap = new Bitmap(jpg);
        PreviewImage.Source = _previewBitmap;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ImportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Global.Workspace == null)
            return;

        if (DataContext is not string text)
            return;

        if (string.IsNullOrEmpty(text))
        {
            await MessageBox.Show("Please input a file path", "Import Media Error", MessageBox.MessageBoxButtons.Ok);
            return;
        }

        var path = Global.Workspace.GetFilePath(text);

        var file = await FileIO.TryOpenFile("Import JPEG", Path.GetFileNameWithoutExtension(text) + ".jpg", FileIO.FilterJpeg);

        if (file == null) return;

        var thp = THP.FromJPEG(File.ReadAllBytes(file));
        Global.Workspace.FileManager.Set(path, thp.Data);
        UpdatePreview(thp);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ExportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Global.Workspace == null)
            return;

        if (DataContext is not string text)
            return;

        var path = Global.Workspace.GetFilePath(text);

        if (!Global.Workspace.FileManager.Exists(path))
            return;

        var file = await FileIO.TrySaveFile("Export JPEG", Path.GetFileNameWithoutExtension(text) + ".jpg", FileIO.FilterJpeg);

        if (file == null) return;

        var thp = new THP(Global.Workspace.FileManager.Get(path));
        File.WriteAllBytes(file, thp.ToJPEG());
    }
}