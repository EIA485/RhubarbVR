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

namespace RhubarbEngine.World.Asset
{
    public abstract class AssetProvider<A> : Component where A: IAsset
    {
        private A _value;
        public A value{get{
                return _value;
        } }

        public void load(A data)
        {
            _value = data;
        }
        
        public AssetProvider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public AssetProvider()
        {
        }
    }
}
