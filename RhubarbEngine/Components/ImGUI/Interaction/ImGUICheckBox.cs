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
	public class ImGUICheckBox : UIWidget
	{

		public Sync<string> label;

		public Sync<bool> value;
		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			label = new Sync<string>(this, newRefIds);
			value = new Sync<bool>(this, newRefIds);
		}

		public ImGUICheckBox(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUICheckBox()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			bool vale = value.Value;
			ImGui.Checkbox(label.Value ?? "", ref vale);
			if (vale != value.Value)
			{
				value.Value = vale;
			}
		}
	}
}
