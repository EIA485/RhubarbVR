﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine;
using SteamAudio;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using OpenAL;
using NAudio;
using NAudio.Wave;

namespace RhubarbEngine.Managers
{
    public interface IAudioManager : IManager
    {
        bool OpenAl { get; }
        int SamplingRate { get; }
        Stopwatch Stopwatch { get; }
        int AudioFrameSize { get; }
        int AudioFrameSizeInBytes { get; }
        IPL._IPLContext_t IplContext { get; }
        ref IPL._IPLContext_t RefIplContext { get; }
        IPL.AudioSettings IplAudioSettings { get; }
        ref IPL.AudioSettings RefIplAudioSettings { get; }
        IPL._IPLHRTF_t Hrtf { get; }
        Thread Task { get; }

        void UnloadAll();
    }


    public class AudioManager : IAudioManager
    {
		private IEngine _engine;

        public bool OpenAl
        {
            get
            {
                return _engine?.SettingsObject.AudioSettings.OpenAL ?? true;
            }
        }

        //NAudio
        private DirectSoundOut _waveOutDevice;
		private BufferedWaveProvider _mainOutputStream;

		private bool _running = false;
		public Stopwatch Stopwatch { get; private set; }

        public int SamplingRate
        {
            get
            {
                return _engine?.SettingsObject.AudioSettings.SamplingRate ?? 48000;
            }
        }

        public int AudioFrameSize
        {
            get
            {
                return _engine?.SettingsObject.AudioSettings.AudioFrameSize ?? 2048;
            }
        }

        public int AudioFrameSizeInBytes
        {
            get
            {
                return AudioFrameSize * sizeof(float);
            }
        }

        //OpenAL
        private IntPtr _alAudioDevice;
		private IntPtr _alAudioContext;

		public const int BUFFER_FORMAT_STEREO_FLOAT_32 = 0x10011;

		private IntPtr _outBuff;

		public float[] ee;

		public IPL._IPLHRTF_t hrtf;

        public IPL._IPLHRTF_t Hrtf
        {
            get
            {
                return hrtf;
            }
        }
        //Steam Audio
        public IPL._IPLContext_t iplContext;
        public IPL._IPLContext_t IplContext { get { return iplContext; } }

        public ref IPL._IPLContext_t RefIplContext
        {
            get
            {
                return ref iplContext;
            }
        }

        public IPL.AudioBuffer iplOutputBuffer;

		public IPL.AudioSettings iplAudioSettings;
        public IPL.AudioSettings IplAudioSettings { get { return iplAudioSettings; } }

        public ref IPL.AudioSettings RefIplAudioSettings
        {
            get
            {
                return ref iplAudioSettings;
            }
        }

        public uint sourceId;

		private uint[] _alBuffers;

		public Thread Task { get; private set; }

		public int audioframeTimeMs;

		public unsafe IManager Initialize(IEngine _engine)
		{
            if(!_engine.Audio)
            {
                return this;
            }
			audioframeTimeMs = AudioFrameSize / (SamplingRate / 1000);
			ee = new float[AudioFrameSize * 2];
			iplAudioSettings = new IPL.AudioSettings { frameSize = AudioFrameSize, samplingRate = SamplingRate };
			this._engine = _engine;
			GC.KeepAlive(ee);

			Stopwatch = new Stopwatch();

			_outBuff = Marshal.AllocHGlobal(AudioFrameSizeInBytes * 2);

			if (OpenAl)
			{
                try
                {
                    PrepareOpenAL();
                }
                catch(Exception e)
                {
                    _engine.Logger.Log(e.Message, true);
                    _engine.Audio = false;
                    return this;
                }

            }
			else
			{
				PrepareNAudio();
			}

			PrepareSteamAudio();

			if (OpenAl)
			{
				AL.GenBuffers(_alBuffers.Length, _alBuffers);
				AL.GenSource(out sourceId);
			}
			Stopwatch.Start();

			Console.WriteLine("Starting Audio task");
			_running = true;
			Task = OpenAl ? new Thread(OpenALUpdater, 1024 * 4) : new Thread(NaudioUpdater, 1024 * 4);
            Task.Name = "Audio";
			Task.IsBackground = true;
			Task.Priority = ThreadPriority.Highest;
			return this;
		}

		private void PrepareNAudio()
		{
			_managedArray = new byte[(AudioFrameSizeInBytes * 2)];
			GC.KeepAlive(_managedArray);

			try
			{
				_waveOutDevice = new DirectSoundOut(70);
			}
			catch (Exception driverCreateException)
			{
				Console.WriteLine(string.Format("{0}", driverCreateException.Message));
				return;
			}

            _mainOutputStream = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(SamplingRate, 2))
            {
                BufferLength = AudioFrameSizeInBytes * 4,
                DiscardOnBufferOverflow = false
            };

            try
			{
				_waveOutDevice.Init(_mainOutputStream);
				_waveOutDevice.Play();
			}
			catch (Exception initException)
			{
				Console.WriteLine(string.Format("{0}", initException.Message), "Error Initializing Output");
				return;
			}
		}

