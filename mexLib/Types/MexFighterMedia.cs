using mexLib.Attributes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace mexLib.Types
{
    public partial class MexFighter
    {
        [Browsable(false)]
        public FighterMedia Media { get; set; } = new FighterMedia();
        public class FighterMedia : INotifyPropertyChanged
        {

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private string _endClassicFile = "";
            private string _endAdventureFile = "";
            private string _endAllStarFile = "";
            private string _endMovieFile = "";

            [Category("Classic Mode"), DisplayName(" "), Description("")]
            [MexMedia]
            [MexFilePathValidator(MexFilePathType.Files)]
            public string EndClassicFile
            {
                get => _endClassicFile;
                set
                {
                    if (_endClassicFile != value)
                    {
                        _endClassicFile = value;
                        OnPropertyChanged();
                    }
                }
            }

            [Category("Adventure Mode"), DisplayName(" "), Description("")]
            [MexMedia]
            [MexFilePathValidator(MexFilePathType.Files)]
            public string EndAdventureFile
            {
                get => _endAdventureFile;
                set
                {
                    if (_endAdventureFile != value)
                    {
                        _endAdventureFile = value;
                        OnPropertyChanged();
                    }
                }
            }

            [Category("All Star Mode"), DisplayName(" "), Description("")]
            [MexMedia]
            [MexFilePathValidator(MexFilePathType.Files)]
            public string EndAllStarFile
            {
                get => _endAllStarFile;
                set
                {
                    if (_endAllStarFile != value)
                    {
                        _endAllStarFile = value;
                        OnPropertyChanged();
                    }
                }
            }

            [Category("Ending Movie"), DisplayName(" "), Description("")]
            [MexFilePathValidator(MexFilePathType.Files)]
            public string EndMovieFile
            {
                get => _endMovieFile;
                set
                {
                    if (_endMovieFile != value)
                    {
                        _endMovieFile = value;
                        OnPropertyChanged();
                    }
                }
            }
        }
    }
}
