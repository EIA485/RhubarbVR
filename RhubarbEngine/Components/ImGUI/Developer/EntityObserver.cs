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
using System.Threading;

namespace RhubarbEngine.Components.ImGUI
{


	[Category("ImGUI/Developer")]
	public class EntityObserver : UIWidget, IObserver
	{

		public SyncRef<Entity> target;

		public SyncRefList<IObserver> children;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			target = new SyncRef<Entity>(this, newRefIds);
			target.Changed += Target_Changed;
			children = new SyncRefList<IObserver>(this, newRefIds);
		}

		private void Target_Changed(IChangeable obj)
		{
			if (entity.Manager != world.LocalUser)
            {
                return;
            }

            var e = new Thread(BuildView, 1024)
            {
                Priority = ThreadPriority.BelowNormal
            };
            e.Start();
		}
		private void ClearOld()
		{
			foreach (var item in children)
			{
				item.Target?.Dispose();
			}
			children.Clear();
		}
		public EntityObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public EntityObserver()
		{
		}
		[NoSave]
		[NoShow]
		[NoSync]
		Entity _e;

		private void BuildView()
		{
			try
			{
				ClearOld();
				if (target.Target == null)
                {
                    return;
                }

                var fields = typeof(Entity).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				//This is a temp fix
				if (_e == null)
				{
					_e = entity.AddChild("Entity Children");
					_e.persistence.Value = false;
				}
				//I should remove on change update before initialized or add a on initialized check inside this function
				foreach (var field in fields)
				{
					if (typeof(Worker).IsAssignableFrom(field.FieldType) && (field.GetCustomAttributes(typeof(NoShowAttribute), false).Length <= 0))
					{
						var obs = _e.AttachComponent<WorkerObserver>();
						obs.fieldName.Value = field.Name;
						obs.target.Target = ((Worker)field.GetValue(target.Target));
						children.Add().Target = obs;
					}
				}
			}
			catch { }
		}

		public override void Dispose()
		{
			base.Dispose();
			if (world.lastEntityObserver == this)
			{
				world.lastEntityObserver = null;
			}
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			Interaction.GrabbableHolder source = null;
			switch (canvas.imputPlane.Target?.source ?? Interaction.InteractionSource.None)
			{
				case Interaction.InteractionSource.LeftLaser:
					source = world.LeftLaserGrabbableHolder;
					break;
				case Interaction.InteractionSource.RightLaser:
					source = world.RightLaserGrabbableHolder;
					break;
				case Interaction.InteractionSource.HeadLaser:
					source = world.HeadLaserGrabbableHolder;
					break;
				default:
					break;
			}
			ImGui.Text($"{target.Target?.name.Value ?? "null"} ID:({target.Target?.referenceID.id.ToHexString() ?? "null"})");
			if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
			{
				if (source != null)
				{
					source.Referencer.Target = target.Target;
				}
			}
			ImGui.SameLine();
			if (ImGui.Button("X##" + referenceID.id.ToString()))
			{
				var e = target.Target?.parent.Target;
				target.Target?.Destroy();
				target.Target = e;
			}
			ImGui.SameLine();
			if (ImGui.Button("+##" + referenceID.id.ToString()))
			{
				var e = target.Target?.AddChild();
				if (e != null)
                {
                    target.Target = e;
                }
            }
			ImGui.SameLine();
			if (ImGui.ArrowButton(referenceID.id.ToString(), ImGuiDir.Up))
			{
				var c = target.Target.parent.Target.AddChild(target.Target.name.Value + "Parent");
				if (target.Target != null)
                {
                    target.Target.parent.Target = c;
                }

                if (c != null)
                {
                    target.Target = c;
                }
            }
			foreach (var item in children)
			{
				item.Target?.ImguiRender(imGuiRenderer, canvas);
			}
			ImGui.EndChild();
			if (ImGui.IsMouseClicked(ImGuiMouseButton.COUNT))
			{
				world.lastEntityObserver = this;
			}
		}
	}
}
