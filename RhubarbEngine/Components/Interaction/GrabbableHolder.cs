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
using RhubarbEngine.Components.Transform;
using RhubarbEngine.World.Asset;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Color;
using RhubarbEngine.Components.Users;
using RhubarbEngine.Components.ImGUI;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Components.PrivateSpace;
using RhubarbEngine.Components.Interaction;
using Veldrid;

namespace RhubarbEngine.Components.Interaction
{
	public class GrabbableHolder : Component
	{
		public SyncRef<Entity> holder;

		public SyncRef<IWorldObject> Referencer;

		public SyncRef<User> user;

		public Sync<InteractionSource> source;

		bool gripping = false;

		public void initializeGrabHolder(InteractionSource _source)
		{
			user.target = world.localUser;
			source.value = _source;
			switch (_source)
			{
				case InteractionSource.None:
					break;
				case InteractionSource.LeftLaser:
					world.LeftLaserGrabbableHolder = this;
					break;
				case InteractionSource.LeftFinger:
					break;
				case InteractionSource.RightLaser:
					world.RightLaserGrabbableHolder = this;
					break;
				case InteractionSource.RightFinger:
					break;
				case InteractionSource.HeadLaser:
					world.HeadLaserGrabbableHolder = this;
					break;
				case InteractionSource.HeadFinger:
					break;
				default:
					break;
			}
		}

		public bool DropedRef => (timeout <= 16) && (timeout != 0) && !gripping;

		byte timeout = 0;
		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
			if (user.target == null)
				return;
			if (holder.target == null)
				return;
			if (user.target != world.localUser)
				return;
			if (source.value == InteractionSource.HeadLaser)
			{
				var mousepos = engine.inputManager.mainWindows.MousePosition;
				var size = new System.Numerics.Vector2(engine.windowManager.mainWindow.width, engine.windowManager.mainWindow.height);
				float x = 2.0f * mousepos.X / size.X - 1.0f;
				float y = 2.0f * mousepos.Y / size.Y - 1.0f;
				float ar = size.X / size.Y;
				float tan = (float)Math.Tan(engine.settingsObject.RenderSettings.DesktopRenderSettings.fov * Math.PI / 360);
				Vector3f vectforward = new Vector3f(-x * tan * ar, y * tan, 1);
				Vector3f vectup = new Vector3f(0, 1, 0);
				holder.target.rotation.value = Quaternionf.LookRotation(vectforward, vectup);
			}
			if (onGriping() != gripping)
			{
				gripping = !gripping;
				if (!gripping)
				{

					foreach (var child in holder.target._children.getCopy())
					{
						foreach (var grab in child.getAllComponents<Grabbable>())
						{
							grab.Drop();
						}
					}
					switch (source.value)
					{
						case InteractionSource.LeftLaser:
							input.LeftLaser.unLock();
							break;
						case InteractionSource.RightLaser:
							input.RightLaser.unLock();
							break;
						case InteractionSource.HeadLaser:
							input.RightLaser.unLock();
							break;
						default:
							break;
					}
				}
				else
				{
					world.lastHolder = this;
				}
			}
			if (Referencer.target == null)
				return;
			if (!gripping)
			{
				timeout++;
				if (timeout > 16)
				{
					Referencer.target = null;
				}
			}
		}

		private bool onGriping()
		{
			if ((engine.outputType == VirtualReality.OutputType.Screen) && (source.value == InteractionSource.RightLaser))
			{
				return engine.inputManager.mainWindows.GetMouseButton(MouseButton.Right);
			}
			switch (source.value)
			{
				case InteractionSource.None:
					break;
				case InteractionSource.LeftLaser:
					return input.GrabPress(Input.Creality.Left);
				case InteractionSource.LeftFinger:
					break;
				case InteractionSource.RightLaser:
					return input.GrabPress(Input.Creality.Right);
				case InteractionSource.RightFinger:
					break;
				case InteractionSource.HeadLaser:
					return engine.inputManager.mainWindows.GetMouseButton(MouseButton.Right);
				case InteractionSource.HeadFinger:
					break;
				default:
					break;
			}
			return false;
		}

		public override void OnAttach()
		{
			base.OnAttach();
			holder.target = entity.addChild("Holder");
		}

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			holder = new SyncRef<Entity>(this, newRefIds);
			user = new SyncRef<User>(this, newRefIds);
			source = new Sync<InteractionSource>(this, newRefIds);
			Referencer = new SyncRef<IWorldObject>(this, newRefIds);
			Referencer.Changed += Referencer_Changed;
		}

		private void Referencer_Changed(IChangeable obj)
		{
			timeout = 0;
			Console.WriteLine("Changed To " + Referencer.target?.ReferenceID.id.ToHexString());
		}

		public GrabbableHolder(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public GrabbableHolder()
		{
		}
	}
}
