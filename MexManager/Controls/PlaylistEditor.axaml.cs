using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using mexLib;
using MexManager.Tools;
using MexManager.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MexManager.Controls;

public partial class PlaylistEditor : UserControl
{
    public static readonly DirectProperty<PlaylistEditor, MexPlaylist> PlaylistProperty =
        AvaloniaProperty.RegisterDirect<PlaylistEditor, MexPlaylist>(nameof(Playlist), o => o.Playlist, (o, v) => o.Playlist = v);

    private MexPlaylist _playlist;
    public MexPlaylist Playlist
    {
        get => _playlist;
        set
        {
            _playlist = value;
            SetPlaylist(value);
        }
    }

    private ObservableCollection<MexPlaylistEntry> Entries { get; set; } = [];

    public PlaylistEditor()
    {
        InitializeComponent();
    }

    private void SetPlaylist(MexPlaylist playlist)
    {
        if (playlist == null)
            return;

        Entries = playlist.Entries;

        PlaylistPanel.Children.Clear();
        foreach (var e in Entries)
            GenerateCell(PlaylistPanel, e);
    }

    private void GenerateCell(StackPanel root, MexPlaylistEntry entry)
    {
        var c = new DockPanel()
        {

        };

        var topPanel = new DockPanel();
        c.Children.Add(topPanel);
        DockPanel.SetDock(topPanel, Dock.Top);

        {
            var button = new Button()
            {
                Content = new Image() { Source = BitmapManager.Minus, Width = 24, Height = 24 }
            };
            ToolTip.SetTip(button, "Remove");

            button.Click += (s, a) =>
            {
                Entries.Remove(entry);
                root.Children.Remove(c);
            };

            topPanel.Children.Add(button);
        }
        {
            var button = new Button()
            {
                Content = new Image() { Source = BitmapManager.PlayIconImage, Width=24, Height=24 }
            };
            ToolTip.SetTip(button, "Play");

            button.Click += (s, a) =>
            {
                if (Global.Workspace != null)
                    Global.PlayMusic(Global.Workspace.Project.Music[entry.MusicID]);
            };

            topPanel.Children.Add(button);
        }
        {
            var button = new Button()
            {
                Content = new Image() { Source = BitmapManager.ArrowUp, Width = 24, Height = 24 }
            };
            ToolTip.SetTip(button, "Move Up");

            button.Click += (s, a) =>
            {
                var index = Entries.IndexOf(entry);
                var control_index = root.Children.IndexOf(c);
                if (index > 0 && control_index > 0)
                {
                    Entries.Move(index, index - 1);
                    root.Children.Move(control_index, control_index - 1);
                }
            };

            topPanel.Children.Add(button);
        }
        {
            var button = new Button()
            {
                Content = new Image() { Source = BitmapManager.ArrowDown, Width = 24, Height = 24 }
            };
            ToolTip.SetTip(button, "Move Down");

            button.Click += (s, a) =>
            {
                var index = Entries.IndexOf(entry);
                var control_index = root.Children.IndexOf(c);
                if (index < Entries.Count - 1 && control_index < root.Children.Count - 1)
                {
                    Entries.Move(index, index + 1);
                    root.Children.Move(control_index, control_index + 1);
                }
            };

            topPanel.Children.Add(button);
        }
        {
            var combobox = new ComboBox()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                ItemsSource = Global.Workspace?.Project.Music,
                SelectedIndex = entry.MusicID,
            };

            combobox.SelectionChanged += (s, a) =>
            {
                entry.MusicID = combobox.SelectedIndex;
            };
            topPanel.Children.Add(combobox);
        }
        {
            var slider = new Slider()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Minimum = 0,
                Maximum = 100,
                Value = entry.ChanceToPlay,
            };

            slider.ValueChanged += (s, w) =>
            {
                entry.ChanceToPlay = (byte)slider.Value;
            };
            c.Children.Add(slider);
            DockPanel.SetDock(slider, Dock.Top);
        }

        root.Children.Add(c);
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var entry = new MexPlaylistEntry() { MusicID = 5, ChanceToPlay = 50 };
        Entries.Add(entry);
        GenerateCell(PlaylistPanel, entry);
    }
}