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


	[Category("ImGUI/Begin")]
	public class ImGUIBeginMenuBar : UIWidgetList
	{


		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);

		}

		public ImGUIBeginMenuBar(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIBeginMenuBar()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (ImGui.BeginMenuBar())
			{
				foreach (var item in children)
				{
					item.Target?.ImguiRender(imGuiRenderer, canvas);
				}
				ImGui.EndMenuBar();
			}


		}
	}
}

