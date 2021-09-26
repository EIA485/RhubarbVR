﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using RhubarbEngine.Render.Shader.Vals;

namespace RhubarbEngine.Render.Material.Fields
{
	public class FloatField : GenericField<float>
	{

		public override void CreateDeviceResource(ResourceFactory fact)
		{
			if (resource != null)
			{
				return;
			}
			resource = fact.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		}
		unsafe public override void UpdateBuffer(GraphicsDevice gb)
		{

            if (gb is null)
            {
                return;
            }
            gb.UpdateBuffer((DeviceBuffer)resource, 0, new Val_float(field.Value));
		}
	}
}
