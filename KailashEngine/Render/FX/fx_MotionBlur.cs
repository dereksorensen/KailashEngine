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
        private Program _pBlur;
        private Program _pBlend;

        // Frame Buffers
        private FrameBuffer _fMotionBlur;

        // Textures
        private Texture _tFinal;
        public Texture tFinal
        {
            get { return _tFinal; }
        }


        // Other Buffers
        private ShaderStorageBuffer _ssboExposure;


        public fx_MotionBlur(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }

        protected override void load_Programs()
        {

            _pBlur = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "mb_Blur.frag", null)
            });
            _pBlur.enable_Samplers(3);
            _pBlur.addUniform("fps_scaler");

        }

        protected override void load_Buffers()
        {

            _tFinal = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tFinal.load();


            _fMotionBlur = new FrameBuffer("Motion Blur");
            _fMotionBlur.load(new Dictionary<FramebufferAttachment, Texture>()
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


        public void render(fx_Quad quad, FrameBuffer scene_fbo, Texture scene_texture, Texture depth_texture, Texture velocity_texture)
        {


            GL.Viewport(0, 0, _tFinal.width, _tFinal.height);

            _pBlur.bind();


            GL.Uniform1(_pBlur.getUniform("fps_scaler"), 60.0f);


            // Velocity Texture
            velocity_texture.bind(_pBlur.getSamplerUniform(1), 1);

            // Depth Texture
            depth_texture.bind(_pBlur.getSamplerUniform(2), 2);


            //------------------------------------------------------
            // Pass 1
            //------------------------------------------------------
            _fMotionBlur.bind(DrawBuffersEnum.ColorAttachment0);

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


    }
}
