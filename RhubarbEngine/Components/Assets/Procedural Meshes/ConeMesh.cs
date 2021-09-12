﻿using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class ConeMesh : ProceduralMesh
	{
		private readonly ConeGenerator _generator = new ConeGenerator();

		public Sync<float> BaseRadius;
		public Sync<float> Height;
		public Sync<float> StartAngleDeg;
		public Sync<float> EndAngleDeg;
		public Sync<int> Slices;

		public Sync<bool> NoSharedVertices;

		public override void buildSyncObjs(bool newRefIds)
		{
			BaseRadius = new Sync<float>(this, newRefIds);
			BaseRadius.Value = 1f;
			Height = new Sync<float>(this, newRefIds);
			Height.Value = 1f;
			StartAngleDeg = new Sync<float>(this, newRefIds);
			StartAngleDeg.Value = 0.0f;
			EndAngleDeg = new Sync<float>(this, newRefIds);
			EndAngleDeg.Value = 360f;
			Slices = new Sync<int>(this, newRefIds);
			Slices.Value = 16;

			NoSharedVertices = new Sync<bool>(this, newRefIds);
		}
		public override void onChanged()
		{
			updateMesh();
		}

		private void updateMesh()
		{
			_generator.BaseRadius = BaseRadius.Value;
			_generator.Height = Height.Value;
			_generator.StartAngleDeg = StartAngleDeg.Value;
			_generator.EndAngleDeg = EndAngleDeg.Value;
			_generator.Slices = Slices.Value;
			_generator.NoSharedVertices = NoSharedVertices.Value;
			MeshGenerator newmesh = _generator.Generate();
			RMesh kite = new RMesh(newmesh.MakeDMesh());
			kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
			load(kite, true);
		}
		public override void onLoaded()
		{
			updateMesh();
		}
		public ConeMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ConeMesh()
		{
		}
	}
}
