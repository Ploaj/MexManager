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
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (Global.Workspace == null)
            return;

        if (FileTextBox.Text == null)
            return;

        var path = Global.Workspace.GetFilePath(FileTextBox.Text);

        if (!Global.Workspace.FileManager.Exists(path))
        {
            PreviewImage.Source = BitmapManager.MissingImage;
            return;
        }

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

        if (FileTextBox.Text == null)
            return;

        if (string.IsNullOrEmpty(FileTextBox.Text))
        {
            await MessageBox.Show("Please input a file path", "Import Media Error", MessageBox.MessageBoxButtons.Ok);
            return;
        }

        var path = Global.Workspace.GetFilePath(FileTextBox.Text);

        var file = await FileIO.TryOpenFile("Import JPEG", Path.GetFileNameWithoutExtension(FileTextBox.Text) + ".jpg", FileIO.FilterJpeg);

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

        if (FileTextBox.Text == null)
            return;

        var path = Global.Workspace.GetFilePath(FileTextBox.Text);

        if (!Global.Workspace.FileManager.Exists(path))
            return;

        var file = await FileIO.TrySaveFile("Export JPEG", Path.GetFileNameWithoutExtension(FileTextBox.Text) + ".jpg", FileIO.FilterJpeg);

        if (file == null) return;

        var thp = new THP(Global.Workspace.FileManager.Get(path));
        File.WriteAllBytes(file, thp.ToJPEG());
    }
}