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
    class fx_MotionBlur : RenderEffect
    {

        // Programs
        private Program _pDilate;
        private Program _pBlur;

        // Frame Buffers
        private FrameBuffer _fFullResolution;

        // Textures
        private Texture _tFinal;
        public Texture tFinal
        {
            get { return _tFinal; }
        }

        private Texture _tVelocity_1;
        public Texture tVelocity_1
        {
            get { return _tVelocity_1; }
        }

        private Texture _tVelocity_2;
        public Texture tVelocity_2
        {
            get { return _tVelocity_2; }
        }


        public fx_MotionBlur(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }

        protected override void load_Programs()
        {
            string[] blur_helpers = new string[]
            {
                EngineHelper.path_glsl_common_ubo_gameConfig
            };

            _pDilate = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.ComputeShader, _path_glsl_effect + "mb_Dilate.comp", null)
            });
            _pDilate.enable_Samplers(2);
            _pDilate.addUniform("blur_amount");
            _pDilate.addUniform("texture_size");
            _pDilate.addUniform("direction_selector");


            _pBlur = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "mb_Blur.frag", blur_helpers)
            });
            _pBlur.enable_Samplers(3);
            _pBlur.addUniform("fps_scaler");

        }

        protected override void load_Buffers()
        {

            _tVelocity_1 = new Texture(TextureTarget.Texture2D,
                _resolution_half.W, _resolution_half.H, 0, 
                false, false,
                PixelInternalFormat.Rg16f, PixelFormat.Rg, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tVelocity_1.load();

            _tVelocity_2 = new Texture(TextureTarget.Texture2D,
                _tVelocity_1.width, _tVelocity_1.height, 0,
                false, false,
                PixelInternalFormat.Rg16f, PixelFormat.Rg, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tVelocity_2.load();


            _tFinal = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H, 0, 
                false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tFinal.load();


            _fFullResolution = new FrameBuffer("Motion Blur");
            _fFullResolution.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.ColorAttachment0, _tFinal },
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

        private void dilateVelocity(fx_Quad quad, fx_Special special, Texture velocity_texture)
        {

            int blur_amount = 150;


            _pDilate.bind();
            GL.Uniform1(_pDilate.getUniform("blur_amount"), blur_amount);
            GL.Uniform2(_pDilate.getUniform("texture_size"), _tVelocity_1.dimensions.Xy);

            int fragmentation = 2;

            //------------------------------------------------------
            // Horizontal
            //------------------------------------------------------
            GL.Uniform1(_pDilate.getUniform("direction_selector"), 0);
            velocity_texture.bind(_pDilate.getSamplerUniform(0), 0);
            _tVelocity_2.bindImageUnit(_pDilate.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            GL.DispatchCompute((int)_tVelocity_1.dimensions.Y, fragmentation, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);


            //------------------------------------------------------
            // Vertical
            //------------------------------------------------------
            GL.Uniform1(_pDilate.getUniform("direction_selector"), 1);
            _tVelocity_2.bind(_pDilate.getSamplerUniform(0), 0);
            _tVelocity_1.bindImageUnit(_pDilate.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            GL.DispatchCompute((int)_tVelocity_1.dimensions.X, fragmentation, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);


        }

        private void motionBlur(fx_Quad quad, FrameBuffer scene_fbo, Texture scene_texture, Texture depth_texture, float fps)
        {


            GL.Viewport(0, 0, _tFinal.width, _tFinal.height);

            _pBlur.bind();


            GL.Uniform1(_pBlur.getUniform("fps_scaler"), fps);


            // Velocity Texture
            _tVelocity_1.bind(_pBlur.getSamplerUniform(1), 1);

            // Depth Texture
            depth_texture.bind(_pBlur.getSamplerUniform(2), 2);


            //------------------------------------------------------
            // Pass 1
            //------------------------------------------------------
            _fFullResolution.bind(DrawBuffersEnum.ColorAttachment0);

            // Source Texture
            scene_texture.bind(_pBlur.getSamplerUniform(0), 0);

            quad.render();


            //------------------------------------------------------
            // Pass 2
            //------------------------------------------------------
            scene_fbo.bind(DrawBuffersEnum.ColorAttachment0);

            // Source Texture
            _tFinal.bind(_pBlur.getSamplerUniform(0), 0);

            quad.render();


            ////------------------------------------------------------
            //// Pass 3
            ////------------------------------------------------------
            //_fMotionBlur.bind(DrawBuffersEnum.ColorAttachment0);

            //// Source Texture
            //scene_texture.bind(_pBlur.getSamplerUniform(0), 0);

            //quad.render();


            ////------------------------------------------------------
            //// Pass 4
            ////------------------------------------------------------
            //scene_fbo.bind(DrawBuffersEnum.ColorAttachment0);

            //// Source Texture
            //_tFinal.bind(_pBlur.getSamplerUniform(0), 0);

            //quad.render();
        }


        public void render(fx_Quad quad, fx_Special special, FrameBuffer scene_fbo, Texture scene_texture, Texture depth_texture, Texture velocity_texture, float fps)
        {
            dilateVelocity(quad, special, velocity_texture);
            motionBlur(quad, scene_fbo, scene_texture, depth_texture, fps);
        }


    }
}
