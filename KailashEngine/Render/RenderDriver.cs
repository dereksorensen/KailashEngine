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

        private fx_gBuffer _gBuffer;


        public RenderDriver(
            ProgramLoader pLoader,
            Resolution resolution)
        {
            _pLoader = pLoader;
            _resolution = resolution;

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
        // Rendering
        //------------------------------------------------------


        public void render(Scene scene)
        {
            _gBuffer.render(scene);
        }





    }
}
