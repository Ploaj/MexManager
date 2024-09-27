using Avalonia.Controls;
using Avalonia.Interactivity;
using mexLib.Types;
using MexManager.ViewModels;

namespace MexManager.Views;

public partial class TrophyView : UserControl
{
    public TrophyView()
    {
        InitializeComponent();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImportTrophy_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: import trophy
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ExportTrophy_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: export trophy
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddTrophy_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: add trophy
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RemoveTrophy_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: remove trophy
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DuplicateTrophy_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: duplicate trophy
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveUpTrophy_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: move up trophy
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveDownTrophy_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: move down trophy
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NumericUpDown_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (DataContext is TrophyViewModel model && 
            model.SelectedTrophy is MexTrophy trophy && 
            model.Trophies != null &&
            model.SeriesOrder != null)
        {
            var old_index = trophy.SortSeries;
            var new_value = e.NewValue;

            if (new_value == null)
                new_value = 0;

            if (new_value == trophy.SortSeries)
                return;

            if (new_value >= model.Trophies.Count)
                new_value = model.Trophies.Count - 1;

            model.SeriesOrder.Move((int)old_index, (int)new_value);

            // update series sort indices
            for (int i = 0; i < model.SeriesOrder.Count; i++)
            {
                model.SeriesOrder[i].SortSeries = (short)i;
            }

            // reselect trophy
            model.SelectedTrophy = trophy;
        }
    }
}