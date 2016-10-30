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
    class fx_Special : RenderEffect
    {

        // Programs
        private Program _pBlur_Guass;

        // Frame Buffers
        private FrameBuffer _fSpecial;

        // Textures
        private Texture _tSpecial;
        public Texture tSpecial
        {
            get { return _tSpecial; }
            set { _tSpecial = value; }
        }


        public fx_Special(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }

        protected override void load_Programs()
        {

            _pBlur_Guass = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "special_BlurGuass.frag", null)
            });
            _pBlur_Guass.enable_Samplers(2);
            _pBlur_Guass.addUniform("direction");
        }

        protected override void load_Buffers()
        {
            _tSpecial = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, false, false,
                PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tSpecial.load();


            _fSpecial = new FrameBuffer("Special");
            _fSpecial.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.ColorAttachment0, _tSpecial }
            });
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


        public void render(fx_Quad quad)
        {
            
        }


        public void blur_Guass(
            fx_Quad quad,
            float blur_amount, 
            Texture texture_to_blur)
        {
            _fSpecial.bind(DrawBuffersEnum.ColorAttachment0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Viewport(0, 0, texture_to_blur.width, texture_to_blur.height);

            _pBlur_Guass.bind();


            GL.Uniform1(_pBlur_Guass.getUniform("direction"), 0.5f);

            // Source
            texture_to_blur.bind(_pBlur_Guass.getSamplerUniform(0), 0);
            // Destination
            _tSpecial.bindImageUnit(_pBlur_Guass.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            quad.render();



            GL.Uniform1(_pBlur_Guass.getUniform("direction"), 0.5f);

            // Source
            _tSpecial.bind(_pBlur_Guass.getSamplerUniform(0), 0);
            // Destination
            texture_to_blur.bindImageUnit(_pBlur_Guass.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            quad.render();

        }

    }
}
