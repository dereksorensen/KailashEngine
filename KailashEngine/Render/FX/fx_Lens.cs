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
    class fx_Lens : RenderEffect
    {

        // Programs
        private Program _pBlend;
        private Program _pBrightSpots;
        private Program _pFlare;

        // Frame Buffers
        private FrameBuffer _fLens;

        // Textures
        private Image _iLensColor;
        private Image _iLensDirt;
        private Image _iLensStar;

        private Texture _tBrightSpots;
        public Texture tBrightSpots
        {
            get { return _tBrightSpots; }
        }

        private Texture _tBloom;
        public Texture tBloom
        {
            get { return _tBloom; }
        }

        private Texture _tFlare;
        public Texture tFlare
        {
            get { return _tFlare; }
        }



        public fx_Lens(ProgramLoader pLoader, StaticImageLoader tLoader, string resource_folder_name, Resolution full_resolution)
            : base(pLoader, tLoader, resource_folder_name, full_resolution)
        { }

        protected override void load_Programs()
        {
            _pBlend = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "lens_Blend.frag", null)
            });
            _pBlend.enable_Samplers(4);

            _pBrightSpots = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "lens_BrightSpots.frag", null)
            });
            _pBrightSpots.enable_Samplers(1);

            _pFlare = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "lens_Flare.frag", null)
            });
            _pFlare.enable_Samplers(2);
        }

        protected override void load_Buffers()
        {
            // Load Lens Images
            _iLensColor = _tLoader.createImage(_path_static_textures + "lf_lensColor-2.png", TextureTarget.Texture1D, TextureWrapMode.ClampToEdge);
            _iLensDirt = _tLoader.createImage(_path_static_textures + "lf_lensDirt-5.jpg", TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
            _iLensStar = _tLoader.createImage(_path_static_textures + "lf_lensStar.png", TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);


            // Load Textures
            float default_texture_scale = 0.25f;

            float bright_spots_scale = default_texture_scale;
            Vector2 bright_spots_resolution = new Vector2(_resolution.W * bright_spots_scale, _resolution.H * bright_spots_scale);
            _tBrightSpots = new Texture(TextureTarget.Texture2D,
                (int)bright_spots_resolution.X, (int)bright_spots_resolution.Y,
                0, true, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tBrightSpots.load();

            float bloom_scale = default_texture_scale;
            Vector2 bloom_resolution = new Vector2(_resolution.W * bloom_scale, _resolution.H * bloom_scale);
            _tBloom = new Texture(TextureTarget.Texture2D,
                (int)bloom_resolution.X, (int)bloom_resolution.Y,
                0, true, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tBloom.load();

            float flare_scale = default_texture_scale;
            Vector2 flare_resolution = new Vector2(_resolution.W * flare_scale, _resolution.H * flare_scale);
            _tFlare = new Texture(TextureTarget.Texture2D,
                (int)flare_resolution.X, (int)flare_resolution.Y,
                0, true, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tFlare.load();

            // Load Frame Buffer
            _fLens = new FrameBuffer("Lens");
            _fLens.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.ColorAttachment0, _tBrightSpots },
                { FramebufferAttachment.ColorAttachment1, _tBloom },
                { FramebufferAttachment.ColorAttachment2, _tFlare }
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


        private void getBrightSpots(fx_Quad quad, Texture scene_texture)
        {
            _fLens.bind(DrawBuffersEnum.ColorAttachment0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, _tBrightSpots.width, _tBrightSpots.height);

            _pBrightSpots.bind();

            scene_texture.bind(_pBrightSpots.getSamplerUniform(0), 0);

            quad.render();

            _tBrightSpots.generateMipMap();
        }

        private void genBloom(fx_Quad quad, fx_Special special)
        {
            _fLens.bind(DrawBuffersEnum.ColorAttachment1);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Blur bloom texture at multiple levels of detail and combine for noice effect
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            special.blur_Guass(quad, 40, _tBrightSpots, _fLens, DrawBuffersEnum.ColorAttachment1, 1);
            special.blur_Guass(quad, 40, _tBrightSpots, _fLens, DrawBuffersEnum.ColorAttachment1, 0.5f);
            special.blur_Guass(quad, 50, _tBrightSpots, _fLens, DrawBuffersEnum.ColorAttachment1, 0.25f);
            special.blur_Guass(quad, 60, _tBrightSpots, _fLens, DrawBuffersEnum.ColorAttachment1, 0.125f);

            //special.blur_Streak(quad, 50, 90.0f, _tBloom, _fLens, DrawBuffersEnum.ColorAttachment1);

            GL.Disable(EnableCap.Blend);
        }


        private void genFlare(fx_Quad quad, fx_Special special)
        {
            _fLens.bind(DrawBuffersEnum.ColorAttachment2);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, _tFlare.width, _tFlare.height);

            _pFlare.bind();

            _tBrightSpots.bind(_pFlare.getSamplerUniform(0), 0);
            _iLensColor.bind(_pFlare.getSamplerUniform(1), 1);

            quad.render();

            special.blur_MovingAverage(13, _tFlare);
        }


        private void blend(fx_Quad quad, FrameBuffer scene_fbo)
        {
            scene_fbo.bind(DrawBuffersEnum.ColorAttachment0);
            GL.Viewport(0, 0, _resolution.W, _resolution.H);

            _pBlend.bind();

            _iLensDirt.bind(_pBlend.getSamplerUniform(0), 0);
            _iLensStar.bind(_pBlend.getSamplerUniform(1), 1);
            _tBloom.bind(_pBlend.getSamplerUniform(2), 2);
            _tFlare.bind(_pBlend.getSamplerUniform(3), 3);

            quad.renderBlend();
        }


        public void render(fx_Quad quad, fx_Special special, Texture scene_texture, FrameBuffer scene_fbo)
        {
            getBrightSpots(quad, scene_texture);

            genBloom(quad, special);
            genFlare(quad, special);

            blend(quad, scene_fbo);
        }
    }
}
