using mexLib.Types;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using static mexLib.Types.MexTrophy;

namespace MexManager.ViewModels
{
    public class TrophyViewModel : ViewModelBase
    {
        private object? _selectedTrophy;
        public object? SelectedTrophy
        {
            get => _selectedTrophy;
            set => this.RaiseAndSetIfChanged(ref _selectedTrophy, value);
        }

        private ObservableCollection<MexTrophy>? _normal;
        public ObservableCollection<MexTrophy>? Trophies
        {
            get
            {
                return _normal;
            }
            set
            {
                if (_normal != null)
                    _normal.CollectionChanged -= FilterCollectionChanged;

                this.RaiseAndSetIfChanged(ref _normal, value);

                if (_normal != null)
                    _normal.CollectionChanged += FilterCollectionChanged;

                InitSeriesList();
                ApplyFilter();
            }
        }

        private ObservableCollection<MexTrophy> _filtered = [];
        public ObservableCollection<MexTrophy> FilteredTrophies
        {
            get
            {
                return _filtered;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _filtered, value);
            }
        }

        private string _filter = "";
        public string Filter
        {
            get => _filter;
            set
            {
                this.RaiseAndSetIfChanged(ref _filter, value);
                object? selected = SelectedTrophy;
                ApplyFilter();
                SelectedTrophy = null;
                SelectedTrophy = selected;
            }
        }

        private ObservableCollection<MexTrophy> _series = [];
        public ObservableCollection<MexTrophy> SeriesOrder
        {
            get => _series;
            internal set { this.RaiseAndSetIfChanged(ref _series, value); }
        }
        public TrophyViewModel()
        {
        }
        private void FilterCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ApplyFilter();
        }
        private void InitSeriesList()
        {
            if (_normal == null)
                return;

            _series.Clear();
            foreach (MexTrophy? s in _normal.OrderBy(e => e.SortSeries))
            {
                SeriesOrder.Add(s);
            }
        }
        private void ApplyFilter()
        {
            if (Trophies == null)
                return;

            FilteredTrophies.Clear();
            foreach (MexTrophy c in Trophies)
            {
                if (string.IsNullOrEmpty(Filter) ||
                    CheckFilter(c.Data.Text) ||
                    CheckFilter(c.USData.Text))
                {
                    FilteredTrophies.Add(c);
                }
            }
        }
        private bool CheckFilter(TrophyTextEntry text)
        {
            /*
             *  || 
                CheckFilter(text.Description) || 
                CheckFilter(text.Source1) || 
                CheckFilter(text.Source2)
             */
            if (CheckFilter(text.Name))
            {
                return true;
            }
            return false;
        }
        private bool CheckFilter(string text)
        {
            // Regex check for the pattern
            //bool regexMatch = Regex.IsMatch(text, Filter);

            // Contains check, ignoring case
            bool containsMatch = text.Contains(Filter, StringComparison.OrdinalIgnoreCase);

            // Return true if either condition is met
            return containsMatch; // regexMatch || 
        }
        /// <summary>
        /// 
        /// </summary>
        internal void UpdateSeriesOrder()
        {
            // update series sort indices
            for (int i = 0; i < SeriesOrder.Count; i++)
            {
                SeriesOrder[i].SortSeries = (short)i;
            }
        }
    }
}
