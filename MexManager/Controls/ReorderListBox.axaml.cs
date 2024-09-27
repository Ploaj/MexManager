using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

namespace MexManager.Controls;

public partial class ReorderListBox : UserControl
{
    //private object? _draggedItem;
    //private int _originalIndex = -1;

    public ReorderListBox()
    {
        InitializeComponent();

        //PointerPressed += OnPointerPressed;
    }
    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="e"></param>
    //private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    //{
    //    Debug.WriteLine("Pressed");
    //    // Get the item that was clicked
    //    var listItem = GetItemFromEvent(e);
    //    if (listItem == null) return;
        
    //    //_draggedItem = listItem;
    //    //_originalIndex = this.Items?.Cast<object>().ToList().IndexOf(_draggedItem) ?? -1;
    //}
    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="e"></param>
    ///// <returns></returns>
    //private object? GetItemFromEvent(RoutedEventArgs e)
    //{
    //    //var point = e.GetCurrentPoint(this);
    //    //var hitControl = this.InputHitTest(point.Position) as IControl;
    //    //if (hitControl != null)
    //    //{
    //    //    return hitControl.DataContext;
    //    //}
    //    return null;
    //}
}