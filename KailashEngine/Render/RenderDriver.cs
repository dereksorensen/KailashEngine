using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using KailashEngine.Client;
using KailashEngine.Render.FX;

namespace KailashEngine.Render
{
    class RenderDriver
    {

        private Game _game;

        private ProgramLoader _pLoader;

        private fx_gBuffer _gBuffer;


        public RenderDriver(
            int glsl_version,
            string glsl_base_path)
        {
            _pLoader = new ProgramLoader(glsl_version, glsl_base_path);

            _gBuffer = new fx_gBuffer(_pLoader, "gBuffer");

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

        public void load(Game game)
        {
            _game = game;

            load_DefaultGL();
            load_FX();
        }

        public void unload()
        {


        }


        public void render(Game game)
        {
            _gBuffer.render(game);
        }





    }
}
