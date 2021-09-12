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
using RNumerics;
using Veldrid;
using RhubarbEngine.Helpers;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.Transform;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.World.Asset;

namespace RhubarbEngine.Components.PrivateSpace
{
	public class LaserVisual : AssetProvider<RTexture2D>
	{

		public Sync<InteractionSource> source;

		public SyncRef<Entity> Currsor;
		public SyncRef<Entity> Laser;
		public SyncRef<CurvedTubeMesh> LaserMesh;

		public SyncRef<Render.Material.Fields.ColorField> colorField;
		public SyncRef<Render.Material.Fields.ColorField> planeColorField;

		private bool _bind;

		public override void OnAttach()
		{
			base.OnAttach();
			var (curs, _, cmit) = MeshHelper.AddMesh<PlaneMesh>(entity, world.staticAssets.overLayedUnlitShader, "Currsor", 0);
			var (Lasere, lmesh, mit) = MeshHelper.AddMesh<CurvedTubeMesh>(entity, world.staticAssets.overLayedUnlitShader, "Laser", 3);
			Laser.Target = Lasere;
			Lasere.rotation.Value = Quaternionf.CreateFromEuler(0f, -90f, 0f);
			LaserMesh.Target = lmesh;
			Currsor.Target = curs;
			var look = curs.AttachComponent<LookAtUser>();
			look.offset.Value = Quaternionf.CreateFromEuler(0f, 90f, 0f);
			var scaler = curs.AttachComponent<DistanceScaler>();
			scaler.scale.Value = 0.025f;
			scaler.pow.Value = 0.5;
			scaler.offset.Value = new Vector3f(-0.1);
			var pos = cmit.GetField<Render.Material.Fields.FloatField>("Zpos", Render.Shader.ShaderType.MainFrag);
			var pos2 = mit.GetField<Render.Material.Fields.FloatField>("Zpos", Render.Shader.ShaderType.MainFrag);
			pos.field.Value = 0.1f;
			pos2.field.Value = 0.2f;

			colorField.Target = mit.GetField<Render.Material.Fields.ColorField>("TintColor", Render.Shader.ShaderType.MainFrag);
			planeColorField.Target = cmit.GetField<Render.Material.Fields.ColorField>("TintColor", Render.Shader.ShaderType.MainFrag);
			var textureField = cmit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
			textureField.field.Target = this;
		}

		public override void buildSyncObjs(bool newRefIds)
		{
            source = new Sync<InteractionSource>(this, newRefIds)
            {
                Value = InteractionSource.HeadLaser
            };
            Currsor = new SyncRef<Entity>(this, newRefIds);
			Laser = new SyncRef<Entity>(this, newRefIds);
			LaserMesh = new SyncRef<CurvedTubeMesh>(this, newRefIds);
			colorField = new SyncRef<Render.Material.Fields.ColorField>(this, newRefIds);
			planeColorField = new SyncRef<Render.Material.Fields.ColorField>(this, newRefIds);
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
			if (Currsor.Target == null)
            {
                return;
            }

            var pos = Vector3d.Zero;
			var left = false;
			switch (source.Value)
			{
				case InteractionSource.LeftLaser:
					pos = input.LeftLaser.pos;
					left = true;
					break;
				case InteractionSource.RightLaser:
					pos = input.RightLaser.pos;
					break;
				case InteractionSource.HeadLaser:
					pos = input.RightLaser.pos;
					break;
				default:
					break;
			}
			if (!_bind)
			{
				if (left)
				{
					input.LeftLaser.cursorChange += UpdateCursor;
				}
				else
				{
					input.RightLaser.cursorChange += UpdateCursor;
				}
				_bind = true;
			}
			var hitvector = Vector3d.Zero;
			switch (source.Value)
			{
				case InteractionSource.LeftLaser:
					hitvector = input.LeftLaser.normal;
					break;
				case InteractionSource.RightLaser:
					hitvector = input.RightLaser.normal;
					break;
				case InteractionSource.HeadLaser:
					hitvector = input.RightLaser.normal;
					break;
				default:
					break;
			}
			var newpos = new Vector3f(pos.x, pos.y, pos.z);
			Currsor.Target.SetGlobalPos(newpos);
			if (LaserMesh.Target == null)
            {
                return;
            }

            if (Laser.Target == null)
            {
                return;
            }

            var mesh = LaserMesh.Target;
			mesh.Endpoint.Value = Laser.Target.GlobalPointToLocal(newpos);
			var val = entity.GlobalPos().Distance(new Vector3f(pos.x, pos.y, pos.z));
			mesh.StartHandle.Value = Vector3d.AxisY * (val / 4);
			var e = Laser.Target.GlobalRot().Inverse() * new Vector3f(hitvector.x, hitvector.y, hitvector.z);
			mesh.EndHandle.Value = e * (val / 6);
			switch (source.Value)
			{
				case InteractionSource.LeftLaser:
					Currsor.Target.enabled.Value = input.LeftLaser.isvisible;
					Laser.Target.enabled.Value = input.LeftLaser.isvisible;
					break;
				case InteractionSource.RightLaser:
					Currsor.Target.enabled.Value = input.RightLaser.isvisible;
					Laser.Target.enabled.Value = input.RightLaser.isvisible;
					break;
				case InteractionSource.HeadLaser:
					Currsor.Target.enabled.Value = input.RightLaser.isvisible;
					Laser.Target.enabled.Value = input.RightLaser.isvisible;
					break;
				default:
					break;
			}
		}

		private void UpdateCursor(Input.Cursors newcursor)
		{
			var color = new Colorf(1f, 0.7f, 1f, 0.7f);
			switch (newcursor)
			{
				case Input.Cursors.Grabbing:
					color = new Colorf(0.7f, 0.7f, 1f, 0.7f);
					break;
				default:
					break;
			}
			if (colorField.Target != null)
            {
                colorField.Target.field.Value = color;
            }

            if (planeColorField.Target != null)
            {
                planeColorField.Target.field.Value = color;
            }

            load(new RTexture2D(engine.renderManager.cursors[(int)newcursor]));
		}

		public override void onLoaded()
		{
			base.onLoaded();
			UpdateCursor(Input.Cursors.None);
		}

		public LaserVisual(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public LaserVisual()
		{
		}

	}
}
