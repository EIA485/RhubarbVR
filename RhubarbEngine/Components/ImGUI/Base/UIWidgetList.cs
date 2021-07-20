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

namespace RhubarbEngine.Components.ImGUI
{
    [Category(new string[] { "ImGUI" })]
    public abstract class UIWidgetList : Component, IUIElement
    {
        public SyncRefList<IUIElement> children;

        public override void inturnalSyncObjs(bool newRefIds)
        {
            children = new SyncRefList<IUIElement>(this,newRefIds);
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {

        }

        public virtual void ImguiRender()
        {

        }

        public UIWidgetList(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public UIWidgetList()
        {
        }
    }
}
