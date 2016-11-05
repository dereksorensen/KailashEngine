using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

using KailashEngine.Render.Shader;
using KailashEngine.Render.Objects;
using KailashEngine.Output;

namespace KailashEngine.Render.FX
{
    class fx_Quad : RenderEffect
    {

        private int _vao;

        // Programs
        private Program _pRenderTexture2D;
        private Program _pRenderTexture1D;


        public fx_Quad(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }

        protected override void load_Programs()
        {
            _pRenderTexture2D = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "render_Texture2D.frag", null)
            });
            _pRenderTexture2D.enable_Samplers(1);

            _pRenderTexture1D = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "render_Texture1D.frag", null)
            });
            _pRenderTexture1D.enable_Samplers(1);
        }

        protected override void load_Buffers()
        {
            //float[] temp = {
            //      -1.0f, -1.0f, 0.0f,
            //      3.0f, -1.0f, 0.0f,
            //      -1.0f, 3.0f, 0.0f
            //};

            //int bSize = sizeof(float) * temp.Length;

            GL.GenVertexArrays(1, out _vao);
            GL.BindVertexArray(_vao);

            //int vbo = 0;
            //GL.GenBuffers(1, out vbo);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            //GL.BufferData(
            //    BufferTarget.ArrayBuffer,
            //    (IntPtr)bSize,
            //    temp,
            //    BufferUsageHint.StaticDraw);

            //GL.EnableVertexAttribArray(0);
            //GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            //GL.BindVertexArray(0);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public override void load()
        {
            load_Programs();
            load_Buffers();
        }

        public override void unload()
        {

        }

        public override void reload()
        {

        }


        //------------------------------------------------------
        // Render Full Screen Quad
        //------------------------------------------------------

        public void render()
        {
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.BindVertexArray(0);
        }

        public void renderBlend()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            render();

            GL.Disable(EnableCap.Blend);
        }


        //------------------------------------------------------
        // Render Textures
        //------------------------------------------------------

        public void render_Texture2D(Texture texture)
        {
            render_Texture2D(texture, 1, 0);
        }

        public void render_Texture2D(Texture texture, float size, int position)
        {
            // Calculate Quad Positioning
            int size_x = (int)(_resolution.W * size);
            int size_y = (int)(_resolution.H * size);

            int pos_x = _resolution.W - size_x;
            int pos_y = size_y * position;

            // Render it!
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            
            GL.Viewport(pos_x, pos_y, size_x, size_y);

            switch(texture.target)
            {
                case TextureTarget.Texture1D:
                    _pRenderTexture1D.bind();
                    break;
                case TextureTarget.Texture2D:
                    _pRenderTexture2D.bind();
                    break;
            }

            texture.bind(_pRenderTexture2D.getSamplerUniform(0), 0);

            render();
        }

    }
}
