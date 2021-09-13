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
using RhubarbEngine.World.Asset;
using RNumerics;
using System.Numerics;
using Veldrid;
using RhubarbEngine.Render;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;
using System.IO;
using RhubarbEngine.Components.Assets;

namespace RhubarbEngine.Components.Assets
{
	[Category(new string[] { "Assets/Utility" })]
	public class AssetMultiplexer<T> : AssetProvider<T> where T : class, IAsset
	{
		public Sync<int> index;

		public SyncAssetRefList<T> targets;

		public override void BuildSyncObjs(bool newRefIds)
		{
			index = new Sync<int>(this, newRefIds);
			index.Changed += Index_Changed;
			targets = new SyncAssetRefList<T>(this, newRefIds);
			targets.loadChange += UpdateTargets;
		}

		private void Index_Changed(IChangeable obj)
		{
			UpdateProvider();
		}

		private void UpdateTargets(T val)
		{
			UpdateProvider();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			UpdateProvider();
		}

		private void UpdateProvider()
		{
			if (index.Value >= targets.Length)
			{
				load(null);
			}
			else
			{
				load(targets[index.Value]?.Asset);
			}
		}

		public AssetMultiplexer(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public AssetMultiplexer()
		{
		}
	}
}
