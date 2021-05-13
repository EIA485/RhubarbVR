﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using BaseR;

namespace RhubarbEngine.World
{
    public class Worker : IWorldObject
    {
        public World world { get; protected set; }

        private IWorldObject parent;

        public RefID referenceID { get; protected set; }

        public bool Persistent = true;

        RefID IWorldObject.ReferenceID => referenceID;

        World IWorldObject.World => world;

        IWorldObject IWorldObject.Parent => parent;

        bool IWorldObject.IsLocalObject => false;

        bool IWorldObject.IsPersistent => Persistent;

        bool IWorldObject.IsRemoved => false;

        public Worker(World _world, IWorldObject _parent, bool newRefID = true)
        {
            world = _world;
            parent = _parent;
            inturnalSyncObjs(newRefID);
            buildSyncObjs(newRefID);
            if (newRefID)
            {
                referenceID = _world.buildRefID();
                _world.addWorldObj(this);
                onLoaded();
            }
        }

        public void initialize(World _world, IWorldObject _parent, bool newRefID = true)
        {
            world = _world;
            parent = _parent;
            inturnalSyncObjs(newRefID);
            buildSyncObjs(newRefID);
            if (newRefID)
            {
                referenceID = _world.buildRefID();
                _world.addWorldObj(this);
                onLoaded();
            }
        }
        public Worker()
        {

        }

        public virtual void inturnalSyncObjs(bool newRefIds)
        {

        }


        public virtual void buildSyncObjs(bool newRefIds)
        {

        }

        public virtual void onLoaded()
        {

        }

        public virtual void Dispose()
        {
            world.removeWorldObj(this);
        }
        public virtual void CommonUpdate()
        {

        }
        public virtual DataNodeGroup serialize() {
            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            DataNodeGroup obj = new DataNodeGroup();
            if (Persistent)
            {
                foreach (var field in fields)
                {
                    if (typeof(IWorldObject).IsAssignableFrom(field.FieldType))
                    {
                        obj.setValue(field.Name, ((IWorldObject)field.GetValue(this)).serialize());
                    }
                }
                DataNode<RefID> Refid = new DataNode<RefID>(referenceID);
                obj.setValue("referenceID", Refid);
            }
            return obj;
        }

        public virtual void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<RefID, RefID> newRefID = default(Dictionary<RefID, RefID>), Dictionary<RefID, RefIDResign> latterResign = default(Dictionary<RefID, RefIDResign>))
        {
            if(data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading Node");
                return;
            }
            if (NewRefIDs)
            {
                newRefID.Add(((DataNode<RefID>)data.getValue("referenceID")).Value, referenceID);
                latterResign[((DataNode<RefID>)data.getValue("referenceID")).Value](referenceID);
            }
            else
            {
                referenceID = ((DataNode<RefID>)data.getValue("referenceID")).Value;
                world.addWorldObj(this);
            }

            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (typeof(IWorldObject).IsAssignableFrom(field.FieldType))
                {
                    if (((IWorldObject)field.GetValue(this)) == null)
                    {
                        throw new Exception("Sync not initialized on " + this.GetType().FullName + " Firld: " + field.Name);
                    }
                    ((IWorldObject)field.GetValue(this)).deSerialize((DataNodeGroup)data.getValue(field.Name), NewRefIDs, newRefID, latterResign);
                }
            }
            onLoaded();
        }

    }
}
