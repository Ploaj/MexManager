using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.PropertyGrid.Controls;
using mexLib.Attributes;
using PropertyModels.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MexManager;

public partial class ConfigWindow : Window
{
    public bool Confirmed { get; internal set; } = false;

    /// <summary>
    /// 
    /// </summary>
    private PropertyGrid? PropertyGridItem => this.FindControl<PropertyGrid>("PropertyGrid");

    /// <summary>
    /// 
    /// </summary>
    public ConfigWindow()
    {
        InitializeComponent();

        if (PropertyGridItem != null)
            PropertyGridItem.DataContext = App.Settings;
    }
    /// <summary>
    /// 
    /// </summary>
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ValidateISO_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var res = MeleeISOValidator.IsValid(App.Settings.MeleePath);
        if (res == null || res.IsSuccess())
        {
            MessageBox.Show("ISO is valid", "ISO Validation", MessageBox.MessageBoxButtons.Ok);
        }
        else
        {
            if (res.ErrorMessage != null)
                MessageBox.Show($"Error: {res.ErrorMessage}", "ISO Validation", MessageBox.MessageBoxButtons.Ok);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConfirmButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Confirmed = true;
        App.Settings.Save();
        Close();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="confirm_text"></param>
    /// <param name="o"></param>
    /// <returns></returns>
    public async static Task<bool> ShowDialog()
    {
        var popup = new ConfigWindow();
        if (App.MainWindow != null)
        {
            await popup.ShowDialog(App.MainWindow);
            return popup.Confirmed;
        }
        return false;
    }
}