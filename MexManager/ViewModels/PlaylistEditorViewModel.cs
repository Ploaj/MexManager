using mexLib.Types;
using PropertyModels.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MexManager.ViewModels
{
    public class PlaylistEditorViewModel : ReactiveObject
    {
        public ObservableCollection<MexPlaylistEntry> Entries { get; } = [];

        public ICommand AddEntryCommand { get; }
        public ICommand RemoveEntryCommand { get; }
        public ICommand MoveEntryUpCommand { get; }
        public ICommand MoveEntryDownCommand { get; }

        public PlaylistEditorViewModel()
        {
            AddEntryCommand = ReactiveCommand.Create(AddEntry);
            RemoveEntryCommand = ReactiveCommand.Create(RemoveEntry);
            MoveEntryUpCommand = ReactiveCommand.Create(MoveEntryUp);
            MoveEntryDownCommand = ReactiveCommand.Create(MoveEntryDown);

            
        }

        private void AddEntry()
        {
            var entry = new MexPlaylistEntry { MusicID = 0, ChanceToPlay = 50 };
            Entries.Add(entry);
            entry.MusicID = 20;
        }

        private void RemoveEntry(object entry)
        {
            if (entry is MexPlaylistEntry e)
                Entries.Remove(e);
        }

        private void MoveEntryUp(object entry)
        {
            if (entry is MexPlaylistEntry e)
            {
                int index = Entries.IndexOf(e);
                if (index > 0)
                {
                    Entries.Move(index, index - 1);
                }
            }
        }

        private void MoveEntryDown(object entry)
        {
            if (entry is MexPlaylistEntry e)
            {
                int index = Entries.IndexOf(e);
                if (index < Entries.Count - 1)
                {
                    Entries.Move(index, index + 1);
                }
            }
        }
    }
}
