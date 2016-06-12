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

        private ProgramLoader _pLoader;
        private Resolution _resolution;
        

        // Local Programs
        private Program _pDrawQuad;

        // Render UBOs
        private UniformBuffer _ubo_camera;

        // Render FXs
        private fx_Quad _quad;
        private fx_gBuffer _gBuffer;


        public RenderDriver(
            ProgramLoader pLoader,
            Resolution resolution)
        {
            _pLoader = pLoader;
            _resolution = resolution;
            

            // Render UBOs
            _ubo_camera = new UniformBuffer(BufferUsageHint.StaticDraw, 0, new EngineHelper.size[] {
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.vec3
            });


            // Render FXs
            _quad = new fx_Quad(_pLoader, "common", _resolution);
            _gBuffer = new fx_gBuffer(_pLoader, "gBuffer", _resolution);


        }


        //------------------------------------------------------
        // Loading
        //------------------------------------------------------

        private void load_DefaultGL()
        {
            // Default OpenGL Setup
            GL.ClearColor(Color.DarkSlateGray);
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
            _gBuffer.load();
        }

        private void unload_FX()
        {

        }

        public void load()
        {
            load_DefaultGL();
            load_FX();

        }

        public void unload()
        {

            unload_FX();

        }

        //------------------------------------------------------
        // Updating
        //------------------------------------------------------

        public void updateUBO_Camera(Matrix4 view, Matrix4 perspective, Vector3 position)
        {
            _ubo_camera.update(0, view);
            _ubo_camera.update(1, perspective);
            _ubo_camera.update(2, position);
        }


        //------------------------------------------------------
        // Rendering
        //------------------------------------------------------


        public void render(Scene scene)
        {

            //------------------------------------------------------
            // Scene Processing
            //------------------------------------------------------

            _gBuffer.pass_DeferredShading(scene);


            //------------------------------------------------------
            // Post-processing
            //------------------------------------------------------
            GL.Disable(EnableCap.DepthTest);


            _quad.render_Texture2D(_gBuffer.tDiffuse_ID);


            _quad.render_Texture2D(_gBuffer.tNormal_Depth, 0.25f, 0);
            _quad.render_Texture2D(_gBuffer.tSpecular, 0.25f, 1);

            //_quad.render_Texture2D(_gBuffer.tDiffuse, 0.5f, 0);
            //_quad.render_Texture2D(_gBuffer.tDiffuse, 0.25f, 0);
            //_quad.render_Texture2D(_gBuffer.tDiffuse, 0.125f, 0);

        }





    }
}
