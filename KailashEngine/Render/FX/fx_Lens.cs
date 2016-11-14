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
        // Properties
        private Matrix3 _previous_lens_star_mod;
        private const float _texture_scale = 0.25f;
        private Resolution _resolution_lens;

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
        {
            _previous_lens_star_mod = Matrix3.Identity;
            _resolution_lens = new Resolution(_resolution.W * _texture_scale, _resolution.H * _texture_scale);
        }

        protected override void load_Programs()
        {
            _pBlend = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "lens_Blend.frag", null)
            });
            _pBlend.enable_Samplers(4);
            _pBlend.addUniform("lens_star_mod");

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
            _iLensColor = _tLoader.createImage(_path_static_textures + "lens_Color.png", TextureTarget.Texture1D, TextureWrapMode.ClampToEdge);
            _iLensDirt = _tLoader.createImage(_path_static_textures + "lens_Dirt-5.jpg", TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
            _iLensStar = _tLoader.createImage(_path_static_textures + "lens_Star.png", TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);

            // Load Textures
            _tBloom = new Texture(TextureTarget.Texture2D,
                _resolution_lens.W, _resolution_lens.H,
                0, true, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tBloom.load();

            _tFlare = new Texture(TextureTarget.Texture2D,
                _resolution_lens.W, _resolution_lens.H,
                0, false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tFlare.load();

            // Load Frame Buffer
            _fLens = new FrameBuffer("Lens");
            _fLens.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.ColorAttachment0, _tBloom },
                { FramebufferAttachment.ColorAttachment1, _tFlare }
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
            GL.Viewport(0, 0, _tBloom.width, _tBloom.height);

            _pBrightSpots.bind();

            scene_texture.bind(_pBrightSpots.getSamplerUniform(0), 0);

            quad.render();

            _tBloom.generateMipMap();
        }


        private void genFlare(fx_Quad quad, fx_Special special)
        {
            _fLens.bind(DrawBuffersEnum.ColorAttachment1);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, _tFlare.width, _tFlare.height);

            _pFlare.bind();

            _tBloom.bind(_pFlare.getSamplerUniform(0), 0);
            _iLensColor.bind(_pFlare.getSamplerUniform(1), 1);

            quad.render();

            special.blur_MovingAverage(15, _tFlare);
        }


        private void genBloom(fx_Quad quad, fx_Special special)
        {

            // Blur bloom texture at multiple levels of detail and combine for noice effect
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            special.blur_Guass(quad, 40, _tBloom, _fLens, DrawBuffersEnum.ColorAttachment0, 1);
            special.blur_Guass(quad, 40, _tBloom, _fLens, DrawBuffersEnum.ColorAttachment0, 0.5f);
            special.blur_Guass(quad, 50, _tBloom, _fLens, DrawBuffersEnum.ColorAttachment0, 0.25f);
            special.blur_Guass(quad, 60, _tBloom, _fLens, DrawBuffersEnum.ColorAttachment0, 0.125f);

            GL.Disable(EnableCap.Blend);
        }


        // Spin the lens star mod with camera movements
        private Matrix3 getLensStarMod(Matrix4 camera_matrix)
        {
            Vector3 cam_z;
            Vector3 cam_x;

            cam_z = camera_matrix.Column1.Xyz;
            cam_x = camera_matrix.Column0.Xyz;

            cam_x = Vector3.Cross(cam_x, cam_z) + cam_x;

            float camrot = Vector3.Dot(cam_x, new Vector3(0.0f, 0.0f, 1.0f)) + Vector3.Dot(cam_z, new Vector3(0.0f, 1.0f, 0.0f));


            float cosRot = (float)Math.Cos(camrot * 13.0f);
            float sinRot = (float)Math.Sin(camrot * 13.0f);

            Matrix3 rotation = new Matrix3(
                cosRot, -sinRot, 0.0f,
                sinRot, cosRot, 0.0f,
                0.0f, 0.0f, 1.0f);

            float scaleMod = (float)Math.Cos(camrot * 13.0f) - (float)Math.Sin(camrot * 13.0f);
            scaleMod /= 23.0f;
            scaleMod -= 0.32f;

            Matrix3 scale_bias_1 = new Matrix3(
                2.0f, 0.0f, -1.0f,
                0.0f, 2.0f, -1.0f,
                0.0f, 0.0f, 1.0f);

            Matrix3 scale_bias_2 = new Matrix3(
                scaleMod, 0.0f, 0.5f,
                0.0f, scaleMod, 0.5f,
                0.0f, 0.0f, 1.0f);

            return scale_bias_2 * rotation * scale_bias_1;
        }

        private void blend(fx_Quad quad, FrameBuffer scene_fbo, Matrix4 camera_matrix)
        {
            scene_fbo.bind(DrawBuffersEnum.ColorAttachment0);
            GL.Viewport(0, 0, _resolution.W, _resolution.H);

            _pBlend.bind();

            _iLensDirt.bind(_pBlend.getSamplerUniform(0), 0);
            _iLensStar.bind(_pBlend.getSamplerUniform(1), 1);
            _tBloom.bind(_pBlend.getSamplerUniform(2), 2);
            _tFlare.bind(_pBlend.getSamplerUniform(3), 3);

            // Lerp the lens star mod so the spin is delayed
            Matrix3 current_lens_star_mod = getLensStarMod(camera_matrix);
            Matrix3 lens_star_mod = EngineHelper.lerp(_previous_lens_star_mod, current_lens_star_mod, 0.1f);
            _previous_lens_star_mod = lens_star_mod;

            GL.UniformMatrix3(_pBlend.getUniform("lens_star_mod"), true, ref lens_star_mod);

            quad.renderBlend();
        }


        public void render(fx_Quad quad, fx_Special special, Texture scene_texture, FrameBuffer scene_fbo, Matrix4 camera_matrix)
        {
            getBrightSpots(quad, scene_texture);

            genFlare(quad, special);
            genBloom(quad, special);

            blend(quad, scene_fbo, camera_matrix);
        }
    }
}
