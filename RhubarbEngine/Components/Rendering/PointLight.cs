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
using RhubarbEngine.Render;
using RhubarbEngine.World.Asset;
using g3;
using Veldrid;
using System.Numerics;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;
using System.IO;
using RhubarbEngine.Components.Assets;

namespace RhubarbEngine.Components.Rendering
{
    [Category(new string[] { "Rendering" })]
    public class PointLight : RenderObject
    {


        public override void buildSyncObjs(bool newRefIds)
        {

        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {

        }
        public override void onLoaded()
        {

        }
        public override void onChanged()
        {
        }
        public PointLight(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public PointLight()
        {
        }
    }
}
