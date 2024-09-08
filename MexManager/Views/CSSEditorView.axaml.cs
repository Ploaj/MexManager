using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mexLib;
using mexLib.Types;
using MexManager.ViewModels;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;

namespace MexManager.Views;

public partial class CSSEditorView : UserControl
{

    public class CSSTemplate
    {
        public float TotalWidth { get; set; } = 70.1f;

        public int IconsPerRow { get; set; } = 9;

        public float Scale { get; set; } = 1.0f;

        public float CenterX { get; set; } = 0.05f;
        public float CenterY { get; set; } = 9.5f;

        public float IconWidth { get; set; } = 7.05f;
        public float IconHeight { get; set; } = 7.2f;

        public float IconSideDropX { get; set; } = 0;
        public float IconSideDropY { get; set; } = -0.3f;
        public float IconSideDropZ { get; set; } = -1;

        public void Apply(ObservableCollection<MexCharacterSelectIcon> icons)
        {
            int icon_count = icons.Count;

            int num_of_rows = (int)Math.Ceiling(icons.Count / (double)IconsPerRow);

            float icon_height = IconHeight * Scale;
            float icon_width = IconWidth * Scale;

            float total_height = (num_of_rows) * icon_height;
            float total_width = IconsPerRow * icon_width;

            for (int i = 0; i < icons.Count; i++)
            {
                int col = i % IconsPerRow;
                int row = i / IconsPerRow;

                int lastRow = IconsPerRow - 1;

                if (row >= num_of_rows - 1 && (icons.Count % IconsPerRow) > 0)
                {
                    lastRow = (icons.Count % IconsPerRow) - 1;
                    total_width = (icons.Count % IconsPerRow) * icon_width;
                }

                icons[i].X = CenterX - total_width / 2 + icon_width * col + icon_width / 2;
                icons[i].Y = CenterY + total_height / 2 - icon_height * row - icon_height / 2;
                icons[i].Z = 0;
                icons[i].ScaleX = Scale;
                icons[i].ScaleY = Scale;

                if (col == lastRow || col == 0)
                {
                    icons[i].X += IconSideDropX;
                    icons[i].Y += IconSideDropY;
                    icons[i].Z += IconSideDropZ;
                }

                Debug.WriteLine($"{i} {col} {row} {total_width}");
            }

            // TODO: calculate collisions
        }
    }

    private CSSTemplate template = new CSSTemplate();

    public CSSEditorView()
    {
        InitializeComponent();

        TemplateGrid.DataContext = template;
    }

    public void Button_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.CSSIcons != null)
        {
            template.Apply(model.CSSIcons);
            SelectScreen.InvalidateVisual();
        }
    }

    public void UndoButton_Click(object? sender, RoutedEventArgs args)
    {
        SelectScreen.Undo();
    }

    public void RedoButton_Click(object? sender, RoutedEventArgs args)
    {
        SelectScreen.Redo();
    }
    public void AddIcon_Click(object? sender, RoutedEventArgs args)
    {
        if (Global.Workspace != null &&
            DataContext is MainViewModel model &&
            model.CSSIcons != null)
        {
            model.CSSIcons.Add(new MexCharacterSelectIcon()
            {
                
            });
        }
    }
}