		private void SteamLogFunction(IPL.LogLevel level, [MarshalAs(UnmanagedType.LPStr)] string message)
		{
			switch (level)
			{
				case IPL.LogLevel.Info:
					Console.WriteLine("Steam Audio Info" + message);
					break;
				case IPL.LogLevel.Warning:
                    _engine.Logger.Log("Steam Audio Warning" + message);
					break;
				case IPL.LogLevel.Error:
                    _engine.Logger.Log("Steam Audio Error" + message, true);
					break;
				case IPL.LogLevel.Debug:
					Console.WriteLine("Steam Audio Debug" + message);
					break;
				default:
					break;
			}
		}
		private void PrepareSteamAudio()
		{
			//Steam Audio Initialization

			var set = new IPL.ContextSettings { logCallback = SteamLogFunction, version = ((uint)4 << 16) | ((uint)0 << 8) | ((uint)0) };

			IPL.ContextCreate(ref set, out iplContext);


			var herfset = new IPL.HRTFSettings { type = IPL.HRTFType.Default };

			IPL.HRTFCreate(iplContext, ref iplAudioSettings, ref herfset, out hrtf);


			_engine.Logger.Log("Created SteamAudio context.");
			IPL.AudioBufferAllocate(_engine.AudioManager.IplContext, 2, AudioFrameSize, ref iplOutputBuffer);

            _engine.Logger.Log("SteamAudio is ready.");

		}


		public unsafe void OpenALUpdater()
		{
			while (_running)
			{
				try
				{
					OpenALUpdate();
				}
				catch
				{

				}
			}
		}
		private unsafe void NaudioUpdater()
		{
			while (_running)
			{
				try
				{
					NaudioUpdate();
				}
				catch
				{

				}
			}
		}

		public unsafe void RunOutput()
		{
			IPL.AudioBufferDeinterleave(iplContext, ee, ref iplOutputBuffer);
			foreach (var world in _engine.WorldManager.Worlds)
			{
				if (world.Focus != World.World.FocusLevel.Background)
				{
					foreach (var comp in world.updateLists.audioOutputs)
					{
						if (comp.IsNotCulled)
						{
							comp.AudioUpdate();
							IPL.AudioBufferMix(iplContext, ref comp.iplOutputBuffer, ref iplOutputBuffer);
						}
					}

				}
			}
			IPL.AudioBufferInterleave(iplContext, ref iplOutputBuffer, _outBuff);
		}
		byte[] _managedArray;
		public unsafe void NaudioUpdate()
		{
			var space = (int)(_waveOutDevice.GetPosition() % _managedArray.Length);
			if (space < (int)(AudioFrameSizeInBytes / 2))
			{
				var watch = new System.Diagnostics.Stopwatch();
				watch.Start();
				RunOutput();
				Marshal.Copy(_outBuff, _managedArray, 0, _managedArray.Length);
				_mainOutputStream.AddSamples(_managedArray, 0, _managedArray.Length);
				watch.Stop();
				Thread.Sleep(audioframeTimeMs - (int)watch.ElapsedMilliseconds - 5);
			}
		}

		public unsafe void OpenALUpdate()
		{
			AL.GetSource(sourceId, AL.GetSourceInt.BuffersProcessed, out var numProcessedBuffers);
			AL.GetSource(sourceId, AL.GetSourceInt.BuffersQueued, out var numQueuedBuffers);

			var buffersToAdd = _alBuffers.Length - numQueuedBuffers + numProcessedBuffers;

            if (buffersToAdd <= 0)
            {
                return;
            }
			RunOutput();

			while (buffersToAdd > 0)
			{
				var bufferId = _alBuffers[buffersToAdd - 1];

				if (numProcessedBuffers > 0)
				{
					AL.SourceUnqueueBuffers(sourceId, 1, &bufferId);

					numProcessedBuffers--;
				}

				AL.BufferData(bufferId, BUFFER_FORMAT_STEREO_FLOAT_32, _outBuff, AudioFrameSizeInBytes * 2, SamplingRate);
				AL.SourceQueueBuffers(sourceId, 1, &bufferId);
                CheckALErrors();

				buffersToAdd--;
			}

			//Start playback whenever it stops
			AL.GetSource(sourceId, AL.GetSourceInt.SourceState, out var sourceState);

			if ((AL.SourceState)sourceState != AL.SourceState.Playing)
			{
				AL.SourcePlay(sourceId);
			}

            CheckALErrors();
		}

		private static void CheckALErrors()
		{
			var error = AL.GetError();

			if (error != AL.Error.NoError)
			{
				throw new Exception($"OpenAL Error: {error}");
			}
		}

		private void PrepareOpenAL()
		{
			_alBuffers = new uint[_engine.SettingsObject.AudioSettings.BufferCount];

			_alAudioDevice = ALC.OpenDevice(null);
			_alAudioContext = ALC.CreateContext(_alAudioDevice, null);

			if (!ALC.MakeContextCurrent(_alAudioContext))
			{
				throw new InvalidOperationException("Unable to make context current");
			}

            _engine.Logger.Log("Created OpenAL context.");

			//Require float32 support.
			const string FLOAT_32_EXTENSION = "AL_EXT_float32";

			if (!AL.IsExtensionPresent(FLOAT_32_EXTENSION))
			{
				throw new Exception($"This program requires '{FLOAT_32_EXTENSION}' OpenAL extension to function.");
			}

            CheckALErrors();

            _engine.Logger.Log("OpenAL is ready.");
		}
		private void UnloadOpenAL()
		{
			ALC.DestroyContext(_alAudioContext);
			ALC.CloseDevice(_alAudioDevice);
		}


		private void UnloadSteamAudio()
		{
			IPL.AudioBufferFree(iplContext, ref iplOutputBuffer);
			IPL.ContextRelease(ref iplContext);
		}

		public void UnloadAll()
		{
			_running = false;
			if (OpenAl)
			{
				UnloadOpenAL();
			}
			UnloadSteamAudio();
		}

        public void Update()
        {
        }
    }
}
