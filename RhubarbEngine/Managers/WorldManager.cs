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
using RhubarbEngine.Components.ImGUI;
using RhubarbEngine.Components.Physics.Colliders;

using Org.OpenAPITools.Model;
using BulletSharp;
using System.Numerics;

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
                World.World world = new World.World(this, "Loading", 1,false,false,null,true);
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
             //   createNewWorld(AccessLevel.Anyone, SessionsType.Casual, "Faolan World", "", false, 16, false, "Basic");
            }
            return this;
        }


        public void BuildLocalWorld()
        {
            localWorld.RootEntity.attachComponent<SimpleSpawn>();
            Entity e = localWorld.RootEntity.addChild("Gay");
            AddMesh<ArrowMesh>(e).position.value = new Vector3f(10f,10f,10f);

            StaicMainShader shader = e.attachComponent<StaicMainShader>();
            PlaneMesh bmesh = e.attachComponent<PlaneMesh>();
            InputPlane bmeshcol = e.attachComponent<InputPlane>();
            //e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
            e.rotation.value = Quaternionf.CreateFromEuler(0f,-90f, 0f);
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            ImGUICanvas imGUICanvas = e.attachComponent<ImGUICanvas>();
            ImGUIInputText imGUIText = e.attachComponent<ImGUIInputText>();
            Textue2DFromUrl urr = e.attachComponent<Textue2DFromUrl>();
            imGUICanvas.imputPlane.target = bmeshcol;
            imGUICanvas.element.target = imGUIText;
            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;

            //mit.setValueAtField("Texture", Render.Shader.ShaderType.MainFrag, textue2DFromUrl);
            
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;
            //  rgbainbowDriver.driver.setDriveTarget(field.field);
            // rgbainbowDriver.speed.value = 50f;

            //e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
            //e.scale.value = new Vector3f(1f);

        }

        public Entity AddMesh<T>(Entity ea) where T: ProceduralMesh
        {
            Entity e = ea.addChild();
            StaicMainShader shader = e.attachComponent<StaicMainShader>();
            T bmesh = e.attachComponent<T>();
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            //Textue2DFromUrl textue2DFromUrl = e.attachComponent<Textue2DFromUrl>();

            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            //Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            //field.field.target = textue2DFromUrl;
            // rgbainbowDriver.driver.setDriveTarget(field.field);
            // rgbainbowDriver.speed.value = 50f;
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
