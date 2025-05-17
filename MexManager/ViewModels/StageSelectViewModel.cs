using mexLib.Types;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MexManager.ViewModels
{
    public partial class MainViewModel
    {
        public ICommand AddStagePageCommand { get; }
        public ICommand DeleteStagePageCommand { get; }
        public ICommand MoveLeftStagePageCommand { get; }
        public ICommand MoveRightStagePageCommand { get; }


        private ObservableCollection<MexStageSelect>? _stagePages;
        public ObservableCollection<MexStageSelect>? StagePages
        {
            get => _stagePages;
            set => this.RaiseAndSetIfChanged(ref _stagePages, value);
        }

        private MexStageSelect? _stageSelect;
        public MexStageSelect? StageSelect
        {
            get => _stageSelect;
            set => this.RaiseAndSetIfChanged(ref _stageSelect, value);
        }

        private object? _selectedSSSIcon;
        public object? SelectedSSSIcon
        {
            get => _selectedSSSIcon;
            set => this.RaiseAndSetIfChanged(ref _selectedSSSIcon, value);
        }

        private object? _selectedSSSTemplateIcon;
        public object? SelectedSSSTemplateIcon
        {
            get => _selectedSSSTemplateIcon;
            set => this.RaiseAndSetIfChanged(ref _selectedSSSTemplateIcon, value);
        }

        private bool _autoApplySSSTemplate = true;
        public bool AutoApplySSSTemplate
        {
            get => _autoApplySSSTemplate;
            set
            {
                this.RaiseAndSetIfChanged(ref _autoApplySSSTemplate, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void AddStagePage(object? add)
        {
            if (Global.Workspace != null &&
                StagePages != null)
            {
                MexStageSelect ss = new();

                for (int i = 0; i < ss.Template.IconPlacements.Count; i++)
                {
                    ss.StageIcons.Add(new MexStageSelectIcon()
                    {
                        Status = MexStageSelectIcon.StageIconStatus.Locked,
                    });
                }
                ss.Template.ApplyTemplate(ss.StageIcons);

                StagePages.Add(ss);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        public async void DeleteStagePage(object? parameter)
        {
            if (parameter is MexStageSelect ss &&
                StagePages != null)
            {
                if (StagePages.Count > 1)
                {
                    MessageBox.MessageBoxResult res = await MessageBox.Show($"Are you sure you want\nto delete \"{ss.Name}\"?", "Delete Page", MessageBox.MessageBoxButtons.YesNoCancel);

                    if (res != MessageBox.MessageBoxResult.Yes)
                        return;

                    StagePages.Remove(ss);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        public void MoveStagePageRight(object? parameter)
        {
            if (parameter is MexStageSelect ss &&
                StagePages != null)
            {
                int index = StagePages.IndexOf(ss);
                if (index < StagePages.Count - 1)
                {
                    StagePages.Move(index, index + 1);
                    StageSelect = ss;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        public void MoveStagePageLeft(object? parameter)
        {
            if (parameter is MexStageSelect ss &&
                StagePages != null)
            {
                int index = StagePages.IndexOf(ss);
                if (index > 0)
                {
                    StagePages.Move(index, index - 1);
                    StageSelect = ss;
                }
            }
        }
    }
}
