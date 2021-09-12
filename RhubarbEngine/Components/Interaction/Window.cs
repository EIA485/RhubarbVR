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
using RhubarbEngine.Components.Physics;
using RhubarbEngine.Components.PrivateSpace;
using RhubarbEngine.Components.Interaction;
using Veldrid;
using ImGuiNET;

namespace RhubarbEngine.Components.Interaction
{
	public class Window : UIWidget
	{
		public enum DockPos
		{
			Up,
			Down,
			Left,
			Right
		}

		public SyncRef<IUIElement> element;

		public SyncRef<ImGUICanvas> canvas;

		public Sync<Vector2f> size;

		public Sync<uint> pixelDensity;

		public Driver<string> labelDriver;
		public Driver<float> meshWidth;
		public Driver<float> meshHeight;
		public Driver<Vector2f> colDriver;
		public Driver<Vector3d> BackGround;
		public Driver<Vector3f> colBackGround;

		public Driver<Vector2u> colPixelsizeDriver;
		public Driver<Vector2u> canvasPixelsizeDriver;
		public Driver<bool> renderEnableDriver;

		public Sync<DockPos> ChildDocPos;

		public SyncRef<Window> ChildDock;
		public SyncRef<Window> ParentDock;

		public void Dock(Window window, DockPos pos)
		{
			if (world.grabedWindow == this)
			{
				world.grabedWindow = null;
			}
			window.ChildDock.Target = this;
			ParentDock.Target = this;
			window.ChildDocPos.Value = pos;
			entity.SetParent(window.entity, false, true);
			sizeUpdate();
		}

		public void unDock()
		{
			if (ParentDock.Target == null)
				return;
			entity.SetParent(ParentDock.Target.entity.parent.Target, false, true);
			sizeUpdate();
		}

