﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine;
using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World.DataStructure;
using RhubarbEngine.Render;
using g3;
using RhubarbEngine.Components.Transform;
using RhubarbEngine.World.Asset;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Color;
using RhubarbEngine.Components.Users;

using Org.OpenAPITools.Model;

namespace RhubarbEngine.Managers
{
    public class WorldManager : IManager
    {
        public Engine engine;

        public List<World.World> worlds = new List<World.World>();

        public World.World privateOverlay;

        public World.World localWorld;

        public World.World focusedWorld;

        public bool dontSaveLocal = false;
        public void addToRenderQueue(RenderQueue gu, RemderLayers layer)
        {
            foreach(World.World world in worlds)
            {
                if (world.Focus != World.World.FocusLevel.Background)
                {
                    world.addToRenderQueue(gu, layer);
                }
            }
        }

        public void createNewWorld(AccessLevel accessLevel, SessionsType sessionsType, string name, string worlduuid, bool isOver, int maxusers, bool mobilefriendly,string templet = null, bool focus = true)
        {
            Logger.Log("Creating world", true);
            World.World world = new World.World(this, sessionsType, accessLevel, name, maxusers, worlduuid, isOver, mobilefriendly, templet);
            string ip = LiteNetLib.NetUtils.GetLocalIp(LiteNetLib.LocalAddrType.IPv4);
            string conectionkey = ip + " _ " + world.port;
            try
            {
                string sessionid = engine.netApiManager.sessionApi.SessionCreatesessionPost(new CreateSessionReq(name, worlduuid, new List<string>(new[] { "" }), "", (int)sessionsType, (int)accessLevel, isOver, maxusers, mobilefriendly, conectionkey), engine.netApiManager.token);
                world.SessionID.value = sessionid;
                world.loadHostUser();
                worlds.Add(world);
                if (focus)
                {
                    world.Focus = World.World.FocusLevel.Focused;
                }
                Logger.Log("World Created sessionID: "+sessionid, true);

            }
            catch (Exception e)
            {
                Logger.Log("Create World Error"+e.ToString(), true);
            }

        }

        public void JoinSessionFromUUID(string uuid, bool focus = true)
        {
            try
            {
                Logger.Log("Joining session ID: " + uuid, true);
                World.World world = new World.World(this, "Loading", 1);
                string ip = LiteNetLib.NetUtils.GetLocalIp(LiteNetLib.LocalAddrType.IPv4);
                string conectionkey = ip + " _ " + world.port;
                var join = engine.netApiManager.sessionApi.SessionJoinsessionGet(uuid, conectionkey, engine.netApiManager.token);
                world.SessionID.value = join.Uuid;
                world.joinsession(join,conectionkey);
                worlds.Add(world);
            if (focus)
            {
                world.Focus = World.World.FocusLevel.Focused;
            }
        }
            catch (Exception e)
            {
                Logger.Log("Create World Error"+e.ToString(), true);
            }
}

        public World.World loadWorldFromBytes(byte[] data)
        {
            DataNodeGroup node = new DataNodeGroup(data);
            World.World tempWorld = new World.World(this, node, false);
            return tempWorld;
        }
        public byte[] worldToBytes(World.World world)
        {
            byte[] val = new byte[] { };
            if (focusedWorld != null)
            {
                DataNodeGroup node = world.serialize();
                val = node.getByteArray();
            }
            return val;
        }

        public byte[] focusedWorldToBytes()
        {
            return worldToBytes(focusedWorld);
        }

        public IManager initialize(Engine _engine)
        {
            engine = _engine;

            engine.logger.Log("Starting Private Overlay World");
            privateOverlay = new World.World(this, "Private Overlay", 1);
            privateOverlay.Focus = World.World.FocusLevel.PrivateOverlay;
            worlds.Add(privateOverlay);


            engine.logger.Log("Starting Local World");
            if(File.Exists(engine.dataPath + "/LocalWorld.RWorld"))
            {
                try
                {
                    DataNodeGroup node = new DataNodeGroup(File.ReadAllBytes(engine.dataPath + "/LocalWorld.RWorld"));
                    localWorld = new World.World(this, "LocalWorld", 16, false, true,node);
                }
                catch (Exception e)
                {
                    dontSaveLocal = true;
                    Logger.Log("Failed To load LocalWorld" + e.ToString(), true);
                    localWorld = new World.World(this, "TempLoaclWorld", 16, false, true);
                    BuildLocalWorld();
                }
            }
            else
            {
                localWorld = new World.World(this, "LocalWorld", 16,false,true);
                BuildLocalWorld();
            }
            localWorld.Focus = World.World.FocusLevel.Focused;
            worlds.Add(localWorld);
            focusedWorld = localWorld;
            if(engine.engineInitializer.session != null)
            {
                JoinSessionFromUUID(engine.engineInitializer.session, true);
            }
            else
            {
                createNewWorld(AccessLevel.Anyone, SessionsType.Casual, "Faolan World", "", false, 16, false, "Basic");
            }
            return this;
        }

        public void BuildLocalWorld()
        {
            Entity e = localWorld.RootEntity.addChild("Gay");
            localWorld.RootEntity.attachComponent<SimpleSpawn>();
            StaicMainShader shader = e.attachComponent<StaicMainShader>();
            RevolveMesh bmesh = e.attachComponent<RevolveMesh>();
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            RGBRainbowDriver rgbainbowDriver = e.attachComponent<RGBRainbowDriver>();

            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            mit.setValueAtField("rambow", Render.Shader.ShaderType.MainFrag, Colorf.Blue);
            Render.Material.Fields.ColorField field = mit.getField<Render.Material.Fields.ColorField>("rambow", Render.Shader.ShaderType.MainFrag);
            rgbainbowDriver.driver.setDriveTarget(field.field);
            rgbainbowDriver.speed.value = 50f;

            e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
            e.scale.value = new Vector3f(1f);
            Entity ea = localWorld.RootEntity.addChild("Gayer");
            ea.position.value = Vector3f.One;
            AddMesh<BoxMesh>(ea);
        }

        public Entity AddMesh<T>(Entity ea) where T: ProceduralMesh
        {
            Entity e = ea.addChild();
            StaicMainShader shader = e.attachComponent<StaicMainShader>();
            T bmesh = e.attachComponent<T>();
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            RGBRainbowDriver rgbainbowDriver = e.attachComponent<RGBRainbowDriver>();

            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            mit.setValueAtField("rambow", Render.Shader.ShaderType.MainFrag, Colorf.Blue);
            Render.Material.Fields.ColorField field = mit.getField<Render.Material.Fields.ColorField>("rambow", Render.Shader.ShaderType.MainFrag);
            rgbainbowDriver.driver.setDriveTarget(field.field);
            rgbainbowDriver.speed.value = 50f;
            return e;
        }

        public void Update(DateTime startTime, DateTime Frame)
        {
            foreach (World.World world in worlds)
            {
                world.Update(startTime, Frame);
            }
        }
        public void CleanUp()
        {
            if (engine.engineInitializer == null && !dontSaveLocal)
            {
                File.WriteAllBytes(engine.dataPath + "/LocalWorld.RWorld", worldToBytes(localWorld));
            }
        }
    }
}
