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
using RhubarbEngine.Render.Material.Fields;
using RhubarbEngine.Render.Shader;
using System.Net;
using System.Web;
using System.Net.Http;

namespace RhubarbEngine.Components.Assets
{
	[Category(new string[] { "Assets/Texture2Ds" })]
	public class RhubarbTextue2D : AssetProvider<RTexture2D>, IAsset
	{
		public Sync<bool> solid;

		public override void OnLoaded()
		{
			LoadRhubarbTexture();
		}

		private void LoadRhubarbTexture()
		{
			if (solid.Value)
			{
				Load(new RTexture2D(Engine.RenderManager.RhubarbSolidview));
			}
			else
			{
				Load(new RTexture2D(Engine.RenderManager.Rhubarbview));
			}

		}

		public override void BuildSyncObjs(bool newRefIds)
		{
			solid = new Sync<bool>(this, newRefIds);
		}
		public override void OnChanged()
		{
			base.OnChanged();
			LoadRhubarbTexture();
		}

		public RhubarbTextue2D(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public RhubarbTextue2D()
		{
		}
	}
}
