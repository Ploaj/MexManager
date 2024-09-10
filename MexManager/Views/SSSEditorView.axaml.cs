using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mexLib;
using mexLib.Types;
using MexManager.Extensions;
using MexManager.ViewModels;
using System.ComponentModel;

namespace MexManager.Views;

public partial class SSSEditorView : UserControl
{
    public SSSEditorView()
    {
        InitializeComponent();

        SelectScreen.OnSwap += (i, j) =>
        {
            if (Global.Workspace != null &&
                DataContext is MainViewModel model &&
                model.StageSelect != null)
            {
                var Icons = model.StageSelect.StageIcons;
                (Icons[i], Icons[j]) = (Icons[j], Icons[i]);
            }

            ApplySelectTemplate();
        };
    }
    /// <summary>
    /// 
    /// </summary>
    private void ApplySelectTemplate()
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void AddIcon_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.StageSelect != null)
        {
            model.StageSelect.StageIcons.Add(new MexStageSelectIcon()
            {

            });

            IconList.SelectedIndex = model.StageSelect.StageIcons.Count - 1;

            if (model.AutoApplyCSSTemplate)
                ApplySelectTemplate();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void RemoveIcon_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.StageSelect != null &&
            model.SelectedSSSIcon is MexStageSelectIcon icon)
        {
            var res = await MessageBox.Show("Are you sure you want\nto remove this icon?", "Remove Icon", MessageBox.MessageBoxButtons.YesNoCancel);

            if (res != MessageBox.MessageBoxResult.Yes)
                return;

            int index = IconList.SelectedIndex;
            model.StageSelect.StageIcons.Remove(icon);
            IconList.SelectedIndex = index;

            if (model.AutoApplyCSSTemplate)
                ApplySelectTemplate();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void MoveUpIcon_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.StageSelect != null)
        {
            int index = IconList.SelectedIndex;
            if (index > 0)
            {
                model.StageSelect.StageIcons.Move(index, index - 1);
                IconList.SelectedIndex = index - 1;

                if (model.AutoApplyCSSTemplate)
                    ApplySelectTemplate();
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void MoveDownIcon_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.StageSelect != null)
        {
            int index = IconList.SelectedIndex;
            if (index != -1 &&
                index + 1 < model.StageSelect.StageIcons.Count)
            {
                model.StageSelect.StageIcons.Move(index, index + 1);
                IconList.SelectedIndex = index + 1;
                ApplySelectTemplate();
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void IconPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        ApplySelectTemplate();

        if (args.PropertyName == nameof(MexStageSelectIcon.StageID))
            IconList.RefreshList(IconList.SelectedIndex);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        foreach (MexReactiveObject i in e.RemovedItems)
        {
            i.PropertyChanged -= IconPropertyChanged;
        }

        foreach (MexReactiveObject i in e.AddedItems)
        {
            i.PropertyChanged += IconPropertyChanged;
        }

        SelectScreen.InvalidateVisual();
    }
}