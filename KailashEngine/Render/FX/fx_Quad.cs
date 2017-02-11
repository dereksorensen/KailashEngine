using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
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
        private Program _pRenderTexture1D;
        private Program _pRenderTexture2D;
        private Program _pRenderTexture2DArray;
        private Program _pRenderTexture3D;
        private Program _pRenderTextureCube;
        private Program _pRenderTextureCubeArray;

        public fx_Quad(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }

        protected override void load_Programs()
        {
            string[] texture_cube_helpers = new string[]
            {
                EngineHelper.path_glsl_common_ubo_cameraSpatials
            };

            _pRenderTexture1D = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "render_Texture1D.frag", null)
            });
            _pRenderTexture1D.enable_Samplers(1);

            _pRenderTexture2D = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "render_Texture2D.frag", null)
            });
            _pRenderTexture2D.enable_Samplers(1);
            _pRenderTexture2D.addUniform("channel");

            _pRenderTexture2DArray = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "render_Texture2DArray.frag", null)
            });
            _pRenderTexture2DArray.enable_Samplers(1);
            _pRenderTexture2DArray.addUniform("layer");

            _pRenderTexture3D = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "render_Texture3D.frag", null)
            });
            _pRenderTexture3D.enable_Samplers(1);
            _pRenderTexture3D.addUniform("layer");

            _pRenderTextureCube = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "render_TextureCube.vert", texture_cube_helpers),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "render_TextureCube.frag", null)
            });
            _pRenderTextureCube.enable_Samplers(1);

            _pRenderTextureCubeArray = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "render_TextureCube.vert", texture_cube_helpers),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "render_TextureCubeArray.frag", null)
            });
            _pRenderTextureCubeArray.enable_Samplers(1);
            _pRenderTextureCubeArray.addUniform("layer");
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


        public void renderBlend_Blend()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            render();

            GL.Disable(EnableCap.Blend);
        }

        // Renders full quad instead of hacked triangles
        public void renderFullQuad()
        {
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);
        }

        public void renderFullQuad_Blend()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            renderFullQuad();

            GL.Disable(EnableCap.Blend);
        }

        public void render3D(int depth)
        {
            GL.BindVertexArray(_vao);
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 3, depth);
            GL.BindVertexArray(0);
        }


        //------------------------------------------------------
        // Render Textures
        //------------------------------------------------------



        public void render_Texture(Texture texture, int channel = -1)
        {
            render_Texture(texture, 1, 0, 0, channel);
        }

        public void render_Texture(Texture texture, int layer = 0, int channel = -1)
        {
            render_Texture(texture, 1, 0, layer, channel);
        }

        public void render_Texture(Texture texture, float size, int position, int layer = 0, int channel = -1)
        {
            // Calculate Quad Positioning
            int size_x = (int)(_resolution.W * size);
            int size_y = (int)(_resolution.H * size);

            int pos_x = _resolution.W - size_x;
            int pos_y = size_y * position;

            // Render it!
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            
            GL.Viewport(pos_x, pos_y, size_x, size_y);

            // Clamp requested layer to texture's depth
            layer = MathHelper.Clamp(layer, 0, texture.depth);
            channel = MathHelper.Clamp(channel, -1, 3);

            switch (texture.target)
            {
                case TextureTarget.Texture1D:
                    _pRenderTexture1D.bind();
                    texture.bind(_pRenderTexture1D.getSamplerUniform(0), 0);
                    break;
                case TextureTarget.Texture2D:
                    _pRenderTexture2D.bind();
                    texture.bind(_pRenderTexture2D.getSamplerUniform(0), 0);
                    GL.Uniform1(_pRenderTexture2D.getUniform("channel"), channel);
                    break;
                case TextureTarget.Texture3D:
                    _pRenderTexture3D.bind();
                    GL.Uniform1(_pRenderTexture3D.getUniform("layer"), layer);
                    texture.bind(_pRenderTexture3D.getSamplerUniform(0), 0);
                    break;
                case TextureTarget.Texture2DArray:
                    _pRenderTexture2DArray.bind();
                    GL.Uniform1(_pRenderTexture2DArray.getUniform("layer"), layer);
                    texture.bind(_pRenderTexture2DArray.getSamplerUniform(0), 0);
                    break;
                case TextureTarget.TextureCubeMap:
                    _pRenderTextureCube.bind();
                    texture.bind(_pRenderTextureCube.getSamplerUniform(0), 0);
                    renderFullQuad();
                    return;
                case TextureTarget.TextureCubeMapArray:
                    _pRenderTextureCubeArray.bind();
                    texture.bind(_pRenderTextureCubeArray.getSamplerUniform(0), 0);
                    GL.Uniform1(_pRenderTextureCubeArray.getUniform("layer"), layer);
                    renderFullQuad();
                    return;
                default:
                    throw new Exception($"Render Texture: Unsupported Texture Target [ {texture.target.ToString()} ]");
            }


            render();
        }

    }
}
