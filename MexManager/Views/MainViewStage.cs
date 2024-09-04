using Avalonia.Controls;
using Avalonia.Interactivity;
using mexLib;
using MexManager.Extensions;
using MexManager.ViewModels;

namespace MexManager.Views
{
    public partial class MainView : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void StageAddMenuItem_Click(object? sender, RoutedEventArgs args)
        {
            if (DataContext is MainViewModel model && 
                model.Stages != null)
            {
                var stage = new MexStage()
                {
                    Playlist = new MexPlaylist()
                    {
                        Entries = new System.Collections.ObjectModel.ObservableCollection<MexPlaylistEntry>()
                        {
                            new MexPlaylistEntry()
                            {
                                MusicID = 0,
                                ChanceToPlay = 50,
                            }
                        }
                    }
                };
                model.Stages.Add(stage);
                StagesList.RefreshList();
                StagesList.SelectedItem = stage;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void StageRemoveMenuItem_Click(object? sender, RoutedEventArgs args)
        {
            if (Global.Workspace != null &&
                DataContext is MainViewModel model &&
                StagesList.SelectedIndex != -1 &&
                model.SelectedStage is MexStage stage)
            {
                var res =
                    await MessageBox.Show(
                        $"Are you sure you want to\nremove \"{stage.Name}\"?",
                        "Remove Stage",
                        MessageBox.MessageBoxButtons.YesNoCancel);

                if (res != MessageBox.MessageBoxResult.Yes)
                    return;

                var sel = StagesList.SelectedIndex;
                if (Global.Workspace.Project.RemoveStage(StagesList.SelectedIndex))
                {
                    StagesList.RefreshList();
                    StagesList.SelectedIndex = sel;
                }
                else
                {
                    await MessageBox.Show(
                        $"Failed to remove stage \"{stage.Name}\"\nBase game stages cannot be removed", 
                        "Remove Stage Failed",
                        MessageBox.MessageBoxButtons.Ok);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void StageImportMenuItem_Click(object? sender, RoutedEventArgs args)
        {
            // TODO: stage import
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void StageExportMenuItem_Click(object? sender, RoutedEventArgs args)
        {
            // TODO: stage export
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void StageAddItemMenuItem_Click(object? sender, RoutedEventArgs args)
        {
            if (DataContext is MainViewModel model && 
                model.SelectedStage is MexStage stage)
            {
                stage.Items.Add(new MexItem());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void StageRemoveItemMenuItem_Click(object? sender, RoutedEventArgs args)
        {
            if (DataContext is MainViewModel model &&
                model.SelectedStage is MexStage stage && 
                model.SelectedStageItem is MexItem item)
            {
                var res = await MessageBox.Show(
                    $"Are you sure you want\nto remove\"{item.Name}\"?", 
                    "Remove Item",
                    MessageBox.MessageBoxButtons.YesNoCancel);

                if (res == MessageBox.MessageBoxResult.Yes)
                {
                    var selected = StageItemList.SelectedIndex;
                    stage.Items.Remove(item);
                    StageItemList.SelectedIndex = selected;
                }
            }
        }
    }
}
