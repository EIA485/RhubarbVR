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
using RhubarbEngine.Components.Physics.Colliders;
using BulletSharp;
using BulletSharp.Math;
using g3;
using Veldrid;

namespace RhubarbEngine.Components.Interaction
{

    [Category(new string[] { "Interaction" })]
    public class InteractionLaser: Component
    {
        public Sync<InteractionSource> source;

        public Sync<Vector3f> rayderection;

        public Sync<float> distances;

        public override void buildSyncObjs(bool newRefIds)
        {
            source = new Sync<InteractionSource>(this, newRefIds);
            source.value = InteractionSource.HeadLaser;
            rayderection = new Sync<Vector3f>(this, newRefIds);
            rayderection.value = -Vector3f.AxisZ;
            distances = new Sync<float>(this, newRefIds);
            distances.value = 10f;
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            try
            {
                System.Numerics.Matrix4x4.Decompose((System.Numerics.Matrix4x4.CreateTranslation((rayderection.value * distances.value).ToSystemNumrics()) * entity.globalTrans()), out System.Numerics.Vector3 vs, out System.Numerics.Quaternion vr, out System.Numerics.Vector3 val);
                System.Numerics.Matrix4x4.Decompose( entity.globalTrans(), out System.Numerics.Vector3 vsg, out System.Numerics.Quaternion vrg, out System.Numerics.Vector3 global);
                Vector3 sourcse = new Vector3(global.X, global.Y, global.Z);
                Vector3 destination = new Vector3(val.X, val.Y, val.Z);
                using (var cb = new ClosestRayResultCallback(ref sourcse, ref destination))
                {
                    cb.Flags = 0xFFFFFFFF;
                    world.physicsWorld.RayTest(sourcse, destination, cb);
                    if (cb.HasHit)
                    {
                        try
                        {
                            var inputPlane = ((InputPlane)cb.CollisionObject.UserObject);
                            System.Numerics.Matrix4x4.Decompose(inputPlane.entity.globalTrans(), out System.Numerics.Vector3 scale, out System.Numerics.Quaternion rotation, out System.Numerics.Vector3 translation);
                            var size = inputPlane.size.value;
                            var pixsize = inputPlane.pixelSize.value;

                            var hit = cb.HitPointWorld;
                            var hitnormal = cb.HitNormalWorld;

                            var stepone = ((hit - new Vector3(translation.X, translation.Y, translation.Z)) / new Vector3(scale.X, scale.Y, scale.Z));
                            var steptwo = System.Numerics.Matrix4x4.CreateScale(1) * System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3((float)stepone.X, (float)stepone.Y, (float)stepone.Z));
                            var stepthree = System.Numerics.Matrix4x4.CreateScale(1) * System.Numerics.Matrix4x4.CreateFromQuaternion(System.Numerics.Quaternion.Inverse(rotation));
                            var stepfour = (steptwo * stepthree);
                            System.Numerics.Matrix4x4.Decompose(stepfour, out System.Numerics.Vector3 scsdale, out System.Numerics.Quaternion rotatdsion, out System.Numerics.Vector3 trans);
                            var nonescaleedpos = new Vector2f(trans.X, -trans.Z);
                            var posnopixs = ((nonescaleedpos * (1 / size))/2) + 0.5f;
                            var pospix = posnopixs * new Vector2f(pixsize.x, pixsize.y );

                            var pos = new System.Numerics.Vector2(pospix.x, pospix.y);
                            inputPlane.updatePos(pos, source.value);
                            if (HasClicked())
                            {
                                inputPlane.Click(source.value);
                            }
                        }
                        catch
                        {
                            if (HasClicked())
                            {
                                input.RemoveFocus();
                            }
                        }

                    }
                    else
                    {
                        if (HasClicked())
                        {
                            input.RemoveFocus();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log("Error With Interaction :" + e.ToString(), true);
            }

        }

        public bool HasClicked()
        {
            switch (source.value)
            {
                case InteractionSource.None:
                    break;
                case InteractionSource.LeftLaser:
                    return input.TriggerTouching(Input.Creality.Left) | input.SecondaryPress(Input.Creality.Left) | input.PrimaryPress(Input.Creality.Left);
                    break;
                case InteractionSource.LeftFinger:
                    break;
                case InteractionSource.RightLaser:
                    return input.TriggerTouching(Input.Creality.Right) | input.SecondaryPress(Input.Creality.Right) | input.PrimaryPress(Input.Creality.Right);
                    break;
                case InteractionSource.RightFinger:
                    break;
                case InteractionSource.HeadLaser:
                    return engine.inputManager.mainWindows.GetMouseButtonDown(MouseButton.Right)| engine.inputManager.mainWindows.GetMouseButtonDown(MouseButton.Left) | engine.inputManager.mainWindows.GetMouseButtonDown(MouseButton.Middle);
                    break;
                case InteractionSource.HeadFinger:
                    break;
                default:
                    break;
            }
            return false;
        }

        public InteractionLaser(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public InteractionLaser()
        {
        }
    }
}
