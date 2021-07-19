﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Veldrid;
using RhubarbEngine.VirtualReality;
using RhubarbEngine.Render;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using Veldrid.ImageSharp;

namespace RhubarbEngine.Managers
{
    public class RenderManager : IManager
    {
        public float fieldOfView = 1f;
        public float aspectRatio => engine.windowManager.mainWindow.aspectRatio;

        public float nearPlaneDistance = 1f;

        public float farPlaneDistance = 1000f;


        private Engine engine;

        private Matrix4x4 _userTrans => (engine.worldManager.focusedWorld != null)?engine.worldManager.focusedWorld.playerTrans: engine.worldManager.privateOverlay.playerTrans;

        private RenderQueue mainQueue;

        public VRContext vrContext;

        public GraphicsDevice gd;

        public Swapchain sc;

        private CommandList eyesCL;

        public CommandList windowCL;

        public Skybox skybox;

        public TextureView nulview;

        public MirrorTextureEyeSource eyeSource = MirrorTextureEyeSource.BothEyes;
        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            GraphicsBackend backend = engine.backend;
            if(engine.platformInfo.platform == PlatformInfo.Platform.OSX)
            {
                backend = GraphicsBackend.Metal;
            }
            if ((backend == GraphicsBackend.Metal && engine.platformInfo.platform != PlatformInfo.Platform.OSX) && engine.platformInfo.platform != PlatformInfo.Platform.iOS)
            {
                backend = GraphicsBackend.Direct3D11;
            }
            if (backend == GraphicsBackend.Direct3D11 && engine.platformInfo.platform != PlatformInfo.Platform.Windows)
            {
                backend = GraphicsBackend.Vulkan;
            }
            engine.logger.Log("Graphics Backend:" + backend, true);
            if (engine.outputType == OutputType.Auto) {
                engine.outputType = OutputType.OculusVR;
                if (!VRContext.IsOculusSupported())
                {
                    engine.outputType = OutputType.SteamVR;
                    if (!VRContext.IsOpenVRSupported())
                    {
                        engine.logger.Log("Failed to find vr device starting in screen");
                        engine.outputType = OutputType.Screen;
                    }
                }
            }
            engine.logger.Log("Output Device:" + engine.outputType.ToString(), true);
            vrContext = buildVRContext();
            (gd, sc) = engine.windowManager.mainWindow.CreateScAndGD(vrContext, backend);
            engine.backend = backend;
            vrContext.Initialize(gd);
            windowCL = gd.ResourceFactory.CreateCommandList();
            eyesCL = gd.ResourceFactory.CreateCommandList();
            mainQueue = new RenderQueue();
            var _texture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "StaticAssets", "nulltexture.jpg"), true, true).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
            nulview = engine.renderManager.gd.ResourceFactory.CreateTextureView(_texture);
            skybox = new Skybox(
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_ft.png")),
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_bk.png")),
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_lf.png")),
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_rt.png")),
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_up.png")),
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_dn.png")));
            skybox.CreateDeviceObjects(gd, vrContext.LeftEyeFramebuffer.OutputDescription);

            return this;
        }

        public VRContext buildVRContext()
        {
            VRContextOptions options = new VRContextOptions
            {
                EyeFramebufferSampleCount = TextureSampleCount.Count4
            };
            switch (engine.outputType)
            {
                case OutputType.Screen:
                    return VRContext.CreateScreen(options,engine);
                    break;
                case OutputType.SteamVR:
                    return VRContext.CreateOpenVR(options);
                    break;
                case OutputType.OculusVR:
                    return VRContext.CreateOculus(options);
                    break;
                default:
                    return VRContext.CreateScreen(options, engine);
                    break;
            }
        }

        private void BuildMainRenderQueue()
        {
            mainQueue.Clear();
            engine.worldManager.addToRenderQueue(mainQueue, RemderLayers.normal_overlay_privateOverlay);
        }

        private void RenderEye(CommandList cl, Framebuffer fb,  Matrix4x4 proj, Matrix4x4 view)
        {
            cl.SetFramebuffer(fb);
            cl.ClearDepthStencil(1f);
            cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            foreach (Renderable renderObj in mainQueue.Renderables)
            {
                renderObj.Render(gd, cl, new UBO(
                proj,
                view,
                renderObj.entity.globalTrans()));
            }
            skybox.Render(cl, fb, proj, view);
        }


        public void switchVRContext(OutputType type)
        {
            if(type != engine.outputType)
            {
                engine.logger.Log("Output Device Change:" + type.ToString());
                engine.outputType = type;
                if (!vrContext.Disposed)
                {
                    vrContext.Dispose();
                }
                vrContext = buildVRContext();
                vrContext.Initialize(gd);
            }

        }

        public void Update()
        {
            if (vrContext.Disposed)
            {
               switchVRContext(OutputType.Screen);
            }

            windowCL.Begin();
            windowCL.SetFramebuffer(sc.Framebuffer);
            windowCL.ClearColorTarget(0, new RgbaFloat(0f, 0f, 0.2f, 1f));
            vrContext.RenderMirrorTexture(windowCL, sc.Framebuffer, (engine.outputType != OutputType.Screen)? eyeSource : MirrorTextureEyeSource.LeftEye);
            windowCL.End();
            gd.SubmitCommands(windowCL);
            gd.SwapBuffers(sc);

            HmdPoseState poses = vrContext.WaitForPoses();
            BuildMainRenderQueue();
            // Render Eyes

            if(!(engine.backend == GraphicsBackend.OpenGL|| engine.backend == GraphicsBackend.OpenGLES))
            {
                eyesCL.PushDebugGroup("Left Eye");
            }
            Matrix4x4 leftView = poses.CreateView(VREye.Left, _userTrans, - Vector3.UnitZ, Vector3.UnitY);
                RenderEye(eyesCL, vrContext.LeftEyeFramebuffer, poses.LeftEyeProjection, leftView);
                eyesCL.PopDebugGroup();
            if (engine.outputType != OutputType.Screen)
            {
                if (!(engine.backend == GraphicsBackend.OpenGL || engine.backend == GraphicsBackend.OpenGLES))
                {
                    eyesCL.PushDebugGroup("Right Eye");
                }
                Matrix4x4 rightView = poses.CreateView(VREye.Right, _userTrans, -Vector3.UnitZ, Vector3.UnitY);
                RenderEye(eyesCL, vrContext.RightEyeFramebuffer, poses.RightEyeProjection, rightView);
                eyesCL.PopDebugGroup();
            }


            eyesCL.End();
            gd.SubmitCommands(eyesCL);

            vrContext.SubmitFrame();
        }
    }
}
