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


        int vao_bokeh = 0;
        int vbo_bokeh = 0;

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


            //Vector3[] temp = { new Vector3(-1f, -1f, 0f), new Vector3(3f, -1f, 0f), new Vector3(-1f, 3f, 0f) };
            Vector3[] temp = {
                new Vector3(1f, -1f, 0f),
                new Vector3(-1f, -1f, 0f),
                new Vector3(1f, 1f, 0f),
                new Vector3(-1f, 1f, 0f)
            };
            //Vector3[] temp = { new Vector3(-1f, 3f, 0f), new Vector3(3f, -1f, 0f), new Vector3(-1f, -1f, 0f) };

            int bSize = (int)EngineHelper.size.vec3 * 4;
            
            GL.GenBuffers(1, out vbo_bokeh);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_bokeh);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)bSize,
                temp,
                BufferUsageHint.StaticDraw);


            GL.GenVertexArrays(1, out vao_bokeh);
            GL.BindVertexArray(vao_bokeh);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_bokeh);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
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
            //_gBuffer.render(scene);

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, _resolution.W/2, _resolution.H/2);


            GL.BindVertexArray(vao_bokeh);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_bokeh);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

        }





    }
}
