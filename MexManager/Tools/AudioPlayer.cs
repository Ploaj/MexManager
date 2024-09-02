using MeleeMedia.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace MexManager.Tools
{
    public class AudioPlayer : IDisposable
    {
        public ALSourceState State
        {
            get
            {
                AL.GetSource(_source, ALGetSourcei.SourceState, out int sourceState);
                return (ALSourceState)sourceState;
            }
        }

        public float Percentage
        {
            get
            {
                AL.GetSource(_source, ALGetSourcei.SampleOffset, out int state);
                return state / (float)_totalSize;
            }
        }

        public TimeSpan TotalLength { get; internal set; }

        public TimeSpan LoopPoint { get; internal set; }


        private bool _manualstop = false;

        private ALDevice _device;
        private ALContext _context;

        private int _buffer;
        private int _source;

        private int _loopPoint;
        private int _totalSize;

        private readonly Timer? _loopTimer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hps"></param>
        public AudioPlayer()
        {
            _device = ALC.OpenDevice(null);
            if (_device == ALDevice.Null)
            {
                System.Diagnostics.Debug.WriteLine("Audio Device failed");
            }

            _context = ALC.CreateContext(_device, new ALContextAttributes());
            if (_context == ALContext.Null)
            {
                System.Diagnostics.Debug.WriteLine("Audio Context failed");
            }

            ALC.MakeContextCurrent(_context);
            _buffer = AL.GenBuffer();
            _source = AL.GenSource();

            _loopTimer = new Timer(CheckPlayback, null, 0, 100); // Check every 100ms
        }
        /// <summary>
        /// 
        /// </summary>
        public void LoadDSP(byte[] dsp)
        {
            var d = new DSP();
            d.FromFormat(dsp, "dsp");
            LoadDSP(d);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hps"></param>
        public void LoadDSP(DSP dsp)
        {
            AL.SourceStop(_source);

            // regenerate sources
            AL.DeleteBuffer(_buffer);
            AL.DeleteSource(_source);
            _buffer = AL.GenBuffer();
            _source = AL.GenSource();


            var wave = dsp.ToWAVE();
            var raw = wave.RawData.ToArray();

            _loopPoint = (int)Math.Ceiling((double)dsp.Channels[0].LoopStart / 2f * 1.75f);
            _totalSize = raw.Length / 2;
            TotalLength = TimeSpan.Parse(dsp.Length);
            LoopPoint = TimeSpan.Parse(dsp.LoopPoint);

            // Pin the managed array so that the GC doesn't move it
            GCHandle handle = GCHandle.Alloc(raw, GCHandleType.Pinned);

            try
            {
                // Get a pointer to the pinned array
                IntPtr ptr = handle.AddrOfPinnedObject();

                // Use the IntPtr to pass the data to OpenAL
                var format = wave.Channels.Count == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
                AL.BufferData(_buffer, format, ptr, raw.Length * sizeof(short), wave.Frequency);
                AL.Source(_source, ALSourcei.Buffer, _buffer);
            }
            finally
            {
                // Free the pinned handle
                handle.Free();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Play()
        {
            _manualstop = false;
            switch (State)
            {
                case ALSourceState.Playing:
                    AL.SourcePause(_source);
                    break;
                default:
                    AL.SourcePlay(_source);
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void CheckPlayback(object? state)
        {
            if (!_manualstop &&
                State == ALSourceState.Stopped)
            {
                Stop();
                AL.Source(_source, ALSourcei.SampleOffset, _loopPoint);
                AL.SourcePlay(_source);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            _manualstop = true;
            AL.SourceStop(_source);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentage"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SeekPercentage(double percentage)
        {
            Stop();
            AL.Source(_source, ALSourcei.SampleOffset, (int)(_totalSize * percentage));
            AL.SourcePlay(_source);
            AL.SourcePause(_source);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            ALC.CloseDevice(_device);
            ALC.DestroyContext(_context);
            AL.DeleteSource(_source);
            AL.DeleteBuffer(_buffer);
            GC.SuppressFinalize(this);
        }
    }
}