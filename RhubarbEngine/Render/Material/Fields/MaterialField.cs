﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RhubarbEngine.Render.Shader;
using Veldrid;
using RhubarbEngine.Components.Assets;

namespace RhubarbEngine.Render.Material.Fields
{
	public abstract class MaterialField : Worker, IWorldObject
	{
		public ShaderValueType valueType;

		[NoShow]
		public Sync<ShaderType> shaderType;
		[NoShow]
		public Sync<string> fieldName;

		[NoShow]
		[NoSave]
		[NoSync]
		public BindableResource resource;

		[NoShow]
		[NoSave]
		[NoSync]
		public RMaterial rMaterial => (RMaterial)parent.Parent;

		public override void OnLoaded()
		{
			base.OnLoaded();
			createDeviceResource(Engine.renderManager.gd.ResourceFactory);
			updateBuffer(Engine.renderManager.gd);
		}

		public virtual void setValue(Object val)
		{

		}

		public override void InturnalSyncObjs(bool newRefIds)
		{
			shaderType = new Sync<ShaderType>(this, newRefIds);
			fieldName = new Sync<string>(this, newRefIds);
		}

		public virtual void updateBuffer(GraphicsDevice gb)
		{

		}

		public virtual void createDeviceResource(ResourceFactory fact)
		{
		}
	}
}
