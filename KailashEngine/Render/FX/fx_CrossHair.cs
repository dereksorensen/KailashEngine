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
    class fx_Crosshair : RenderEffect
    {

        // Programs
        private Program _pCrosshair;

        // Frame Buffers
        int vao_crosshair;

        // Textures
        public Image _iCrosshair;


        public fx_Crosshair(ProgramLoader pLoader, StaticImageLoader tLoader, string resource_folder_name, Resolution full_resolution)
            : base(pLoader, tLoader, resource_folder_name, full_resolution)
        { }

        protected override void load_Programs()
        {
            // Rendering Geometry into gBuffer
            _pCrosshair = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "crosshair_Render.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "crosshair_Render.frag", null)
            });
            _pCrosshair.enable_Samplers(1);
            _pCrosshair.addUniform("rotation");
        }

        protected override void load_Buffers()
        {
            // Load Crosshair texture
            _iCrosshair = _tLoader.createImage(_path_static_textures + "crosshair.png", TextureWrapMode.ClampToEdge);


            // Create dummy VAO for point rendering
            GL.GenVertexArrays(1, out vao_crosshair);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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


        public void render(float animation_time)
        {
            if (!enabled) return;

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            _pCrosshair.bind();

            // Blend with default frame buffer
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.VertexProgramPointSize);

            // Bind Crosshair Texture
            _iCrosshair.bind(_pCrosshair.getSamplerUniform(0), 0);

            // Rotate Crosshair
            float angle = -animation_time * 2.0f;
            Vector2 rot = new Vector2(
                    (float)(Math.Sin(angle)),
                    (float)(Math.Cos(angle))
                );
            GL.Uniform2(_pCrosshair.getUniform("rotation"), rot);
            
            // Render Point Sprite
            GL.BindVertexArray(vao_crosshair);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
            GL.BindVertexArray(0);

            GL.Disable(EnableCap.Blend);
        }


    }
}
