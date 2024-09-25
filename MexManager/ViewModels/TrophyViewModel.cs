using Avalonia.Interactivity;
using mexLib.Types;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace MexManager.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private object? _selectedTrophy;
        public object? SelectedTrophy
        {
            get => _selectedTrophy;
            set => this.RaiseAndSetIfChanged(ref _selectedTrophy, value);
        }

        private ObservableCollection<MexTrophy>? _trophies;
        public ObservableCollection<MexTrophy>? Trophies
        {
            get => _trophies;
            set => this.RaiseAndSetIfChanged(ref _trophies, value);
        }
    }
}
