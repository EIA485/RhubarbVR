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
using Veldrid;

namespace RhubarbEngine.Components.ImGUI
{
    

    [Category("ImGUI/Interaction/Button")]
    public class ImGUIColorButton : UIWidget
    {

        public Sync<Colorf> color;
        public Sync<string> id;
        public Sync<ImGuiColorEditFlags> imGuiColorEditFlags;

        public SyncDelegate action;
        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            color = new Sync<Colorf>(this, newRefIds);
            id = new Sync<string>(this, newRefIds);
            imGuiColorEditFlags = new Sync<ImGuiColorEditFlags>(this, newRefIds);
            imGuiColorEditFlags.value = ImGuiColorEditFlags.None;
            action = new SyncDelegate(this, newRefIds);
        }

        public ImGUIColorButton(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIColorButton()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            if (ImGui.ColorButton(id.value ?? "", color.value.ToRGBA().ToSystem(), imGuiColorEditFlags.value))
            {
                action.Target?.Invoke();
            }
        }
    }
}
