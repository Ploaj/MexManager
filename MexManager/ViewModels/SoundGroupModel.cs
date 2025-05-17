using mexLib.Types;
using mexLib.Utilties;
using ReactiveUI;
using System.Collections.ObjectModel;

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

        private MexSoundGroup? _selectedSoundGroup;
        public MexSoundGroup? SelectedSoundGroup
        {
            get => _selectedSoundGroup;
            set => this.RaiseAndSetIfChanged(ref _selectedSoundGroup, value);
        }

        private MexSound? _selectedSound;
        public MexSound? SelectedSound
        {
            get => _selectedSound;
            set => this.RaiseAndSetIfChanged(ref _selectedSound, value);
        }

        private SemScript? _selectedScript;
        public SemScript? SelectedScript
        {
            get => _selectedScript;
            set => this.RaiseAndSetIfChanged(ref _selectedScript, value);
        }

        public int ScriptOffset
        {
            get
            {
                if (SoundGroups == null || SelectedSoundGroup == null)
                    return 0;

                return SoundGroups.IndexOf(SelectedSoundGroup) * 10000;
            }
        }
    }
}
