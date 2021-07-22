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
using RhubarbEngine.World.Asset;
using Veldrid;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Components.ImGUI
{
    [Category(new string[] { "ImGUI" })]
    public class ImGUICanvas: AssetProvider<RTexture2D> , IRenderObject, KeyboardStealer
    {
        public Sync<Vector2u> scale;

        public Sync<RenderFrequency> renderFrequency;

        public SyncRef<IUIElement> element;

        public SyncRef<IinputPlane> imputPlane;

        private ImGuiRenderer igr;

        private CommandList UIcommandList;

        private Framebuffer framebuffer;

        private bool UIloaded = false;

        public RenderFrequency renderFrac => renderFrequency.value;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            scale = new Sync<Vector2u>(this, newRefIds);
            renderFrequency = new Sync<RenderFrequency>(this, newRefIds);
            scale.value = new Vector2u(600, 600);
            scale.Changed += onScaleChange;
            element = new SyncRef<IUIElement>(this, newRefIds);
            imputPlane = new SyncRef<IinputPlane>(this, newRefIds);
        }

        private void onScaleChange(IChangeable val)
        {
            if(framebuffer != null && igr != null && UIloaded)
            {
                UIloaded = false;
                load(null);
                framebuffer.Dispose();
                igr.Dispose();
                loadUI();
            }
        }

        public override void onLoaded()
        {
            base.onLoaded();
            UIcommandList = engine.renderManager.gd.ResourceFactory.CreateCommandList();
            loadUI();
        }

        private Framebuffer CreateFramebuffer(uint width, uint height)
        {
            ResourceFactory factory = engine.renderManager.gd.ResourceFactory;
            Texture colorTarget = factory.CreateTexture(TextureDescription.Texture2D(
                width, height,
                1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
                TextureUsage.RenderTarget | TextureUsage.Sampled
                ));
            Texture depthTarget = factory.CreateTexture(TextureDescription.Texture2D(
                width, height,
                1, 1,
                PixelFormat.R32_Float,
                TextureUsage.DepthStencil));
            return factory.CreateFramebuffer(new FramebufferDescription(depthTarget, colorTarget));
        }

        private  void loadUI()
        {
            try
            { 
                logger.Log("Loading ui");
                framebuffer = CreateFramebuffer(scale.value.x, scale.value.y);
                igr = new ImGuiRenderer(engine.renderManager.gd, framebuffer.OutputDescription, (int)scale.value.x, (int)scale.value.y, ColorSpaceHandling.Linear);
                UIloaded = true;
                Texture target = framebuffer.ColorTargets[0].Target;
                TextureView view = engine.renderManager.gd.ResourceFactory.CreateTextureView(target);
                load(new RTexture2D(view));
            }
            catch (Exception e)
            {
                logger.Log("ImGUI Error When Loading Error" + e.ToString(), true);
            }
        }

        public class FakeInputSnapshot : InputSnapshot
        {
            public IReadOnlyList<KeyEvent> KeyEvents => new List<KeyEvent>();

            public IReadOnlyList<MouseEvent> MouseEvents => new List<MouseEvent>();

            public IReadOnlyList<char> KeyCharPresses => new List<char>();

            public Vector2 MousePosition => Vector2.Zero;

            public float WheelDelta => 0;

            public bool IsMouseDown(MouseButton button)
            {
                return false;
            }
        }

        private void ImGuiUpdate()
        {
            if(element.target != null)
            {
                ImGui.SetWindowPos(Vector2.Zero);
                ImGui.SetWindowSize(new Vector2(scale.value.x, scale.value.y));
                ImGui.SetNextWindowCollapsed(false);
                element.target.ImguiRender(igr);
                if (ImGui.IsAnyItemActive())
                {
                    input.keyboard = this;
                }
                else
                {
                    if (input.keyboard == this)
                    {
                        input.keyboard = null;
                    }
                }

            }
        }

        public void Render()
        {
            try
            {
                igr.Update((float)engine.platformInfo.deltaSeconds, (InputSnapshot)imputPlane.target??(InputSnapshot)new FakeInputSnapshot());
                ImGuiUpdate();
                UIcommandList.Begin();
                UIcommandList.SetFramebuffer(framebuffer);
                UIcommandList.ClearColorTarget(0, new RgbaFloat(0f, 0f, 0f, 0f));
                igr.Render(engine.renderManager.gd, UIcommandList);
                UIcommandList.End();
                engine.renderManager.gd.SubmitCommands(UIcommandList);
            }catch (Exception e)
            {
                logger.Log("Error Rendering" + e.ToString(), true);
            }
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            base.CommonUpdate(startTime, Frame);
            Render();
        }

        public ImGUICanvas(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUICanvas()
        {
        }

    }
}
