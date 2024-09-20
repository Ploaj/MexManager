using HSDRaw.MEX.Sounds;
using mexLib.Attributes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace mexLib.Types
{
    public class MexPlaylistEntry : INotifyPropertyChanged
    {
        private int _musicID;

        public int MusicID
        {
            get => _musicID; set
            {
                if (value >= 0)
                {
                    _musicID = value;
                    OnPropertyChanged();
                }
            }
        }

        private byte _chanceToPlay;

        public byte ChanceToPlay { get => _chanceToPlay; set => _chanceToPlay = Math.Clamp(value, (byte)0, (byte)100); }


        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MexPlaylist
    {
        public ObservableCollection<MexPlaylistEntry> Entries { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void RemoveTrack(int id)
        {
            for (int i = Entries.Count - 1; i >= 0; i--)
            {
                if (Entries[i].MusicID == id)
                    Entries.RemoveAt(i);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mEX_Playlist"></param>
        internal void FromMexPlayList(MEX_Playlist mEX_Playlist)
        {
            Entries.Clear();

            if (mEX_Playlist.MenuPlaylist == null)
                return;

            foreach (var e in mEX_Playlist.MenuPlaylist.Array)
            {
                Entries.Add(new MexPlaylistEntry()
                {
                    MusicID = e.HPSID,
                    ChanceToPlay = (byte)e.ChanceToPlay,
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal MEX_Playlist ToMexPlaylist()
        {
            return new MEX_Playlist()
            {
                MenuPlayListCount = Entries.Count,
                MenuPlaylist = new HSDRaw.HSDArrayAccessor<MEX_PlaylistItem>()
                {
                    Array = Entries.Select(e => new MEX_PlaylistItem()
                    {
                        HPSID = (ushort)e.MusicID,
                        ChanceToPlay = e.ChanceToPlay
                    }).ToArray()
                }
            };
        }
    }
}
