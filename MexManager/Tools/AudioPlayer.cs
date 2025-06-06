﻿using MeleeMedia.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace MexManager.Tools
{
    public class AudioPlayer : IDisposable
    {
        public bool Initialize { get; internal set; }
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

        public double EndPercentage { get; set; } = 1.0;

        public bool EnableLoop { get; set; } = true;
        public double LoopPointPercent
        {
            set
            {
                _loopPoint = (int)(_totalSize * value);
            }
        }

        private int _totalSize;
        private int _loopPoint;

        private bool _manualstop = false;

        private static ALDevice? _device;
        private static ALContext? _context;

        private int _buffer;
        private int _source;

        private readonly Timer? _loopTimer;

        private bool _hasLoop = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hps"></param>
        public AudioPlayer()
        {
            if (_device == null)
            {
                Logger.WriteLine("Trying to open ALC device");
                _device = ALC.OpenDevice(null);
                if (_device == ALDevice.Null)
                {
                    AlcError error = ALC.GetError((ALDevice)_device);
                    Logger.WriteLine($"Audio Device failed: {error}");

                    Logger.WriteLine("List of devices:");
                    System.Collections.Generic.IEnumerable<string> devices = ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier);
                    foreach (string? device in devices)
                        Logger.WriteLine(device);
                    return;
                }
                else
                {
                    Logger.WriteLine("Success");
                }
            }

            if (_context == null && _device != null && _device != ALDevice.Null)
            {
                Logger.WriteLine("Trying to create ALC Context");
                _context = ALC.CreateContext((ALDevice)_device, new ALContextAttributes());
                if (_context == ALContext.Null)
                {
                    Logger.WriteLine("Audio Context failed");
                    return;
                }
                else
                {
                    Logger.WriteLine("Success");
                }
            }

            if (_context is ALContext con)
            {
                Logger.WriteLine("Trying make ALC context current");
                if (ALC.MakeContextCurrent(con))
                {
                    Logger.WriteLine("Success");
                    _loopTimer = new Timer(CheckPlayback, null, 0, 30); // Check every 100ms
                    Initialize = true;
                }
                else
                {
                    Logger.WriteLine("Failed");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void LoadDSP(byte[] dsp)
        {
            if (!Initialize)
                return;

            DSP d = new();
            d.FromFormat(dsp, "dsp");
            LoadDSP(d);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hps"></param>
        public void LoadDSP(DSP dsp)
        {
            if (!Initialize)
                return;

            _hasLoop = dsp.LoopSound;

            // regenerate sources
            AL.SourceStop(_source);
            AL.Source(_source, ALSourcei.Buffer, 0);
            AL.DeleteBuffers(1, ref _buffer);
            AL.DeleteSources(1, ref _source);
            _buffer = AL.GenBuffer();
            _source = AL.GenSource();
            Debug.WriteLine($"Audio: buffer {_buffer} source {_source}");

            //AL.Source(_source, ALSourcef.Pitch, 1 + 1000 / 1000f); // pitch
            // volume
            // padding
            // unknown
            // reverb

            WAVE wave = dsp.ToWAVE();
            short[] raw = wave.RawData.ToArray();

            _totalSize = raw.Length / dsp.Channels.Count;
            double loop = dsp.LoopPointMilliseconds / dsp.TotalMilliseconds;
            _loopPoint = (int)(loop * _totalSize); // (int)Math.Ceiling((double)dsp.Channels[0].LoopStart / dsp.Channels.Count * 1.75f);

            // Pin the managed array so that the GC doesn't move it
            GCHandle handle = GCHandle.Alloc(raw, GCHandleType.Pinned);

            try
            {
                // Get a pointer to the pinned array
                IntPtr ptr = handle.AddrOfPinnedObject();

                // Use the IntPtr to pass the data to OpenAL
                ALFormat format = wave.Channels.Count == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
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
            if (!Initialize)
                return;

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
            if (!Initialize)
                return;

            // trim end loop point
            if (Percentage >= EndPercentage)
            {
                //var isPlaying = State == ALSourceState.Playing;
                Stop();
                AL.Source(_source, ALSourcei.SampleOffset, _loopPoint);
                AL.SourcePlay(_source);
            }

            if (!EnableLoop || !_hasLoop)
                return;

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
            if (!Initialize)
                return;

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
            if (!Initialize)
                return;

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
            _loopTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _loopTimer?.Dispose();

            if (!Initialize)
                return;

            AL.Source(_source, ALSourcei.Buffer, 0);
            AL.DeleteSource(_source);
            AL.DeleteBuffer(_buffer);
            GC.SuppressFinalize(this);
        }
    }
}