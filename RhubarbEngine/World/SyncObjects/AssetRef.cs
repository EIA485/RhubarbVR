﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.Asset;

namespace RhubarbEngine.World
{
    public class AssetRef<T> : SyncRef<AssetProvider<T>>, IWorldObject where T : IAsset
    {
        public T Asset
        {
            get
            {
                if(base.target == null)
                {
                    return default(T);
                }
                return base.target.value;
            }
        }

        public event Action<T> loadChange;

        public void loadedCall(T newAsset)
        {
            loadChange?.Invoke(newAsset);
        }

        public override void Bind()
        {
            base.Bind();
            base.target.onLoadedCall += loadedCall;
            if (base.target.loaded)
            {
                loadedCall(target.value);
            }
        }
        public override void onLoaded()
        {
            base.onLoaded();
            base.target.onLoadedCall += loadedCall;
            if (base.target.loaded)
            {
                loadedCall(base.target.value);
            }
        }
        public AssetRef(IWorldObject _parent,bool newrefid = true) : base(_parent, newrefid)
        {
        }
    }
}
