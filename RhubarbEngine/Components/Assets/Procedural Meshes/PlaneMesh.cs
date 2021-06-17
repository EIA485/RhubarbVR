﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

// TODO
namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{
    public class PlaneMesh : ProceduralMesh
    {
        private TrivialRectGenerator planeGen = new TrivialRectGenerator();

        public Sync<float> Width;
        public Sync<float> Height;
        public Sync<Vector3f> Normal;
        public Sync<Index2i> IndicesMap;
        public Sync<TrivialRectGenerator.UVModes> UVMode;

        public override void buildSyncObjs(bool newRefIds)
        {
            Width = new Sync<float>(this, newRefIds);
            Width.value = 1.0f;
            Height = new Sync<float>(this, newRefIds);
            Height.value = 1.0f;
            Normal = new Sync<Vector3f>(this, newRefIds);
            Normal.value = Vector3f.AxisY;
            IndicesMap = new Sync<Index2i>(this, newRefIds);
            IndicesMap.value = new Index2i(1, 3);
            UVMode = new Sync<TrivialRectGenerator.UVModes>(this, newRefIds);
            UVMode.value = TrivialRectGenerator.UVModes.FullUVSquare;
        }
        public override void onChanged()
        {
            planeGen.Width = Width.value;
            planeGen.Height = Height.value;
            planeGen.Normal = Normal.value;
            planeGen.IndicesMap = IndicesMap.value;
            planeGen.UVMode = UVMode.value;
            updateMesh();
        }

        private void updateMesh()
        {
            MeshGenerator newmesh = planeGen.Generate();
            RMesh kite = new RMesh(newmesh.MakeSimpleMesh());
            kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
            load(kite);
        }
        public override void onLoaded()
        {
            updateMesh();
        }
        public PlaneMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public PlaneMesh()
        {
        }


    }
}
