using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using MeleeMedia.Audio;
using mexLib.Types;
using MexManager.Tools;
using MexManager.ViewModels;
using System.Globalization;
using System;
using System.IO;
using System.Collections;

namespace MexManager.Views;

public partial class SoundGroupView : UserControl
{
    /// <summary>
    /// 
    /// </summary>
    public SoundGroupView()
    {
        InitializeComponent();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (DataContext is SoundGroupModel model &&
            model.SelectedSound is MexSound sound && 
            sound.DSP != null)
        {
            Global.PlaySound(sound.DSP);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ScriptList_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (DataContext is SoundGroupModel model &&
            model.SelectedScript is SEMBankScript script &&
            model.SelectedSoundGroup is MexSoundGroup group &&
            group.Sounds != null)
        {
            var soundId = script.SFXID;
            if (soundId >= 0 && 
                soundId < group.Sounds.Count)
            {
                var sound = group.Sounds[soundId];
                if (sound.DSP != null)
                    Global.PlaySound(sound.DSP);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PreviewSound_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SoundGroupModel model &&
            model.SelectedSound is MexSound sound &&
            sound.DSP != null)
        {
            Global.PlaySound(sound.DSP);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ImportSound_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SoundGroupModel model &&
            model.SelectedSoundGroup != null)
        {
            var file = await FileIO.TryOpenFile("Import Sound", "", FileIO.FilterMusic);

            if (file == null)
                return;

            var dsp = new DSP();
            if (dsp.FromFile(file))
            {
                model.SelectedSoundGroup.Sounds.Add(new MexSound()
                {
                    Name = System.IO.Path.GetFileNameWithoutExtension(file),
                    DSP = dsp,
                });
            }
            else
            {
                await MessageBox.Show($"Failed to import file\n{file}", "Import Music Error", MessageBox.MessageBoxButtons.Ok);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ExportSound_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SoundGroupModel model &&
            model.SelectedSound is MexSound sound &&
            sound.DSP != null)
        {
            var file = await FileIO.TrySaveFile("Export Sound", sound.Name + ".wav", FileIO.FilterWav);

            if (file != null)
            {
                File.WriteAllBytes(file, sound.DSP.ToWAVE().ToFile());
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditSound_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SoundGroupModel model &&
            model.SelectedSound is MexSound sound &&
            sound.DSP != null)
        {
            Global.StopMusic();

            // create editor popup
            var popup = new AudioLoopEditor();
            popup.SetAudio(sound.DSP);
            if (App.MainWindow != null)
            {
                await popup.ShowDialog(App.MainWindow);

                if (popup.Result == AudioLoopEditor.AudioEditorResult.SaveChanges)
                {
                    var newdsp = popup.ApplyChanges();

                    if (newdsp != null)
                    {
                        sound.DSP = newdsp;
                        SoundPropertyGrid.DataContext = null;
                        SoundPropertyGrid.DataContext = sound;
                    }
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveSound_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SoundGroupModel model &&
            model.SelectedSound is MexSound sound && 
            model.SelectedSoundGroup is MexSoundGroup group &&
            group.Scripts != null)
        {
            var check = await MessageBox.Show($"Are you sure you want\nto remove \"{sound.Name}\"?", "Remove Sound", MessageBox.MessageBoxButtons.YesNoCancel);

            if (check != MessageBox.MessageBoxResult.Yes)
                return;

            var index = group.Sounds.IndexOf(sound);

            if (index == -1)
                return;

            // remove sound
            group.Sounds.RemoveAt(index);

            // adjust scripts
            foreach (var script in group.Scripts)
            {
                if (script.SFXID == index)
                    script.SFXID = 0;
                else
                if (script.SFXID > index)
                    script.SFXID -= 1;
            }

            // re select index
            SoundList.SelectedIndex = index;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddScript_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: add script
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RemoveScript_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: remove script
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DuplicateScript_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: duplicate script
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveUpScript_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: move up script
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveDownScript_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: move down script
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddGroup_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: add sound group
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RemoveGroup_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: remove sound group
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImportGroup_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: import sound group
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ExportGroup_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: export sound group
    }
}