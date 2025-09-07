using Avalonia.Controls;
using Avalonia.Interactivity;
using mexLib;
using mexLib.Types;
using MexManager.ViewModels;
using System.ComponentModel;
using System.Reactive.Linq;

namespace MexManager.Views;

public partial class CSSEditorView : UserControl
{
    public CSSEditorView()
    {
        InitializeComponent();

        SelectScreenProperties.DataContext = SelectScreen.Properties;

        SelectScreen.OnSwap += (i, j) =>
        {
            if (Global.Workspace != null &&
                DataContext is MainViewModel model &&
                model.CharacterSelect != null)
            {
                System.Collections.ObjectModel.ObservableCollection<MexCharacterSelectIcon> Icons = model.CharacterSelect.FighterIcons;
                //Icons.Move(i, j);
                (Icons[i], Icons[j]) = (Icons[j], Icons[i]);
            }
            ApplySelectTemplate();
        };

        TemplatePropertyGrid.DataContextChanged += (s, e) =>
        {
            if (Global.Workspace != null &&
                DataContext is MainViewModel model &&
                model.CharacterSelect != null)
            {
                model.CharacterSelect.Template.PropertyChanged += (s2, e2) =>
                {
                    ApplySelectTemplate();
                };
            }
            SelectScreen.InvalidateVisual();
        };
    }
    /// <summary>
    /// 
    /// </summary>
    private void ApplySelectTemplate()
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.CharacterSelect != null)
        {
            if (model.AutoApplyCSSTemplate && IconList.SelectedItems != null)
            {
                foreach (MexReactiveObject i in IconList.SelectedItems)
                    i.PropertyChanged -= IconPropertyChanged;

                model.CharacterSelect.Template.Apply(model.CharacterSelect.FighterIcons);

                foreach (MexReactiveObject i in IconList.SelectedItems)
                    i.PropertyChanged += IconPropertyChanged;
            }
            SelectScreen.InvalidateVisual();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void ApplyTemplate_Click(object? sender, RoutedEventArgs args)
    {
        ApplySelectTemplate();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void UndoButton_Click(object? sender, RoutedEventArgs args)
    {
        //SelectScreen.Undo();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void RedoButton_Click(object? sender, RoutedEventArgs args)
    {
        //SelectScreen.Redo();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void ApplyCompression_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.CharacterSelect != null)
        {
            MessageBox.MessageBoxResult res = await MessageBox.Show("Would you like to re-compress all CSPs?", "Apply CSP Compression", MessageBox.MessageBoxButtons.YesNoCancel);
            bool force = res == MessageBox.MessageBoxResult.Yes;

            await ProgressWindow.DisplayProgress((w) =>
            {
                model.CharacterSelect.ApplyCompression(Global.Workspace, force, (r, t) =>
                {
                    w.ReportProgress(t.ProgressPercentage, t.UserState);
                });
            });
        }
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
            model.CharacterSelect != null)
        {
            model.CharacterSelect.FighterIcons.Add(new MexCharacterSelectIcon()
            {

            });

            IconList.SelectedIndex = model.CharacterSelect.FighterIcons.Count - 1;

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
            model.CharacterSelect != null &&
            model.SelectedCSSIcon is MexCharacterSelectIcon icon)
        {
            MessageBox.MessageBoxResult res = await MessageBox.Show("Are you sure you want\nto remove this icon?", "Remove Icon", MessageBox.MessageBoxButtons.YesNoCancel);

            if (res != MessageBox.MessageBoxResult.Yes)
                return;

            int index = IconList.SelectedIndex;
            model.CharacterSelect.FighterIcons.Remove(icon);
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
            model.CharacterSelect != null)
        {
            int index = IconList.SelectedIndex;
            if (index > 0)
            {
                model.CharacterSelect.FighterIcons.Move(index, index - 1);
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
            model.CharacterSelect != null)
        {
            int index = IconList.SelectedIndex;
            if (index != -1 &&
                index + 1 < model.CharacterSelect.FighterIcons.Count)
            {
                model.CharacterSelect.FighterIcons.Move(index, index + 1);
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RefreshIcons_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SelectScreen.RefreshImageCache();
        SelectScreen.InvalidateVisual();
    }
}