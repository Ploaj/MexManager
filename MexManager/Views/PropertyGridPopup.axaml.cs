using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.PropertyGrid.Controls;
using System.Threading.Tasks;

namespace MexManager.Views;

public partial class PropertyGridPopup : Window
{
    public bool Confirmed { get; internal set; } = false;

    /// <summary>
    /// 
    /// </summary>
    private PropertyGrid? PropertyGridItem => this.FindControl<PropertyGrid>("PropertyGrid");

    private Button? FindConfirmButton => this.FindControl<Button>("ConfirmButton");

    /// <summary>
    /// 
    /// </summary>
    public PropertyGridPopup()
    {
        InitializeComponent();
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
    /// <param name="o"></param>
    public void SetObject(string title, string confirm_text, object? o)
    {
        if (PropertyGridItem == null || FindConfirmButton == null)
            return;

        Title = title;
        FindConfirmButton.Content = confirm_text;

        PropertyGridItem.DataContext = o;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConfirmButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Confirmed = true;
        this.Close();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="confirm_text"></param>
    /// <param name="o"></param>
    /// <returns></returns>
    public async static Task<bool> ShowDialog(string title, string confirm_text, object? o)
    {
        var popup = new PropertyGridPopup();
        popup.SetObject(title, confirm_text, o);
        if (App.MainWindow != null)
        {
            await popup.ShowDialog(App.MainWindow);
            return popup.Confirmed;
        }
        return false;
    }
}