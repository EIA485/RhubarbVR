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
using g3;
using System.Numerics;
using ImGuiNET;

namespace RhubarbEngine.Components.ImGUI
{
    

    [Category("ImGUI/Begin")]
    public class ImGUIBeginChild : UIWidgetList
    {
        public Sync<string> id;

        public Sync<Vector2f> size;
        public Sync<bool> border;
        public Sync<ImGuiWindowFlags> windowflag;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            id = new Sync<string>(this, newRefIds);
            size = new Sync<Vector2f>(this, newRefIds);
            border = new Sync<bool>(this, newRefIds);
            windowflag = new Sync<ImGuiWindowFlags>(this, newRefIds);
            windowflag.value = ImGuiWindowFlags.None;
        }

        public ImGUIBeginChild(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIBeginChild()
        {
        }

        public override void ImguiRender()
        {
            if(ImGui.BeginChild(id.value, new Vector2(size.value.x, size.value.y), border.value, windowflag.value))
            {
                foreach (var item in children)
                {
                    item.target?.ImguiRender();
                }
                ImGui.EndChild();
            }
        }
    }
}
