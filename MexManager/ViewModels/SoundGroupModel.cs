using mexLib.Types;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MexManager.ViewModels
{
    public class SoundGroupModel : ViewModelBase
    {
        private ObservableCollection<MexSoundGroup>? _soundGroups;
        public ObservableCollection<MexSoundGroup>? SoundGroups
        {
            get => _soundGroups;
            set => this.RaiseAndSetIfChanged(ref _soundGroups, value);
        }

        private object? _selectedSoundGroup;
        public object? SelectedSoundGroup
        {
            get => _selectedSoundGroup;
            set => this.RaiseAndSetIfChanged(ref _selectedSoundGroup, value);
        }

        private object? _selectedSound;
        public object? SelectedSound
        {
            get => _selectedSound;
            set => this.RaiseAndSetIfChanged(ref _selectedSound, value);
        }

        private object? _selectedScript;
        public object? SelectedScript
        {
            get => _selectedScript;
            set => this.RaiseAndSetIfChanged(ref _selectedScript, value);
        }

    }
}
