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
using KailashEngine.Render.FX;

namespace KailashEngine.Render
{
    class RenderDriver
    {

        private ProgramLoader _pLoader;

        private Resolution _resolution;

        // Render UBOs
        private UBO _ubo_camera;

        // Render FXs
        private fx_gBuffer _gBuffer;


        public RenderDriver(
            ProgramLoader pLoader,
            Resolution resolution)
        {
            _pLoader = pLoader;
            _resolution = resolution;

            // Render UBOs
            _ubo_camera = new UBO(BufferUsageHint.StaticDraw, 0, new EngineHelper.size[] {
                EngineHelper.size.mat4,
                EngineHelper.size.mat4
            });


            // Render FXs
            _gBuffer = new fx_gBuffer(_pLoader, "gBuffer", _resolution);


        }


        //------------------------------------------------------
        // Loading
        //------------------------------------------------------

        private void load_DefaultGL()
        {
            // Default OpenGL Setup
            GL.ClearColor(Color.Blue);
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

        public void updateUBO_Camera(Matrix4 view, Matrix4 perspective)
        {
            _ubo_camera.update(0, view);
            _ubo_camera.update(1, perspective);
        }


        //------------------------------------------------------
        // Rendering
        //------------------------------------------------------


        public void render(Scene scene)
        {
            _gBuffer.render(scene);
        }





    }
}
