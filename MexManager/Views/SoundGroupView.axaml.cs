using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MeleeMedia.Audio;
using mexLib;
using mexLib.Types;
using mexLib.Utilties;
using MexManager.Extensions;
using MexManager.Tools;
using MexManager.ViewModels;
using PropertyModels.ComponentModel;
using System.ComponentModel;
using System.IO;
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
            model.SelectedScript is SemScript script &&
            model.SelectedSoundGroup is MexSoundGroup group &&
            group.Sounds != null)
        {
            int soundId = script.GetFirstSoundID();
            if (soundId >= 0 &&
                soundId < group.Sounds.Count)
            {
                MexSound sound = group.Sounds[soundId];
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
            string? file = await FileIO.TryOpenFile("Import Sound", "", FileIO.FilterMusic);

            if (file == null)
                return;

            DSP dsp = new();
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
            string? file = await FileIO.TrySaveFile("Export Sound", sound.Name + ".wav", FileIO.FilterWav);

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
            AudioLoopEditor popup = new();
            popup.SetAudio(sound.DSP);
            if (App.MainWindow != null)
            {
                await popup.ShowDialog(App.MainWindow);

                if (popup.Result == AudioLoopEditor.AudioEditorResult.SaveChanges)
                {
                    DSP? newdsp = popup.ApplyChanges();

                    if (newdsp != null)
                    {
                        sound.DSP = newdsp;
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
            MessageBox.MessageBoxResult check = await MessageBox.Show($"Are you sure you want\nto remove \"{sound.Name}\"?", "Remove Sound", MessageBox.MessageBoxButtons.YesNoCancel);

            if (check != MessageBox.MessageBoxResult.Yes)
                return;

            int index = group.Sounds.IndexOf(sound);

            if (index == -1)
                return;

            // remove sound
            group.Sounds.RemoveAt(index);

            // adjust scripts
            foreach (SemScript script in group.Scripts)
            {
                script.RemoveSoundID(index);
            }

            // re select index
            System.Collections.IEnumerable source = SoundList.ItemsSource;
            SoundList.ItemsSource = null;
            SoundList.ItemsSource = source;
            if (index >= 0 && index < group.Sounds.Count)
            {
                SoundList.SelectedItem = group.Sounds[index];
                SoundList.ScrollIntoView(group.Sounds[index], null);
            }
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
            SemScript newScript = new()
            {
                Name = "SFX_Untitled",
                Script =
                {
                    new SemCommand(SemCode.Sound, 0),
                    new SemCommand(SemCode.SetReverb, 1),
                    new SemCommand(SemCode.SetPriority, 15),
                    new SemCommand(SemCode.SetVolume, 255),
                    new SemCommand(SemCode.End, 0),
                }
            };
            group.Scripts.Add(newScript);
            ScriptList.SelectedItem = newScript;
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
            model.SelectedScript is SemScript script)
        {
            MessageBox.MessageBoxResult check = await MessageBox.Show($"Are you sure you want\nto remove \"{script.Name}\"?", "Remove Script", MessageBox.MessageBoxButtons.YesNoCancel);

            if (check != MessageBox.MessageBoxResult.Yes)
                return;

            int index = group.Scripts.IndexOf(script);

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
            model.SelectedScript is SemScript script)
        {
            int index = group.Scripts.IndexOf(script);

            group.Scripts.Insert(index + 1, new SemScript(script));

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
                ScriptList.RefreshList(index - 1);
            }
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
                ScriptList.RefreshList(index + 1);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="replace"></param>
    private async void ImportSSM(bool replace)
    {
        if (Global.Workspace != null &&
            DataContext is SoundGroupModel model &&
            model.SelectedSoundGroup is MexSoundGroup group)
        {
            string? file = await FileIO.TryOpenFile("Import SSM", "",
            [
                new FilePickerFileType("SSM")
                {
                    Patterns = [ "*.ssm", ],
                }
            ]);

            if (file == null)
                return;

            group.ImportSSM(Global.Workspace, file, replace);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImportSSM_Click(object? sender, RoutedEventArgs e)
    {
        ImportSSM(false);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ReplaceSSM_Click(object? sender, RoutedEventArgs e)
    {
        ImportSSM(true);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ExportSSM_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is SoundGroupModel model &&
            model.SelectedSoundGroup is MexSoundGroup group)
        {
            string? file = await FileIO.TrySaveFile("Export SSM", group.FileName,
            [
                new FilePickerFileType("SSM")
                {
                    Patterns = [ "*.ssm", ],
                }
            ]);

            if (file == null)
                return;

            SSM ssm = new()
            {
                Name = group.Name,
                Sounds = group.Sounds.Select(e => e.DSP).ToArray(),
            };
            ssm.Save(file, out int bufferSize);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class AddGroupOptions : ReactiveObject
    {
        public enum SoundGroupPresets
        {
            Fighter,
            Stage,
            Scene,
            Constant,
        }

        [Category("Options")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                File = FileIO.SanitizeFilename($"{_name.ToLower()}.ssm");
                RaisePropertyChanged(nameof(File));
            }
        }
        private string _name = "New";

        [Category("Options")]
        [DisplayName("File")]
        [Browsable(false)]
        public string File { get; internal set; } = "new.ssm";

        [Category("Options")]
        [DisplayName("Type")]
        public SoundGroupPresets TypePresets { get; set; } = SoundGroupPresets.Fighter;

        public MexSoundGroup? CreateSoundGroup()
        {
            if (Global.Workspace == null)
                return null;

            // get unique path
            string uniquePath = Global.Workspace.FileManager.GetUniqueFilePath(Global.Workspace.GetFilePath($"audio\\us\\{File}"));

            // generate group
            MexSoundGroup group = new()
            {
                Name = Name,
                FileName = Path.GetFileName(uniquePath),
                Scripts = [],
            };

            // add new ssm to workspace
            Global.Workspace.FileManager.Set(uniquePath, []);

            // 
            switch (TypePresets)
            {
                case SoundGroupPresets.Fighter:
                    group.Group = MexSoundGroupGroup.Fighter;
                    group.Type = MexSoundGroupType.Melee;
                    group.SubType = MexSoundGroupSubType.Fighter;
                    break;
                case SoundGroupPresets.Stage:
                    group.Group = MexSoundGroupGroup.Stage;
                    group.Type = MexSoundGroupType.Melee;
                    group.SubType = MexSoundGroupSubType.Stage;
                    break;
                case SoundGroupPresets.Scene:
                    group.Group = MexSoundGroupGroup.Menu;
                    group.Type = MexSoundGroupType.Narrator;
                    group.SubType = MexSoundGroupSubType.Narrator;
                    break;
                case SoundGroupPresets.Constant:
                    group.Group = MexSoundGroupGroup.Constant;
                    group.Type = MexSoundGroupType.Constant;
                    group.SubType = MexSoundGroupSubType.Constant;
                    break;
            }

            return group;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddGroup_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null)
        {
            PropertyGridPopup popup = new();
            AddGroupOptions options = new();

            popup.SetObject("Create SoundBank", "Confirm", options);

            if (App.MainWindow != null)
            {
                await popup.ShowDialog(App.MainWindow);

                if (popup.Confirmed)
                {
                    MexSoundGroup? group = options.CreateSoundGroup();

                    if (group != null)
                        Global.Workspace.Project.AddSoundGroup(group);

                    GroupList.SelectedItem = group;
                }
            }
        }
    }
    public class CopyGroupOptions : ReactiveObject
    {
        [Category("Options")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                File = FileIO.SanitizeFilename($"{_name.ToLower()}.ssm");
                RaisePropertyChanged(nameof(File));
            }
        }
        private string _name = "New";

        [Category("Options")]
        [DisplayName("File")]
        [Browsable(false)]
        public string File { get; internal set; } = "new.ssm";

        public MexSoundGroup? CreateSoundGroup(MexSoundGroup sourceGroup)
        {
            if (Global.Workspace == null)
                return null;

            // get unique path
            string uniquePath = Global.Workspace.FileManager.GetUniqueFilePath(Global.Workspace.GetFilePath($"audio\\us\\{File}"));

            // generate group
            MexSoundGroup group = new()
            {
                Name = Name,
                FileName = Path.GetFileName(uniquePath),
                Scripts = [],
            };

            // copy sounds and scripts
            group.CopyFrom(sourceGroup);

            // add new ssm to workspace
            Global.Workspace.FileManager.Set(uniquePath, []);

            return group;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CopyGroup_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            GroupList.SelectedItem is MexSoundGroup source)
        {
            PropertyGridPopup popup = new();
            CopyGroupOptions options = new()
            {
                Name = source.Name + "_copy",
            };
            popup.SetObject("Duplicate SoundBank", "Confirm", options);

            if (App.MainWindow != null)
            {
                await popup.ShowDialog(App.MainWindow);

                if (popup.Confirmed)
                {
                    MexSoundGroup? group = options.CreateSoundGroup(source);

                    if (group != null)
                        Global.Workspace.Project.AddSoundGroup(group);

                    GroupList.SelectedItem = group;
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveGroup_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is SoundGroupModel model &&
            model.SelectedSoundGroup is MexSoundGroup group)
        {
            if (!MexSoundGroupIDConverter.IsMexSoundGroup(GroupList.SelectedIndex))
            {
                await MessageBox.Show(
                    $"Base game sounds cannot be removed",
                    $"Failed to remove group \"{group.Name}\"",
                    MessageBox.MessageBoxButtons.Ok);
                return;
            }

            MessageBox.MessageBoxResult res =
                await MessageBox.Show(
                    $"Are you sure you want to\nremove \"{group.Name}\"?",
                    "Remove Sound Group",
                    MessageBox.MessageBoxButtons.YesNoCancel);

            if (res != MessageBox.MessageBoxResult.Yes)
                return;

            int selected = GroupList.SelectedIndex;
            if (Global.Workspace.Project.RemoveSoundGroup(group))
            {
                res = await MessageBox.Show(
                    $"Would you like to delete\n\"{group.FileName}\" as well?",
                    "Delete File",
                    MessageBox.MessageBoxButtons.YesNoCancel);
                if (res == MessageBox.MessageBoxResult.Yes)
                    Global.Workspace.FileManager.Remove(Global.Workspace.GetFilePath($"audio\\us\\{group.FileName}"));

                GroupList.RefreshList();
                GroupList.SelectedIndex = selected;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ImportGroup_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null)
        {
            string? file = await FileIO.TryOpenFile("Import Sound Group", "", FileIO.FilterZip);

            if (file == null)
                return;

            using FileStream fs = new(file, FileMode.Open);
            mexLib.Installer.MexInstallerError? res = MexSoundGroup.FromPackage(Global.Workspace, fs, out MexSoundGroup? group);
            if (res == null)
            {
                if (group != null)
                {
                    Global.Workspace.Project.AddSoundGroup(group);
                    GroupList.SelectedItem = group;
                }
            }
            else
            {
                await MessageBox.Show(res.Message, "Import Sound Failed", MessageBox.MessageBoxButtons.Ok);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ExportGroup_Click(object? sender, RoutedEventArgs e)
    {
        if (Global.Workspace != null &&
            DataContext is SoundGroupModel model &&
            model.SelectedSoundGroup is MexSoundGroup group)
        {
            string? file = await FileIO.TrySaveFile("Export Sound Group", group.Name, FileIO.FilterZip);

            if (file == null)
                return;

            using FileStream fs = new(file, FileMode.Create);
            MexSoundGroup.ToPackage(group, fs);
        }
    }
}