		private bool clapsed;

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			var pos = ImGui.GetCursorPos();
			if ((world.grabedWindow != this) && (world.grabedWindow != null) && (ChildDock.Target == null))
			{
				var size = ImGui.GetWindowSize() / 2;
				ImGui.SetCursorPos(size + new Vector2(-20, 0));
				if (ImGui.ArrowButton("left" + referenceID.ToString(), ImGuiDir.Left))
				{
					world.grabedWindow.Dock(this, DockPos.Left);
				}
				ImGui.SetCursorPos(size + new Vector2(20, 0));
				if (ImGui.ArrowButton("right" + referenceID.ToString(), ImGuiDir.Right))
				{
					world.grabedWindow.Dock(this, DockPos.Right);
				}
				ImGui.SetCursorPos(size + new Vector2(0, -20));
				if (ImGui.ArrowButton("up" + referenceID.ToString(), ImGuiDir.Up))
				{
					world.grabedWindow.Dock(this, DockPos.Up);
				}
				ImGui.SetCursorPos(size + new Vector2(0, 20));
				if (ImGui.ArrowButton("down" + referenceID.ToString(), ImGuiDir.Down))
				{
					world.grabedWindow.Dock(this, DockPos.Down);
				}
				ImGui.SetCursorPos(pos);
			}
			if (ChildDock.Target == null)
			{
				element.Target?.ImguiRender(imGuiRenderer, canvas);
			}
			else
			{
				float titleBarHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2.0f;
				float framePad = (ImGui.GetStyle().FramePadding.Y * 2.0f) * 8f;
				var size = ImGui.GetWindowSize();
				var parentsize = new Vector2();
				bool flip = false;
				switch (ChildDocPos.Value)
				{
					case DockPos.Up:
						parentsize = new Vector2(size.X, (clapsed) ? ((size.Y - (titleBarHeight + framePad))) : ((size.Y - (titleBarHeight + framePad)) / 2));
						break;
					case DockPos.Down:
						parentsize = new Vector2(size.X, (clapsed) ? ((size.Y - (titleBarHeight + framePad))) : ((size.Y - (titleBarHeight + framePad)) / 2));
						flip = true;
						break;
					case DockPos.Left:
						parentsize = new Vector2((clapsed) ? size.X : (size.X / 2), size.Y - (titleBarHeight * 2));
						flip = true;
						break;
					case DockPos.Right:
						parentsize = new Vector2((clapsed) ? size.X : (size.X / 2), size.Y - (titleBarHeight * 2));
						break;
					default:
						break;
				}
				if (flip)
				{
					ImGui.SetCursorPos(pos);
					if (ImGui.BeginChildFrame((uint)referenceID.id, parentsize))
					{
						element.Target?.ImguiRender(imGuiRenderer, canvas);
						ImGui.EndChildFrame();
					}
					bool e = true;
					if ((int)ChildDocPos.Value <= 2)
					{
						ImGui.SetCursorPos(pos + new Vector2(0f, parentsize.Y));
					}
					else
					{
						if (clapsed)
						{
							ImGui.SetCursorPos(pos + new Vector2(parentsize.X / 2, framePad * 2f));
						}
						else
						{
							ImGui.SetCursorPos(pos + new Vector2(parentsize.X, framePad * 2f));
						}
						ImGui.SetNextItemWidth((size.X / 2));
					}
					if (ImGui.CollapsingHeader(ChildDock.Target.entity.name.Value ?? "Null", ref e, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.AllowItemOverlap))
					{
						clapsed = false;
						if (ImGui.BeginChildFrame((uint)referenceID.id + 1, parentsize))
						{
							ChildDock.Target?.ImguiRender(imGuiRenderer, canvas);
							ImGui.EndChildFrame();
						}
					}
					else
					{
						clapsed = true;
					}
					if (!e)
					{
						ChildDock.Target.Close();
					}
				}
				else
				{
					if ((int)ChildDocPos.Value < 2)
					{
						ImGui.SetCursorPos(pos + new Vector2(0f, parentsize.Y + titleBarHeight));
					}
					else
					{
						ImGui.SetCursorPos(pos + new Vector2(parentsize.X, titleBarHeight));
					}
					if (clapsed)
					{
						ImGui.SetCursorPos(pos + new Vector2(0f, titleBarHeight));
					}
					if (ImGui.BeginChildFrame((uint)referenceID.id, parentsize))
					{
						element.Target?.ImguiRender(imGuiRenderer, canvas);
						ImGui.EndChildFrame();
					}
					ImGui.SetCursorPos(pos);
					bool e = true;
					ImGui.SetNextItemWidth(parentsize.X);
					if (ImGui.CollapsingHeader(ChildDock.Target.entity.name.Value ?? "Null", ref e, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.AllowItemOverlap))
					{
						ImGui.SetNextItemWidth(parentsize.X);
						clapsed = false;
						if (ImGui.BeginChildFrame((uint)referenceID.id + 1, parentsize))
						{
							ChildDock.Target?.ImguiRender(imGuiRenderer, canvas);
							ImGui.EndChildFrame();
						}
					}
					else
					{
						clapsed = true;
					}
					if (!e)
					{
						ChildDock.Target.Close();
					}
				}

			}

		}

		private void sizeUpdate()
		{
			if (renderEnableDriver.Linked)
			{
				renderEnableDriver.Drivevalue = ParentDock.Target == null;
			}
			if (colBackGround.Linked)
			{
				colBackGround.Drivevalue = new Vector3f(size.Value.x / 2, 0.01f, size.Value.y / 2);
			}
			if (BackGround.Linked)
			{
				BackGround.Drivevalue = new Vector3f(size.Value.x / 2, 0.01f, size.Value.y / 2);
			}
			if (labelDriver.Linked)
			{
				labelDriver.Drivevalue = entity.name.Value;
			}
			if (meshWidth.Linked)
			{
				meshWidth.Drivevalue = size.Value.x;
			}
			if (meshHeight.Linked)
			{
				meshHeight.Drivevalue = size.Value.y;
			}
			if (colDriver.Linked)
			{
				colDriver.Drivevalue = size.Value / 2;
			}
			Vector2u pixsize = new Vector2u((uint)(size.Value.x * pixelDensity.Value), (uint)(size.Value.y * pixelDensity.Value));
			if (colPixelsizeDriver.Linked)
			{
				colPixelsizeDriver.Drivevalue = pixsize;
			}
			if (canvasPixelsizeDriver.Linked)
			{
				canvasPixelsizeDriver.Drivevalue = pixsize;
			}
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
		}

