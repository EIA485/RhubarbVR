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


	[Category("ImGUI/Begin/Popup")]
	public class ImGUIBeginPopup : UIWidgetList
	{

		public Sync<string> id;

		public Sync<ImGuiWindowFlags> windowflag;

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			id = new Sync<string>(this, newRefIds);
			windowflag = new Sync<ImGuiWindowFlags>(this, newRefIds);
		}

		public ImGUIBeginPopup(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIBeginPopup()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (ImGui.BeginPopup(id.Value ?? "", windowflag.Value))
			{
				foreach (var item in children)
				{
					item.Target?.ImguiRender(imGuiRenderer, canvas);
				}
				ImGui.EndPopup();
			}


		}
	}
}

