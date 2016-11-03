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
    class fx_HDR : RenderEffect
    {

        // Programs
        private Program _pLuminosity;
        private Program _pAutoExposure;
        private Program _pBloom;
        private Program _pScaleScene;

        // Frame Buffers
        private FrameBuffer _fBloom;

        // Textures
        private Texture _tLuminosity;
        public Texture tLuminosity
        {
            get { return _tLuminosity; }
        }

        private Texture _tTempScene;
        public Texture tTempScene
        {
            get { return _tTempScene; }
        }

        private Texture _tBloom;
        public Texture tBloom
        {
            get { return _tBloom; }
        }



        // Other Buffers
        private ShaderStorageBuffer _ssboExposure;


        public fx_HDR(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }

        protected override void load_Programs()
        {
            _pLuminosity = _pLoader.createProgram(new ShaderFile[]{
                new ShaderFile(ShaderType.ComputeShader, _path_glsl_effect  + "hdr_Luminosity.comp", null)
            });
            _pLuminosity.enable_Samplers(2);

            _pAutoExposure = _pLoader.createProgram(new ShaderFile[]{
                new ShaderFile(ShaderType.ComputeShader, _path_glsl_effect  + "hdr_AutoExposure.comp", null)
            });
            _pAutoExposure.enable_Samplers(1);
            _pAutoExposure.addUniform("luminosity_lod");

            _pBloom = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "hdr_Bloom.frag", null)
            });
            _pBloom.enable_Samplers(1);

            _pScaleScene = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "hdr_ScaleScene.frag", null)
            });
            _pScaleScene.enable_Samplers(2);
        }

        protected override void load_Buffers()
        {
            _ssboExposure = new ShaderStorageBuffer(new EngineHelper.size[]
            {
                EngineHelper.size.vec2
            });

            _tLuminosity = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, true, false,
                PixelInternalFormat.R16f, PixelFormat.Red, PixelType.Float,
                TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tLuminosity.load();

            _tTempScene = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tTempScene.load();



            float bloom_scale = 0.25f;
            Vector2 bloom_resolution = new Vector2(_resolution.W * bloom_scale, _resolution.H * bloom_scale);
            _tBloom = new Texture(TextureTarget.Texture2D,
                (int)bloom_resolution.X, (int)bloom_resolution.Y,
                0, true, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tBloom.load();

            _fBloom = new FrameBuffer("Bloom");
            _fBloom.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.ColorAttachment0, _tBloom }
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


        private void luminosity(Texture scene_texture)
        {
            int wg_size = 32;

            _pLuminosity.bind();

            // Get luminance from last frame's final scene texture
            scene_texture.bindImageUnit(_pLuminosity.getSamplerUniform(0), 0, TextureAccess.ReadOnly);
            _tLuminosity.bindImageUnit(_pLuminosity.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            GL.DispatchCompute(_resolution.W / wg_size, _resolution.H / wg_size, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            _tLuminosity.generateMipMap();
        }

        private void autoExposure()
        {
            _pAutoExposure.bind();

            // Pass lowest LOD based on log2(max(dimension))
            GL.Uniform1(_pAutoExposure.getUniform("luminosity_lod"), _tLuminosity.getMaxMipMap());

            // Bind SSBO for storing luminosity
            _ssboExposure.bind(0);

            // Bind Scene Texture
            _tLuminosity.bind(_pAutoExposure.getSamplerUniform(0), 0);


            GL.DispatchCompute(1, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

            _ssboExposure.unbind();
        }

        private void printExposure()
        {
            int exp_size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector2));

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            Vector2 lumRead = new Vector2();

            _ssboExposure.bind();
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)0, exp_size, ref lumRead);

            Debug.DebugHelper.logInfo(1, "Scene Luminosity", lumRead.ToString());
        }




        public void calcExposure(Texture scene_texture)
        {
            luminosity(scene_texture);
            autoExposure();
            //printExposure();
        }


        public void genBloom(fx_Quad quad, fx_Special special, Texture scene_texture)
        {
            _fBloom.bind(DrawBuffersEnum.ColorAttachment0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, _tBloom.width, _tBloom.height);

            _pBloom.bind();

            scene_texture.bind(_pBloom.getSamplerUniform(0), 0);

            quad.render();

            _tBloom.generateMipMap();

            // Blur bloom texture at multiple levels of detail and combine for noice effect
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            special.blur_Guass(quad, 40.0f, _tBloom, _fBloom, DrawBuffersEnum.ColorAttachment0, 1);
            special.blur_Guass(quad, 40.0f, _tBloom, _fBloom, DrawBuffersEnum.ColorAttachment0, 0.5f);
            special.blur_Guass(quad, 50.0f, _tBloom, _fBloom, DrawBuffersEnum.ColorAttachment0, 0.25f);
            special.blur_Guass(quad, 60.0f, _tBloom, _fBloom, DrawBuffersEnum.ColorAttachment0, 0.125f);


            GL.Disable(EnableCap.Blend);
        }


        public void scaleScene(fx_Quad quad, FrameBuffer scene_fbo, Texture scene_texture)
        {
            //Copy scene texture to temporary texture so we can read and write to main scene texture
            GL.CopyImageSubData(scene_texture.id, ImageTarget.Texture2D, 0, 0, 0, 0,
                                _tTempScene.id, ImageTarget.Texture2D, 0, 0, 0, 0,
                                _resolution.W, _resolution.H, 1);

            scene_fbo.bind(DrawBuffersEnum.ColorAttachment0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, _resolution.W, _resolution.H);

            _pScaleScene.bind();

            _tTempScene.bind(_pScaleScene.getSamplerUniform(0), 0);
            _tBloom.bind(_pScaleScene.getSamplerUniform(1), 1);
            _ssboExposure.bind(0);

            quad.render();

            _ssboExposure.unbind(0);

        }


    }
}
