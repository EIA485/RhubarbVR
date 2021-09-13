﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RNumerics;
using System.Numerics;
using SteamAudio;
using RhubarbEngine.Managers;
using System.Threading;
using System.Runtime.InteropServices;

namespace RhubarbEngine.Components.Audio
{
	[Category(new string[] { "Audio" })]
	public class AudioOutput : Component
	{
		public SyncRef<IAudioSource> audioSource;

		public Sync<float> spatialBlend;
		public Sync<float> cullingDistance;

		public IPL.AudioBuffer iplOutputBuffer;

		private IPL._IPLBinauralEffect_t iplBinauralEffect;
		private IPL.AudioBuffer iplInputBuffer;

		public bool isNotCulled
		{
			get
			{
				if (audioSource.Target == null)
					return false;
				if (!audioSource.Target.IsActive)
					return false;
				if (audioSource.Target.ChannelCount != 1)
					return false;
				float e = Entity.GlobalPos().Distance(World.HeadTrans.Translation);
				return (e <= cullingDistance.Value);
			}
		}

		public override void BuildSyncObjs(bool newRefIds)
		{
			audioSource = new SyncRef<IAudioSource>(this, newRefIds);
			spatialBlend = new Sync<float>(this, newRefIds);
			spatialBlend.Value = 1f;
			cullingDistance = new Sync<float>(this, newRefIds);
			cullingDistance.Value = 100f;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			IPL.AudioBufferAllocate(Engine.audioManager.iplContext, 1, Engine.audioManager.AudioFrameSize, ref iplInputBuffer);

			IPL.AudioBufferAllocate(Engine.audioManager.iplContext, 2, Engine.audioManager.AudioFrameSize, ref iplOutputBuffer);

			var setings = new IPL.BinauralEffectSettings { hrtf = Engine.audioManager.hrtf };

			var error = IPL.BinauralEffectCreate(Engine.audioManager.iplContext, ref Engine.audioManager.iplAudioSettings, ref setings, out iplBinauralEffect);
			if (error != IPL.Error.Success)
			{
				Logger.Log("There is a prolem with BinauralEffectCreate");
			}
		}

		public override void LoadListObject()
		{
			base.LoadListObject();
			try
			{
				World.updateLists.audioOutputs.SafeAdd(this);
			}
			catch { }
		}

		public override void RemoveListObject()
		{
			base.RemoveListObject();
			try
			{
				World.updateLists.audioOutputs.Remove(this);
			}
			catch { }
		}
		public unsafe void AudioUpdate()
		{
			if (iplBinauralEffect == default)
				return;
			if (!audioSource.Target.IsActive)
				return;
			var data = audioSource.Target.FrameInputBuffer;
			if (data == null)
				return;
			if (data.Length < Engine.audioManager.AudioFrameSizeInBytes)
				return;
			Matrix4x4.Decompose(World.HeadTrans, out Vector3 sc, out Quaternion ret, out Vector3 trans);
			var position = Vector3.Transform((Entity.GlobalPos().ToSystemNumrics() - trans), Quaternion.Inverse(ret));
			var e = new IPL.Vector3(position.X, position.Y, position.Z);
			fixed (byte* ptr = data)
			{
				var prams = new IPL.BinauralEffectParams { direction = e, interpolation = IPL.HRTFInterpolation.Nearest, spatialBlend = spatialBlend.Value, hrtf = Engine.audioManager.hrtf };
				IPL.AudioBufferDeinterleave(Engine.audioManager.iplContext, (IntPtr)ptr, ref iplInputBuffer);
				IPL.BinauralEffectApply(iplBinauralEffect, ref prams, ref iplInputBuffer, ref iplOutputBuffer);
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			IPL.AudioBufferFree(Engine.audioManager.iplContext, ref iplInputBuffer);
		}

		public AudioOutput(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public AudioOutput()
		{
		}
	}
}
