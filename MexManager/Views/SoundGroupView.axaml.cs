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
using MexManager.Extensions;
using System.Linq;

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
        if (DataContext is SoundGroupModel model &&
            model.SelectedSoundGroup is MexSoundGroup group &&
            group.Scripts != null)
        {
            // TODO: initial code
            group.Scripts.Add(new SEMBankScript()
            {
                Name = "SFX_Untitled",
                Codes = new System.Collections.Generic.List<SEMCode>()
                {
                    new SEMCode(SEM_CODE.SET_SFXID),
                    new SEMCode(SEM_CODE.END_PLAYBACK),
                }
            });
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveScript_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SoundGroupModel model &&
            model.SelectedSoundGroup is MexSoundGroup group &&
            group.Scripts != null && 
            model.SelectedScript is SEMBankScript script)
        {
            var check = await MessageBox.Show($"Are you sure you want\nto remove \"{script.Name}\"?", "Remove Script", MessageBox.MessageBoxButtons.YesNoCancel);

            if (check != MessageBox.MessageBoxResult.Yes)
                return;

            var index = group.Scripts.IndexOf(script);

            if (index == -1)
                return;

            // remove sound
            group.Scripts.RemoveAt(index);

            // re select index
            ScriptList.RefreshList(index);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DuplicateScript_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SoundGroupModel model &&
            model.SelectedSoundGroup is MexSoundGroup group &&
            group.Scripts != null &&
            model.SelectedScript is SEMBankScript script)
        {
            var index = group.Scripts.IndexOf(script);

            group.Scripts.Insert(index + 1, new SEMBankScript()
            {
                Name = script.Name + "_copy",
                Codes = script.Codes.Select(e => new SEMCode(e.Pack())).ToList()
            });

            ScriptList.RefreshList(index);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveUpScript_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SoundGroupModel model &&
            model.SelectedSoundGroup is MexSoundGroup group &&
            group.Scripts != null)
        {
            int index = ScriptList.SelectedIndex;
            if (index > 0)
            {
                group.Scripts.Move(index, index - 1);
                ScriptList.SelectedIndex = index - 1;
            }
            ScriptList.RefreshList(index - 1);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveDownScript_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SoundGroupModel model &&
            model.SelectedSoundGroup is MexSoundGroup group &&
            group.Scripts != null)
        {
            int index = ScriptList.SelectedIndex;
            if (index + 1 < group.Scripts.Count)
            {
                group.Scripts.Move(index, index + 1);
                ScriptList.SelectedIndex = index + 1;
            }
            ScriptList.RefreshList(index + 1);
        }
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