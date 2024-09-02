using MeleeMedia.Audio;
using MexManager.Tools;
using ReactiveUI;
using System;
using System.Threading;
using System.Windows.Input;

namespace MexManager.ViewModels
{
    public class AudioPlayerModel : ViewModelBase
    {
        private readonly AudioPlayer? _soundPlayer;
        public ICommand PlaySoundCommand { get; }
        public ICommand StopSoundCommand { get; }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
        }

        private string? _currentTime;
        public string? CurrentTime
        {
            get => _currentTime;
            set => this.RaiseAndSetIfChanged(ref _currentTime, value);
        }

        private string? _startTime;
        public string? StartTime
        {
            get => _startTime;
            set => this.RaiseAndSetIfChanged(ref _startTime, value);
        }

        private string? _endTime;
        public string? EndTime
        {
            get => _endTime;
            set => this.RaiseAndSetIfChanged(ref _endTime, value);
        }

        private double _playbackProgress;
        public double PlaybackProgress
        {
            get => _playbackProgress;
            set => this.RaiseAndSetIfChanged(ref _playbackProgress, value);
        }

        private int _offsetX;
        public int OffsetX
        {
            get => _offsetX;
            set => this.RaiseAndSetIfChanged(ref _offsetX, value);
        }

        public Avalonia.Point StartPoint => new (OffsetX, 0);
        public Avalonia.Point EndPoint => new (OffsetX, 20);

        private readonly float Width = 100;

        private bool SkipUpdate;
        private float _playPercent;
        public float ProgressWidth
        {
            get => _playPercent;
            set
            {
                this.RaiseAndSetIfChanged(ref _playPercent, value);
            }
        }

        private readonly Timer? _updateTimer;

        /// <summary>
        /// 
        /// </summary>
        public AudioPlayerModel()
        {
            PlaySoundCommand = new RelayCommand(PlayButton_Click, AudioIsLoaded);
            StopSoundCommand = new RelayCommand(StopButton_Click, AudioIsLoaded);

            _soundPlayer?.Dispose();
            _soundPlayer = new AudioPlayer();

            _updateTimer = new Timer((o) =>
            {
                if (_soundPlayer?.State == OpenTK.Audio.OpenAL.ALSourceState.Playing)
                {
                    float percent = _soundPlayer.Percentage;
                    //var e = _soundPlayer?.TotalLength;
                    //if (e != null)
                    //{
                    //    // Calculate the offset TimeSpan based on the ratio
                    //    double offsetSeconds = e.Value.TotalSeconds * percent;

                    //    var c = TimeSpan.FromSeconds(offsetSeconds);

                    //    EndTime = $"{c.Minutes}:{c.Seconds} / {e.Value.Minutes}:{e.Value.Seconds}";
                    //}
                    SkipUpdate = true;
                    ProgressWidth = percent * Width;
                }
            }, null, 0, 20); // Check every 20ms
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="waveData"></param>
        /// <param name="loopPoint"></param>
        public void LoadDSP(DSP dsp)
        {
            _soundPlayer?.LoadDSP(dsp);
            if (_soundPlayer != null)
            {
                OffsetX = (int)(_soundPlayer.Percentage * Width);
            }
            var l = _soundPlayer?.LoopPoint;
            if (l != null)
                StartTime = $"Loop Point: {l.Value.Minutes}:{l.Value.Seconds}";
        }
        /// <summary>
        /// 
        /// </summary>
        public void PlaySound()
        {
            _soundPlayer?.Play();
            IsPlaying = _soundPlayer?.State == OpenTK.Audio.OpenAL.ALSourceState.Playing;
        }
        /// <summary>
        /// 
        /// </summary>
        public void StopSound()
        {
            _soundPlayer?.Stop();
            ProgressWidth = 0;
            IsPlaying = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentage"></param>
        public void SeekPercentage(double percentage)
        {
            if (SkipUpdate)
            {
                SkipUpdate = false;
            }
            else
            {
                var playing = _soundPlayer?.State == OpenTK.Audio.OpenAL.ALSourceState.Playing;
                _soundPlayer?.SeekPercentage(percentage);

                if (playing)
                {
                    PlaySound();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public void PlayButton_Click(object? obj)
        {
            PlaySound();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public void StopButton_Click(object? obj)
        {
            StopSound();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public bool AudioIsLoaded(object? obj)
        {
            return _soundPlayer != null;
        }
    }
}
