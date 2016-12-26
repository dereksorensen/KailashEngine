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
        private List<RenderEffect> _effects;

        private fx_Quad _fxQuad;
        private fx_Test _fxTest;
        private fx_Crosshair _fxCrosshair;
        private fx_Special _fxSpecial;
        private fx_Final _fxFinal;
        private fx_gBuffer _fxGBuffer;
        private fx_Shadow _fxShadow;
        private fx_SkyBox _fxSkyBox;
        private fx_HDR _fxHDR;
        private fx_Lens _fxLens;
        private fx_DepthOfField _fxDepthOfField;
        private fx_MotionBlur _fxMotionBlur;
        private fx_AtmosphericScattering _fxAtmosphericScattering;


        public RenderDriver(
            ProgramLoader pLoader,
            StaticImageLoader tLoader,
            Resolution resolution)
        {
            _resolution = resolution;
            _enable_debug_views = true;

            // Render UBOs
            _ubo_game_config = new UniformBuffer(BufferStorageFlags.DynamicStorageBit, 0, new EngineHelper.size[]
            {
                EngineHelper.size.vec4,
                EngineHelper.size.f
            });

            _ubo_camera = new UniformBuffer(BufferStorageFlags.DynamicStorageBit, 1, new EngineHelper.size[] {
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.vec3,
                EngineHelper.size.vec3
            });

            // Render FX List
            _effects = new List<RenderEffect>();

            // Render FXs
            _fxQuad = createEffect<fx_Quad>(pLoader, "common/");
            _fxTest = createEffect<fx_Test>(pLoader, "test/");
            _fxCrosshair = createEffect<fx_Crosshair>(pLoader, tLoader, "crosshair/");
            _fxSpecial = createEffect<fx_Special>(pLoader, "special/");
            _fxFinal = createEffect<fx_Final>(pLoader, "final/");
            _fxGBuffer = createEffect<fx_gBuffer>(pLoader, tLoader, "gBuffer/");
            _fxShadow = createEffect<fx_Shadow>(pLoader, "shadow/");
            _fxSkyBox = createEffect<fx_SkyBox>(pLoader, tLoader, "skybox/");
            _fxHDR = createEffect<fx_HDR>(pLoader, "hdr/");
            _fxLens = createEffect<fx_Lens>(pLoader, tLoader, "lens/");
            _fxDepthOfField = createEffect<fx_DepthOfField>(pLoader, tLoader, "dof/");
            _fxMotionBlur = createEffect<fx_MotionBlur>(pLoader, "motion_blur/");
            _fxAtmosphericScattering = createEffect<fx_AtmosphericScattering>(pLoader, "ats/");

        }




        //------------------------------------------------------
        // Loading
        //------------------------------------------------------

        // Factory workers to create effects
        private T createEffect<T>(ProgramLoader pLoader, string resource_folder_name) where T : RenderEffect
        {
            T temp_effect = (T)Activator.CreateInstance(typeof(T), pLoader, resource_folder_name, _resolution);
            _effects.Add(temp_effect);
            return temp_effect;
        }
        private T createEffect<T>(ProgramLoader pLoader, StaticImageLoader tLoader, string resource_folder_name) where T : RenderEffect
        {
            T temp_effect = (T)Activator.CreateInstance(typeof(T), pLoader, tLoader, resource_folder_name, _resolution);
            _effects.Add(temp_effect);
            return temp_effect;
        }


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
            foreach(RenderEffect effect in _effects)
            {
                effect.load();
            }
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

        public void updateUBO_Camera(
            Matrix4 view, Matrix4 perspective, Matrix4 inv_view_perspective, 
            Matrix4 previous_view_perspective, Matrix4 inv_previous_view_perspective,
            Vector3 position, Vector3 look)
        {
            _ubo_camera.update(0, view);
            _ubo_camera.update(1, perspective);
            _ubo_camera.update(2, inv_view_perspective);
            _ubo_camera.update(3, previous_view_perspective);
            _ubo_camera.update(4, inv_previous_view_perspective);
            _ubo_camera.update(5, position);
            _ubo_camera.update(6, look);
        }


        public void handle_MouseState(bool locked)
        {
            _fxCrosshair.enabled = locked;
        }


        public void toggleDebugViews()
        {
            _enable_debug_views = !_enable_debug_views;
        }


        public void toggleWireframe()
        {
            _fxGBuffer.toggleWireframe();
        }

        //------------------------------------------------------
        // Rendering
        //------------------------------------------------------
        public void render(Scene scene, SpatialData camera_spatial_data)
        {

            //------------------------------------------------------
            // Pre-Processing
            //------------------------------------------------------
            GL.Disable(EnableCap.DepthTest);

            _fxAtmosphericScattering.precompute(_fxQuad);

            _fxHDR.calcExposure(_fxFinal.tFinalScene);


            //------------------------------------------------------
            // Scene Processing
            //------------------------------------------------------
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);


            Debug.DebugHelper.time_function("Shadow", 1, () =>
            {
                _fxShadow.render(scene);
            });

            _fxGBuffer.pass_DeferredShading(scene, _fxShadow);

            _fxSkyBox.render(_fxQuad, _fxGBuffer._fGBuffer, scene.circadian_timer.position);

            //------------------------------------------------------
            // Post-processing
            //------------------------------------------------------
            GL.Disable(EnableCap.DepthTest);


            _fxGBuffer.pass_LightAccumulation(_fxQuad, _fxFinal.fFinalScene);


            _fxAtmosphericScattering.render(_fxQuad, _fxGBuffer.tNormal_Depth, _fxGBuffer.tDiffuse_ID, _fxGBuffer.tSpecular, _fxFinal.fFinalScene, _fxFinal.tFinalScene, scene.circadian_timer.position);


            //_fxDepthOfField.render(_fxQuad, _fxSpecial, _fxGBuffer.tNormal_Depth, _fxFinal.fFinalScene, _fxFinal.tFinalScene);


            _fxHDR.scaleScene(_fxQuad, _fxFinal.fFinalScene, _fxFinal.tFinalScene);


            _fxLens.render(_fxQuad, _fxSpecial, _fxFinal.tFinalScene, _fxFinal.fFinalScene, camera_spatial_data.rotation_matrix);


            //_fxMotionBlur.render(_fxQuad, _fxFinal.fFinalScene, _fxFinal.tFinalScene, _fxGBuffer.tNormal_Depth, _fxGBuffer.tVelocity);


            //------------------------------------------------------
            // Render to Screen
            //------------------------------------------------------
            _fxFinal.render(_fxQuad);



            //------------------------------------------------------
            // Overlays
            //------------------------------------------------------
            _fxCrosshair.render(scene.animation_timer.seconds);



            //Debug.DebugHelper.time_function("Blur Test", 3, () =>
            //{
            //    _fxSpecial.blur_GaussCompute(100, _fxDepthOfField.tDOF_Scene);
            //    //_fxSpecial.blur_Gauss(_fxQuad, 75, _fxDepthOfField.tDOF_Scene, _fxDepthOfField._fHalfResolution, DrawBuffersEnum.ColorAttachment4);
            //    //_fxSpecial.blur_MovingAverage(19, _fxDepthOfField.tDOF_Scene);
            //});

            //------------------------------------------------------
            // Debug Views
            //------------------------------------------------------
            if (_enable_debug_views)
            {
                //_fxQuad.render_Texture(_fxDepthOfField.tDOF_Scene, 1f, 0);
                //_fxQuad.render_Texture(_fxMotionBlur.tFinal, 1f, 0);

                //_fxQuad.render_Texture(_fxShadow.tSpot, 0.25f, 3, 0);
                //_fxQuad.render_Texture(_fxShadow.tSpot, 0.25f, 2, 1);
                _fxQuad.render_Texture(_fxShadow.tPoint, 0.25f, 1);
                _fxQuad.render_Texture(_fxGBuffer.tDiffuse_ID, 0.25f, 0);
            }

        }




    }
}
