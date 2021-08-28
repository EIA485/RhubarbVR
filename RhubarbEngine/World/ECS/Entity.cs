﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using RhubarbEngine.Render;
using RhubarbEngine.Components;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using System.Numerics;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.World.ECS
{
    public class Entity : Worker,IWorldObject
    {
        public Sync<Vector3f> position;

        [NoSave]
        public SyncRef<User> _manager;

        [NoSave]
        [NoSync]
        public User CreatingUser => world.users[(int)referenceID.getOwnerID()];

        [NoSave]
        [NoSync]
        public User manager { 
            get {
                User retur;
                if(_manager.target == null)
                {
                    retur = parent.target?.nullableManager;
                }
                else
                {
                    retur = _manager.target;
                }
                if(retur == null)
                {
                    retur = world.users[(int)referenceID.getOwnerID()];
                }
                return retur;
            }
            set { _manager.target = value; }
        }

        [NoSave]
        [NoSync]
        public User nullableManager
        {
            get
            {
                User retur;
                if (_manager.target == null)
                {
                    retur = parent.target?.manager;
                }
                else
                {
                    retur = _manager.target;
                }
                return retur;
            }
        }

        public Sync<Quaternionf> rotation;

        public Sync<Vector3f> scale;

        private Matrix4x4 cashedGlobalTrans = Matrix4x4.CreateScale(Vector3.One);

        private Matrix4x4 cashedLocalMatrix = Matrix4x4.CreateScale(Vector3.One);

        [NoSave]
        [NoSync]
        private Entity internalParent;

        public SyncRef<Entity> parent;

        public void AddPhysicsDisableder(IPhysicsDisableder physicsDisableder)
        {
            try
            {
                physicsDisableders.Add(physicsDisableder);
                onPhysicsDisableder?.Invoke(PhysicsDisabled);
            }
            catch
            {
            }
        }

        public Vector3f GlobalPointToLocal(Vector3f point,bool Child = true)
        {
            Matrix4x4 newtrans = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromYawPitchRoll(0f, 0f, 0f) * Matrix4x4.CreateTranslation(point.ToSystemNumrics());
            Matrix4x4 parentMatrix;
            if (Child)
            {
                parentMatrix = cashedGlobalTrans;
            }
            else
            {
                parentMatrix = parent.target.cashedGlobalTrans;
            }
            Matrix4x4.Invert(parentMatrix, out Matrix4x4 invparentMatrix);
            Matrix4x4 newlocal = newtrans * invparentMatrix;
            Matrix4x4.Decompose(newlocal, out Vector3 newscale, out Quaternion newrotation, out Vector3 newtranslation);
            return (Vector3f)newtranslation;
        }
        public Vector3f GlobalScaleToLocal(Vector3f Scale, bool Child = true)
        {
            Matrix4x4 newtrans = Matrix4x4.CreateScale((Vector3)Scale) * Matrix4x4.CreateFromYawPitchRoll(0f, 0f, 0f) * Matrix4x4.CreateTranslation(0,0,0);
            Matrix4x4 parentMatrix;
            if (Child)
            {
                parentMatrix = cashedGlobalTrans;
            }
            else
            {
                parentMatrix = parent.target.cashedGlobalTrans;
            }
            Matrix4x4.Invert(parentMatrix, out Matrix4x4 invparentMatrix);
            Matrix4x4 newlocal = newtrans * invparentMatrix;
            Matrix4x4.Decompose(newlocal, out Vector3 newscale, out Quaternion newrotation, out Vector3 newtranslation);
            return (Vector3f)newscale;
        }
        public Quaternionf GlobalRotToLocal(Quaternionf Rot, bool Child = true)
        {
            Matrix4x4 newtrans = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromQuaternion((Quaternion)Rot) * Matrix4x4.CreateTranslation(0,0,0);
            Matrix4x4 parentMatrix;
            if (Child)
            {
                parentMatrix = cashedGlobalTrans;
            }
            else
            {
                parentMatrix = parent.target.cashedGlobalTrans;
            }
            Matrix4x4.Invert(parentMatrix, out Matrix4x4 invparentMatrix);
            Matrix4x4 newlocal = newtrans * invparentMatrix;
            Matrix4x4.Decompose(newlocal, out Vector3 newscale, out Quaternion newrotation, out Vector3 newtranslation);
            return (Quaternionf)newrotation;
        }


        public void RemovePhysicsDisableder(IPhysicsDisableder physicsDisableder)
        {
            try
            {
                physicsDisableders.Remove(physicsDisableder);
                onPhysicsDisableder?.Invoke(PhysicsDisabled);
            }
            catch { 
            }
        }

        public event Action<bool> onPhysicsDisableder;

        [NoSave]
        [NoSync]
        public List<IPhysicsDisableder> physicsDisableders = new List<IPhysicsDisableder>();

        [NoSave]
        [NoSync]
        public bool PhysicsDisabled => physicsDisableders.Count > 0;

        [NoSave]
        [NoSync]
        IWorldObject IWorldObject.Parent => internalParent?._children??(IWorldObject)world;

        public Sync<string> name;

        public Sync<bool> enabled;

        public Sync<bool> persistence;

        public SyncObjList<Entity> _children;

        public SyncAbstractObjList<Component> _components;

        public Sync<int> remderlayer;

        public bool parentEnabled = true;

        public event Action enabledChanged;

        public event Action<bool> onClick;

        public event Action<GrabbableHolder, bool> onGrip;

        public event Action<GrabbableHolder, bool> onDrop;

        public event Action<bool> onPrimary;

        public event Action<bool> onSecondary;

        public event Action<bool> onTriggerTouching;

        public void SetParent(Entity entity,bool preserverGlobal = true, bool resetPos = false)
        {
            Matrix4x4 mach = cashedGlobalTrans;
            parent.target = entity;
            if (preserverGlobal)
            {
               setGlobalTrans(mach);
            }
            else if (resetPos)
            {
                setGlobalTrans(entity.cashedGlobalTrans);
            }
        }

        public void SendTriggerTouching(bool laser,bool click = true)
        {
            if (!click) return;
            onTriggerTouching?.Invoke(laser);
        }

        public void SendClick(bool laser, bool click = true)
        {
            if (!click) return;
            onClick?.Invoke(laser);
        }
        public void SendGrip(bool laser, GrabbableHolder holder, bool click = true)
        {
            if (!click) return;
            onGrip?.Invoke(holder, laser);
        }

        public void SendDrop(bool laser, GrabbableHolder holder, bool click = true)
        {
            if (!click) return;
            onDrop?.Invoke(holder, laser);
        }

        public void SendPrimary(bool laser, bool click = true)
        {
            if (!click) return;
            onPrimary?.Invoke(laser);
        }
        public void SendSecondary(bool laser, bool click = true)
        {
            if (!click) return;
            onSecondary?.Invoke(laser);
        }
        public bool isEnabled => parentEnabled && enabled.value;

        private void LoadListObject()
        {
            foreach (var item in _components)
            {
                item.ListObject(isEnabled);
            }
        }

        public void parentEnabledChange(bool _parentEnabled)
        {
            if (!enabled.value) return;
            if (_parentEnabled != parentEnabled)
            {
                parentEnabled = _parentEnabled;
                foreach (Entity item in _children)
                {
                    item.parentEnabledChange(_parentEnabled);
                }
            }
            LoadListObject();
            enabledChanged?.Invoke();
        }

        public void onPersistenceChange(IChangeable newValue)
        {
            base.Persistent = persistence.value;
        }
        public override void inturnalSyncObjs(bool newRefIds)
        {
            world.addWorldEntity(this);
        }

        public Vector3f globalPos()
        {
            Matrix4x4.Decompose(cashedGlobalTrans, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            return (Vector3f)translation;
        }
        public Quaternionf globalRot()
        {
            Matrix4x4.Decompose(cashedGlobalTrans, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            return (Quaternionf)rotation;
        }
        public Vector3f globalScale()
        {
            Matrix4x4.Decompose(cashedGlobalTrans, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            return (Vector3f)scale;
        }

        public void SetGlobalPos(Vector3f pos)
        {
            Matrix4x4.Decompose(cashedGlobalTrans, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            setGlobalTrans(Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(pos.ToSystemNumrics()));
        }

        public void SetGlobalRot(Quaternionf rot)
        {
            Matrix4x4.Decompose(cashedGlobalTrans, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            setGlobalTrans(Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rot.ToSystemNumric()) * Matrix4x4.CreateTranslation(translation));
        }

        public void SetGlobalScale(Vector3f pos)
        {
            Matrix4x4.Decompose(cashedGlobalTrans, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            setGlobalTrans(Matrix4x4.CreateScale(pos.ToSystemNumrics()) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation));
        }
        public Matrix4x4 globalTrans()
        {
            return cashedGlobalTrans;
        }

        public Vector3f up { get
            {
                var mat = cashedGlobalTrans;
                var e = new Vector3f(mat.M11 * Vector3f.AxisY.x + mat.M12 * Vector3f.AxisY.y + mat.M13 * Vector3f.AxisY.z, mat.M21 * Vector3f.AxisY.x + mat.M22 * Vector3f.AxisY.y + mat.M23 * Vector3f.AxisY.z, mat.M31 * Vector3f.AxisY.x + mat.M32 * Vector3f.AxisY.y + mat.M33 * Vector3f.AxisY.z);
                return e;

            }
        }

        public Matrix4x4 localTrans()
        {
            return cashedLocalMatrix;
        }

        public void setGlobalTrans(Matrix4x4 newtrans, bool SendUpdate = true)
        {
            Matrix4x4 parentMatrix = Matrix4x4.CreateScale(Vector3.One);
            if (internalParent != null)
            {
                parentMatrix = internalParent.globalTrans();
            }
            Matrix4x4.Invert(parentMatrix, out Matrix4x4 invparentMatrix);
            Matrix4x4 newlocal = newtrans * invparentMatrix;
            Matrix4x4.Decompose(newlocal, out Vector3 newscale, out Quaternion newrotation, out Vector3 newtranslation);
            position.setValueNoOnChange((Vector3f)newtranslation);
            rotation.setValueNoOnChange((Quaternionf)newrotation);
            scale.setValueNoOnChange((Vector3f)newscale);
            cashedGlobalTrans = newtrans;
            updateGlobalTrans(SendUpdate);
        }

        public Action<Matrix4x4> GlobalTransformChange;
        public Action<Matrix4x4> GlobalTransformChangePhysics;

        public void setLocalTrans(Matrix4x4 newtrans)
        {
            Matrix4x4.Decompose(newtrans, out Vector3 newscale, out Quaternion newrotation, out Vector3 newtranslation);
            position.setValueNoOnChange((Vector3f)newtranslation);
            rotation.setValueNoOnChange((Quaternionf)newrotation);
            scale.setValueNoOnChange((Vector3f)newscale);
            updateGlobalTrans();
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            position = new Sync<Vector3f>(this, newRefIds);
            scale = new Sync<Vector3f>(this, newRefIds);
            scale.value = Vector3f.One;
            rotation = new Sync<Quaternionf>(this, newRefIds);
            rotation.value = Quaternionf.Identity;
            name = new Sync<string>(this, newRefIds);
            enabled = new Sync<bool>(this, newRefIds);
            persistence = new Sync<bool>(this, newRefIds);
            persistence.value = true;
            _children = new SyncObjList<Entity>(this, newRefIds);
            _components = new SyncAbstractObjList<Component>(this, newRefIds);
            _manager = new SyncRef<User>(this, newRefIds);
            enabled.value = true;
            parent = new SyncRef<Entity>(this, newRefIds);
            parent.Changed += Parent_Changed;
            remderlayer = new Sync<int>(this, newRefIds);
            remderlayer.value = (int)RemderLayers.normal;
            position.Changed += onTransChange;
            rotation.Changed += onTransChange;
            scale.Changed += onTransChange;
            enabled.Changed += onEnableChange;
            persistence.Changed += onPersistenceChange;
        }

        private void Parent_Changed(IChangeable obj)
        {
            if (world.RootEntity == this) return;
            if (parent.target == internalParent) return;
            if(internalParent == null)
            {
                internalParent = parent.target;
                updateGlobalTrans();
                return;
            }
            if (parent.target == null) {
                parent.target = world.RootEntity;
                return;
            }
            if(world != parent.target.world)
            {
                logger.Log("tried to set parent from another world");
                return;
            }
            if (!parent.target.CheckIfParented(this))
            {
                parent.target._children.AddInternal(this);
                internalParent._children.RemoveInternal(this);
                internalParent = parent.target;
                parentEnabledChange(internalParent.isEnabled);
                updateGlobalTrans();
            }
            else
            {
                parent.target = internalParent;
            }
        }
        public bool CheckIfParented(Entity entity)
        {
            if(entity == this)
            {
                return true;
            }
            else
            {
                return internalParent?.CheckIfParented(entity)??false;
            }
        }
        private void onTransChange(IChangeable newValue)
        {
            updateGlobalTrans();
        }

        private void onEnableChange(IChangeable newValue)
        {
            foreach (Entity item in _children)
            {
                item.parentEnabledChange(enabled.value);
            }
            LoadListObject();
            enabledChanged?.Invoke();
        }
        private void updateGlobalTrans(bool Sendupdate = true)
        {
            Matrix4x4 parentMatrix = Matrix4x4.CreateScale(Vector3.One);
            if (internalParent != null)
            {
                parentMatrix = internalParent.globalTrans();
            }
            Matrix4x4 localMatrix = Matrix4x4.CreateScale((Vector3)scale.value) * Matrix4x4.CreateFromQuaternion(rotation.value.ToSystemNumric()) * Matrix4x4.CreateTranslation((Vector3)position.value);
            cashedGlobalTrans = localMatrix * parentMatrix;
            cashedLocalMatrix = localMatrix;
            if(Sendupdate)GlobalTransformChangePhysics?.Invoke(cashedGlobalTrans);
            GlobalTransformChange?.Invoke(cashedGlobalTrans);
            foreach (Entity entity in _children)
            {
                entity.updateGlobalTrans();
            }
        }
        [NoSync]
        [NoSave]
        public Entity addChild(string name = "Entity")
        {
            Entity val = _children.Add(true);
            val.parent.target = this;
            val.name.value = name;
            return val;
        }
        [NoSync]
        [NoSave]
        public T attachComponent<T>() where T: Component
        {
            T newcomp = (T)Activator.CreateInstance(typeof(T));
            _components.Add(newcomp);
            try
            {
                newcomp.OnAttach();
            }
            catch (Exception e)
            {
                Logger.Log("Failed To run Attach On Component" + typeof(T).Name + " Error:" + e.ToString());
            }
            newcomp.onLoaded();
            return newcomp;
        }
        public override void onLoaded()
        {
            base.onLoaded();
            updateGlobalTrans();
        }
        [NoSync]
        [NoSave]
        public T getFirstComponent<T>() where T : Component
        {
            foreach (var item in _components)
            {
                if (item.GetType() == typeof(T))
                {
                    return (T)item;
                }
            }
            return null;
        }
        [NoSync]
        [NoSave]
        public IEnumerable<T> getAllComponents<T>() where T : Component
        {
            foreach (var item in _components)
            {
                if (typeof(T).IsAssignableFrom(item.GetType()))
                {
                    yield return (T)item;
                }
            }
        }

        public void addToRenderQueue(RenderQueue gu, Vector3 playpos, RemderLayers layer)
        {
            if (((int)layer & remderlayer.value) <= 0)
            {
                return;
            }
            foreach (object comp in _components)
            {
                try
                {
                    gu.Add(((Renderable)comp), playpos);
                }
                catch
                {}
            }
        }

        public override void Dispose()
        {
            logger.Log("Entity Remove");
            base.Dispose();
            world.removeWorldEntity(this);
        }

        public void Update(DateTime startTime, DateTime Frame)
        {
            if (base.IsRemoved) return;
            foreach (Component comp in _components)
            {
                if (comp.enabled.value&& !comp.IsRemoved && world.Focus != World.FocusLevel.Background)
                {
                    comp.CommonUpdate(startTime, Frame);
                }
            }
        }

        public Entity(IWorldObject _parent,bool newRefIds=true) : base(_parent.World, _parent, newRefIds)
        {

        }
        public Entity()
        {
        }
    }
}