		private void AttachBackGround()
		{
			entity.AttachComponent<Grabbable>();
			var col = entity.AttachComponent<BoxCollider>();
			var (e, mesh, mit) = Helpers.MeshHelper.AddMesh<BoxMesh>(entity, world.staticAssets.basicUnlitShader, "UIBackGround", 2147483646);
			BackGround.SetDriveTarget(mesh.Extent);
			colBackGround.SetDriveTarget(col.boxExtents);
		}

		public override void onLoaded()
		{
			base.onLoaded();
			entity.OnDrop += Entity_onDrop;
		}

		private void Entity_onDrop(GrabbableHolder obj, bool laser)
		{
			world.grabedWindow = null;
		}

		private void OnGrabHeader()
		{
			if (ParentDock.Target != null)
			{
				unDock();
			}
			foreach (var grab in entity.GetAllComponents<Grabbable>())
			{
				grab.RemoteGrab();
			}
			world.grabedWindow = this;
		}

		public override void OnAttach()
		{
			base.OnAttach();
			PlaneMesh mesh = entity.AttachComponent<PlaneMesh>();
			meshWidth.SetDriveTarget(mesh.Width);
			meshHeight.SetDriveTarget(mesh.Height);
			var UIRender = entity.AddChild("UIRender");
			renderEnableDriver.SetDriveTarget(UIRender.enabled);
			AttachBackGround();
			InputPlane col = UIRender.AttachComponent<InputPlane>();
			colDriver.SetDriveTarget(col.size);
			colPixelsizeDriver.SetDriveTarget(col.pixelSize);
			RMaterial mit = entity.AttachComponent<RMaterial>();
			MeshRender meshRender = UIRender.AttachComponent<MeshRender>();
			UIRender.position.Value = new Vector3f(0f, -0.012f, 0f);
			ImGUICanvas imGUICanvas = UIRender.AttachComponent<ImGUICanvas>();
			imGUICanvas.onClose.Target = Close;
			imGUICanvas.imputPlane.Target = col;
			imGUICanvas.scale.Value = new Vector2u(300);
			imGUICanvas.onHeaderGrab.Target = OnGrabHeader;
			labelDriver.SetDriveTarget(imGUICanvas.name);
			imGUICanvas.element.Target = this;
			canvasPixelsizeDriver.SetDriveTarget(imGUICanvas.scale);
			mit.Shader.Target = world.staticAssets.basicUnlitShader;
			canvas.Target = imGUICanvas;
			Render.Material.Fields.Texture2DField field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
			field.field.Target = imGUICanvas;
			meshRender.Materials.Add().Target = mit;
			meshRender.Mesh.Target = mesh;
			sizeUpdate();
		}

		public void Close()
		{
			entity.Destroy();
		}

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			element = new SyncRef<IUIElement>(this, newRefIds);
			canvas = new SyncRef<ImGUICanvas>(this, newRefIds);
			size = new Sync<Vector2f>(this, newRefIds);
			size.Value = new Vector2f(1, 1.5);
			size.Changed += Size_Changed;
			pixelDensity = new Sync<uint>(this, newRefIds);
			pixelDensity.Value = 450;
			meshWidth = new Driver<float>(this, newRefIds);
			meshHeight = new Driver<float>(this, newRefIds);
			colDriver = new Driver<Vector2f>(this, newRefIds);
			colPixelsizeDriver = new Driver<Vector2u>(this, newRefIds);
			canvasPixelsizeDriver = new Driver<Vector2u>(this, newRefIds);
			labelDriver = new Driver<string>(this, newRefIds);
			BackGround = new Driver<Vector3d>(this, newRefIds);
			colBackGround = new Driver<Vector3f>(this, newRefIds);
			renderEnableDriver = new Driver<bool>(this, newRefIds);
			ChildDocPos = new Sync<DockPos>(this, newRefIds);
			ChildDock = new SyncRef<Window>(this, newRefIds);
			ParentDock = new SyncRef<Window>(this, newRefIds);
		}

		private void Size_Changed(IChangeable obj)
		{
			sizeUpdate();
		}

		public Window(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public Window()
		{
		}
	}
}
