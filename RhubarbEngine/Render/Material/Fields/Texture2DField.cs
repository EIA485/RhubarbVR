﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using Veldrid;
using RhubarbDataTypes;
using Veldrid.ImageSharp;
using System.IO;

namespace RhubarbEngine.Render.Material.Fields
{
	public class Texture2DField : MaterialField, IWorldObject
	{
		public AssetRef<RTexture2D> field;
		public override void BuildSyncObjs(bool newRefIds)
		{
			field = new AssetRef<RTexture2D>(this, newRefIds);
			field.LoadChange += assetChange;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (Input.MainWindows.GetKey(Key.F3))
			{
				loadTextureView(true);
			}

		}

		public override void SetValue(Object val)
		{
			field.Value = (NetPointer)val;
		}
		public void assetChange(RTexture2D newAsset)
		{
			loadTextureView();
		}

		private void SetResource(BindableResource res, bool forceR = false)
		{
			if ((resource != res) || forceR)
			{
				resource = res;
				RMaterial.ReloadBindableResources();
			}
		}
		public void loadTextureView(bool forceR = false)
		{
			if (field.Target != null)
			{
				if (field.Asset != null)
				{
					if (field.Asset.view != null)
					{
						SetResource(field.Asset.view, forceR);
					}
					else
					{
						SetResource(Engine.RenderManager.Nulview);
					}
				}
				else
				{
					SetResource(Engine.RenderManager.Nulview);
				}
			}
			else
			{
				SetResource(Engine.RenderManager.Solidview);
			}

		}

		public unsafe override void CreateDeviceResource(ResourceFactory fact)
		{
			loadTextureView();
		}
	}
}
