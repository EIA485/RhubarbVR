﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using System.Collections;

namespace RhubarbEngine.World
{
    public class SyncAssetRefList<T> : Worker, IWorldObject, ISyncMember where T : IAsset
    {
        private List<AssetRef<T>> _syncreflist = new List<AssetRef<T>>();

        AssetRef<T> this[int i]
        {
            get
            {
                return _syncreflist[i];
            }
        }

        public int Length { get { return _syncreflist.Count; } }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _syncreflist.Count; i++)
            {
                yield return this[i].Asset;
            }
        }


        private void onLoad(T val)
        {
            loadChange?.Invoke(val);
        }

        public Action<T> loadChange;


        public AssetRef<T> Add(bool RefID = true)
        {
            AssetRef<T> a = new AssetRef<T>(this, RefID);
            a.loadChange += onLoad;
            _syncreflist.Add(a);
            netAdd(a);
            return a;
        }

        private void netAdd(AssetRef<T> val)
        {
            DataNodeGroup send = new DataNodeGroup();
            send.setValue("Type", new DataNode<byte>(0));
            DataNodeGroup tip = val.serialize();
            send.setValue("Data", tip);
            world.addToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
        }

        private void netClear()
        {
            DataNodeGroup send = new DataNodeGroup();
            send.setValue("Type", new DataNode<byte>(1));
            world.addToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
        }

        public void ReceiveData(DataNodeGroup data, LiteNetLib.NetPeer peer)
        {
            if (((DataNode<byte>)data.getValue("Type")).Value == 1)
            {
                _syncreflist.Clear();
            }
            else
            {
                AssetRef<T> a = new AssetRef<T>(this, false);
                a.loadChange += onLoad;
                a.initialize(world, this, false);
                List<Action> actions = new List<Action>();
                a.deSerialize((DataNodeGroup)data.getValue("Data"), actions, false);
                foreach (var item in actions)
                {
                    item?.Invoke();
                }
                _syncreflist.Add(a);
            }
        }

        public void Clear()
        {
            _syncreflist.Clear();
            netClear();
        }

        public SyncAssetRefList(IWorldObject _parent,bool newref = true) : base(_parent.World, _parent, newref)
        {

        }

        public DataNodeGroup serialize(bool netsync = false)
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNodeList list = new DataNodeList();
            foreach(AssetRef<T> val in _syncreflist)
            {
                DataNodeGroup tip = val.serialize(netsync);
                if (tip != null)
                {
                    list.Add(tip);
                }
            }
            obj.setValue("list", list);
            return obj;
        }

        public void deSerialize(DataNodeGroup data, List<Action> onload = default(List<Action>), bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncAssetRefList");
                return;
            }
            if (NewRefIDs)
            {
                newRefID.Add(((DataNode<NetPointer>)data.getValue("referenceID")).Value.getID(), referenceID.getID());
                if (latterResign.ContainsKey(((DataNode<NetPointer>)data.getValue("referenceID")).Value.getID()))
                {
                    foreach (RefIDResign func in latterResign[((DataNode<NetPointer>)data.getValue("referenceID")).Value.getID()])
                    {
                        func(referenceID.getID());
                    }
                }
            }
            else
            {
                referenceID = ((DataNode<NetPointer>)data.getValue("referenceID")).Value;
                world.addWorldObj(this);
            }
            foreach (DataNodeGroup val in ((DataNodeList)data.getValue("list")))
            {
                Add(NewRefIDs).deSerialize(val, onload, NewRefIDs, newRefID, latterResign);
            }
        }


    }
}
