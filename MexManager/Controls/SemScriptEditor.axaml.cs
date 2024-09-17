using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using mexLib.Types;
using mexLib;
using mexLib.Utilties;
using MexManager.Tools;
using MexManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using MexManager.Extensions;

namespace MexManager.Controls;

// TODO: icon converter
public class SemCommandIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BitmapManager.MexFighterImage;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class SemLoopOffsetConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return null;

        if (values[0] == null || values[1] == null)
            return null;

        if (values[1] is not ListBox list)
            return null;

        int offset = 0;
        int offset_size = 24;

        foreach (SemCommand? command in list.Items)
        {
            if (command == null)
                continue;

            if (command.SemCode == SemCode.EndLoop)
                offset -= offset_size;

            if (command == values[0])
                break;

            if (command.SemCode == SemCode.SetLoop)
                offset += offset_size;
        }

        return new GridLength(offset);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class SemScriptEditor : UserControl
{
    /// <summary>
    /// 
    /// </summary>
    public SemScriptEditor()
    {
        InitializeComponent();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddCommand_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SemScript script)
        {
            var selected_index = ScriptCommandList.SelectedIndex;
            if (selected_index != -1)
            {
                script.Script.Insert(selected_index + 1, new SemCommand(SemCode.Wait, 0));
                ScriptCommandList.SelectedIndex = selected_index + 1;
            }
            else
            {
                script.Script.Add(new SemCommand(SemCode.Wait, 0));
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddLoopCommand_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SemScript script)
        {
            var selected_index = ScriptCommandList.SelectedIndex;
            if (selected_index != -1)
            {
                script.Script.Insert(selected_index + 1, new SemCommand(SemCode.EndLoop, 0));
                script.Script.Insert(selected_index + 1, new SemCommand(SemCode.SetLoop, 0));
                ScriptCommandList.SelectedIndex = selected_index + 1;
            }
            else
            {
                script.Script.Add(new SemCommand(SemCode.SetLoop, 0));
                script.Script.Add(new SemCommand(SemCode.EndLoop, 0));
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RemoveCommand_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SemScript script)
        {
            int index = ScriptCommandList.SelectedIndex;

            // Ensure the item isn't the last one in the collection
            if (index >= 0 && index < script.Script.Count)
            {
                // can't remove end loop
                if (script.Script[index].SemCode == SemCode.EndLoop)
                    return;

                // remove end loop bracket
                if (script.Script[index].SemCode == SemCode.SetLoop)
                {
                    for (int i = index + 1; i < script.Script.Count; i++)
                    {
                        if (script.Script[i].SemCode == SemCode.EndLoop)
                        {
                            script.Script.RemoveAt(i);
                            break;
                        }
                    }
                    ScriptCommandList.RefreshList();
                }

                // remove selected item
                script.Script.RemoveAt(index);

                // select new item
                ScriptCommandList.SelectedIndex = index;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DuplicateCommand_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SemScript script)
        {
            var selected_index = ScriptCommandList.SelectedIndex;
            if (selected_index != -1)
            {
                var target = script.Script[selected_index];

                if (target.SemCode == SemCode.SetLoop || target.SemCode == SemCode.EndLoop)
                    return;

                script.Script.Insert(selected_index + 1, new SemCommand(target));
                ScriptCommandList.SelectedIndex = selected_index + 1;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveUpCommand_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SemScript script)
        {
            int index = ScriptCommandList.SelectedIndex;

            // Ensure the item isn't the last one in the collection
            if (index > 0)
            {
                // sanity check for loops
                if (script.Script[index].SemCode == SemCode.EndLoop &&
                    script.Script[index - 1].SemCode == SemCode.SetLoop)
                    return;

                // Swap the item with the one below it
                (script.Script[index - 1], script.Script[index]) = (script.Script[index], script.Script[index - 1]);

                ScriptCommandList.SelectedIndex = index - 1;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveDownCommand_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SemScript script)
        {
            int index = ScriptCommandList.SelectedIndex;

            // Ensure the item isn't the last one in the collection
            if (index < script.Script.Count - 1)
            {
                // sanity check for loops
                if (script.Script[index].SemCode == SemCode.SetLoop &&
                    script.Script[index + 1].SemCode == SemCode.EndLoop)
                    return;

                // Swap the item with the one below it
                (script.Script[index + 1], script.Script[index]) = (script.Script[index], script.Script[index + 1]);

                ScriptCommandList.SelectedIndex = index + 1;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        // TODO: ensure loop end sanity
    }
}