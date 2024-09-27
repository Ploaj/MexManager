using Avalonia.Collections;
using Avalonia.Interactivity;
using HSDRaw.AirRide.Vc;
using mexLib.Types;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
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

        private ObservableCollection<MexTrophy> _filtered = new ObservableCollection<MexTrophy>();
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
                ApplyFilter();
            }
        }

        private ObservableCollection<MexTrophy> _series = new ObservableCollection<MexTrophy>();
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
            foreach (var s in _normal.OrderBy(e => e.SortSeries))
            {
                SeriesOrder.Add(s);
            }
        }
        private void ApplyFilter()
        {
            if (Trophies == null)
                return;

            var selected = SelectedTrophy;
            FilteredTrophies.Clear();
            foreach (var c in Trophies)
            {
                if (string.IsNullOrEmpty(Filter) ||
                    CheckFilter(c.Data.Text) ||
                    CheckFilter(c.USData.Text))
                {
                    FilteredTrophies.Add(c);
                }
            }
            SelectedTrophy = selected;
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
    }

    public class CollectionSynchronizer<T> : IDisposable
    {
        private ObservableCollection<T> _collectionA;
        private ObservableCollection<T> _collectionB;
        private bool _isSyncing = false; // To prevent infinite recursion

        public CollectionSynchronizer(ObservableCollection<T> collectionA, ObservableCollection<T> collectionB)
        {
            _collectionA = collectionA;
            _collectionB = collectionB;

            // Subscribe to CollectionChanged events
            _collectionA.CollectionChanged += CollectionA_CollectionChanged;
            _collectionB.CollectionChanged += CollectionB_CollectionChanged;
        }

        public void Dispose()
        {
            _collectionA.CollectionChanged -= CollectionA_CollectionChanged;
            _collectionB.CollectionChanged -= CollectionB_CollectionChanged;
        }

        private void CollectionA_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isSyncing) return; // Prevent recursive syncing

            try
            {
                _isSyncing = true;

                // Sync changes from Collection A to Collection B
                SyncCollections(_collectionA, _collectionB, e);
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void CollectionB_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isSyncing) return; // Prevent recursive syncing

            try
            {
                _isSyncing = true;

                // Sync changes from Collection B to Collection A
                SyncCollections(_collectionB, _collectionA, e);
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void SyncCollections(ObservableCollection<T> source, ObservableCollection<T> target, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                        foreach (T newItem in e.NewItems)
                        {
                            // Add new items to the target collection if they don't already exist
                            if (!target.Contains(newItem))
                            {
                                target.Add(newItem);
                            }
                        }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                        foreach (T oldItem in e.OldItems)
                        {
                            // Remove items from the target collection
                            target.Remove(oldItem);
                        }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null && e.NewItems != null)
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            T? oldItem = (T?)e.OldItems[i];
                            T? newItem = (T?)e.NewItems[i];

                            if (oldItem == null || newItem == null)
                                continue;

                            // Replace items in the target collection
                            int index = target.IndexOf(oldItem);
                            if (index >= 0)
                            {
                                target[index] = newItem;
                            }
                        }
                    break;

                case NotifyCollectionChangedAction.Move:
                    // Handle move by reflecting the same move in the target collection
                    //if (e.OldStartingIndex != e.NewStartingIndex)
                    //{
                    //    T movedItem = source[e.NewStartingIndex];
                    //    int oldIndexInTarget = target.IndexOf(movedItem);
                    //    if (oldIndexInTarget >= 0)
                    //    {
                    //        // Remove the item and insert it at the new index
                    //        target.Move(oldIndexInTarget, e.NewStartingIndex);
                    //    }
                    //}
                    break;

                case NotifyCollectionChangedAction.Reset:
                    // Clear the target collection if the source was cleared
                    target.Clear();
                    foreach (var item in source)
                    {
                        target.Add(item);
                    }
                    break;
            }
        }
    }
}
