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
using ImGuiNET;
using Veldrid;

namespace RhubarbEngine.Components.ImGUI
{


	[Category("ImGUI/Interaction")]
	public class ImGUIint : UIWidget
	{

		public Sync<string> label;

		public Sync<int> value;
		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			label = new Sync<string>(this, newRefIds);
			value = new Sync<int>(this, newRefIds);
		}

		public ImGUIint(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIint()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			var vale = value.Value;
			ImGui.DragInt(label.Value ?? "", ref vale);
			if (vale != value.Value)
			{
				value.Value = vale;
			}
		}
	}
}
