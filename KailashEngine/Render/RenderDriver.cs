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
using KailashEngine.World;

namespace KailashEngine.Render
{
    class RenderDriver
    {
        private Resolution _resolution;

        private bool _enable_debug_views;

        // Render UBOs
        private UniformBuffer _ubo_camera;
        private UniformBuffer _ubo_game_config;

        // Render FXs
        private fx_Quad _fxQuad;
        private fx_Test _fxTest;
        private fx_Crosshair _fxCrosshair;
        private fx_Special _fxSpecial;
        private fx_Final _fxFinal;
        private fx_gBuffer _fxGBuffer;
        private fx_HDR _fxHDR;
        private fx_Lens _fxLens;


        public RenderDriver(
            ProgramLoader pLoader,
            StaticImageLoader tLoader,
            Resolution resolution)
        {
            _resolution = resolution;
            _enable_debug_views = true;

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
            _fxQuad = new fx_Quad(pLoader, "common/", _resolution);
            _fxTest = new fx_Test(pLoader, "test/", _resolution);
            _fxCrosshair = new fx_Crosshair(pLoader, tLoader, "crosshair/", _resolution);
            _fxSpecial = new fx_Special(pLoader, "special/", _resolution);
            _fxFinal = new fx_Final(pLoader, "final/", _resolution);
            _fxGBuffer = new fx_gBuffer(pLoader, "gBuffer/", _resolution);
            _fxHDR = new fx_HDR(pLoader, "hdr/", _resolution);
            _fxLens = new fx_Lens(pLoader, tLoader, "lens/", _resolution);
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
            _fxQuad.load();
            _fxCrosshair.load();
            _fxSpecial.load();
            _fxFinal.load();
            _fxGBuffer.load();
            _fxHDR.load();
            _fxLens.load();
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
            _fxCrosshair.enabled = locked;
        }


        //------------------------------------------------------
        // Rendering
        //------------------------------------------------------
        public void render(Scene scene, SpatialData camera_spatial_data)
        {

            //------------------------------------------------------
            // Pre-Processing
            //------------------------------------------------------
            _fxHDR.calcExposure(_fxFinal.tFinalScene);


            //------------------------------------------------------
            // Scene Processing
            //------------------------------------------------------
            _fxGBuffer.pass_DeferredShading(scene);


            //------------------------------------------------------
            // Post-processing
            //------------------------------------------------------
            GL.Disable(EnableCap.DepthTest);


            _fxGBuffer.pass_LightAccumulation(_fxQuad, _fxFinal.fFinalScene);


            _fxLens.render(_fxQuad, _fxSpecial, _fxFinal.tFinalScene, _fxFinal.fFinalScene, camera_spatial_data.rotation_matrix);


            _fxHDR.scaleScene(_fxQuad, _fxFinal.fFinalScene, _fxFinal.tFinalScene);


            //------------------------------------------------------
            // Render to Screen
            //------------------------------------------------------
            _fxFinal.render(_fxQuad);



            //------------------------------------------------------
            // Overlays
            //------------------------------------------------------
            _fxCrosshair.render(scene.animation_timer.seconds);


            //_fxSpecial.blur_Streak(_fxQuad, 50, 30.0f, _fxHDR.tBloom);


            //------------------------------------------------------
            // Debug Views
            //------------------------------------------------------
            if(_enable_debug_views)
            {
                _fxQuad.render_Texture2D(_fxLens.tFlare, 0.25f, 3);
                _fxQuad.render_Texture2D(_fxLens.tBrightSpots, 0.25f, 2);
                _fxQuad.render_Texture2D(_fxSpecial.tSpecial, 0.25f, 1);
                _fxQuad.render_Texture2D(_fxGBuffer.tDiffuse_ID, 0.25f, 0);
            }

        }


        public void toggleDebugViews()
        {
            _enable_debug_views = !_enable_debug_views;
        }


    }
}
