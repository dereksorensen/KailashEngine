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
        private fx_gBuffer _gBuffer;


        int vao = 0;
        int vbo = 0;

        public RenderDriver(
            ProgramLoader pLoader,
            Resolution resolution)
        {
            _pLoader = pLoader;
            _resolution = resolution;

            // Render UBOs
            _ubo_camera = new UniformBuffer(BufferUsageHint.StaticDraw, 0, new EngineHelper.size[] {
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



            _pDrawQuad = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, "common/draw_quad.vert", null),
                new ShaderFile(ShaderType.FragmentShader, "common/draw_quad.frag", null)
            });
            _pDrawQuad.enable_Samplers(1);







            float[] temp = {
                  -1.0f, -1.0f, 0.0f,
                  3.0f, -1.0f, 0.0f,
                  -1.0f, 3.0f, 0.0f
            };

            int bSize = sizeof(float) * temp.Length;

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)bSize,
                temp,
                BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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


            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);  
            GL.Disable(EnableCap.DepthTest);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, _resolution.W / 2, _resolution.H / 2);

            _pDrawQuad.bind();

            _gBuffer.tDiffuse.bind(_pDrawQuad.getUniform("sampler0"), 0);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.BindVertexArray(0);

        }





    }
}
