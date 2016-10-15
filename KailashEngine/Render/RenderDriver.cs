using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using KailashEngine.Client;
using KailashEngine.Output;
using KailashEngine.Render.Objects;
using KailashEngine.Render.FX;
using KailashEngine.Render.Shader;

namespace KailashEngine.Render
{
    class RenderDriver
    {
        private Resolution _resolution;
        

        // Render UBOs
        private UniformBuffer _ubo_camera;
        private UniformBuffer _ubo_game_config;

        // Render FXs
        private fx_Quad _quad;
        private fx_Test _test;
        private fx_Crosshair _crosshair;
        private fx_Final _final;
        private fx_gBuffer _gBuffer;
        private fx_HDR _hdr;


        public RenderDriver(
            ProgramLoader pLoader,
            StaticImageLoader tLoader,
            Resolution resolution)
        {
            _resolution = resolution;


            // Render UBOs
            _ubo_game_config = new UniformBuffer(BufferUsageHint.StaticDraw, 0, new EngineHelper.size[]
            {
                EngineHelper.size.vec4,
                EngineHelper.size.f
            });

            _ubo_camera = new UniformBuffer(BufferUsageHint.StaticDraw, 1, new EngineHelper.size[] {
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.vec3,
                EngineHelper.size.vec3
            });


            // Render FXs
            _quad = new fx_Quad(pLoader, "common/", _resolution);
            _test = new fx_Test(pLoader, "test/", _resolution);
            _crosshair = new fx_Crosshair(pLoader, tLoader, "crosshair/", _resolution);
            _final = new fx_Final(pLoader, "final/", _resolution);
            _gBuffer = new fx_gBuffer(pLoader, "gBuffer/", _resolution);
            _hdr = new fx_HDR(pLoader, "hdr/", _resolution);


        }


        //------------------------------------------------------
        // Loading
        //------------------------------------------------------

        private void load_DefaultGL()
        {
            // Default OpenGL Setup
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.DepthRange(0.0f, 1.0f);
            GL.Enable(EnableCap.DepthClamp);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);
        }

        private void load_FX()
        {
            _quad.load();
            _crosshair.load();
            _final.load();
            _gBuffer.load();
            _hdr.load();
        }

        public void load()
        {
            load_DefaultGL();
            load_FX();
        }


        private void unload_FX()
        {

        }


        public void unload()
        {

            unload_FX();

        }

        //------------------------------------------------------
        // Updating
        //------------------------------------------------------
        public void updateUBO_GameConfig(Vector4 near_far, float target_fps)
        {
            _ubo_game_config.update(0, near_far);
            _ubo_game_config.update(1, target_fps);
        }

        public void updateUBO_Camera(Matrix4 view, Matrix4 perspective, Vector3 position, Vector3 look)
        {
            _ubo_camera.update(0, view);
            _ubo_camera.update(1, perspective);
            _ubo_camera.update(2, position);
            _ubo_camera.update(3, look);
        }

        public void handle_MouseState(bool locked)
        {
            _crosshair.enabled = locked;
        }


        //------------------------------------------------------
        // Rendering
        //------------------------------------------------------
        public void render(Scene scene)
        {

            //------------------------------------------------------
            // Pre-Processing
            //------------------------------------------------------
            _hdr.calcExposure(_final.tFinalScene);


            //------------------------------------------------------
            // Scene Processing
            //------------------------------------------------------
            _gBuffer.pass_DeferredShading(scene);


            //------------------------------------------------------
            // Post-processing
            //------------------------------------------------------
            GL.Disable(EnableCap.DepthTest);


            _gBuffer.pass_LightAccumulation(_quad, _final.fFinalScene);

            _hdr.scaleScene(_quad, _final.fFinalScene, _final.tFinalScene);


            //------------------------------------------------------
            // Render to Screen
            //------------------------------------------------------
            _final.render(_quad);


            //------------------------------------------------------
            // Overlays
            //------------------------------------------------------
            _crosshair.render(scene.animation_timer.seconds);





            //------------------------------------------------------
            // Debug Views
            //------------------------------------------------------
            _quad.render_Texture2D(_gBuffer.tDiffuse_ID, 0.25f, 0);

        }





    }
}
