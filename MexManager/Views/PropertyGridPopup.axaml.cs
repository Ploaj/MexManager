using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.PropertyGrid.Controls;

namespace MexManager.Views;

public partial class PropertyGridPopup : Window
{
    public bool Confirmed { get; internal set; } = false;

    /// <summary>
    /// 
    /// </summary>
    private PropertyGrid? PropertyGridItem => this.FindControl<PropertyGrid>("PropertyGrid");

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
    public void SetObject(object? o)
    {
        if (PropertyGridItem == null)
            return;

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
}