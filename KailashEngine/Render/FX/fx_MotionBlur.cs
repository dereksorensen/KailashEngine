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

        private Texture _tFinal_2;
        public Texture tFinal_2
        {
            get { return _tFinal_2; }
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

            _pBlend = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "mb_Blend.frag", null)
            });
            _pBlend.enable_Samplers(3);
            _pBlend.addUniform("fps_scaler");
        }

        protected override void load_Buffers()
        {

            _tFinal = new Texture(TextureTarget.Texture2D,
                _resolution.W / 4, _resolution.H / 4,
                0, false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tFinal.load();

            _tFinal_2 = new Texture(TextureTarget.Texture2D,
                _resolution.W / 4, _resolution.H / 4,
                0, false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tFinal_2.load();

            _fMotionBlur = new FrameBuffer("Motion Blur");
            _fMotionBlur.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.ColorAttachment0, _tFinal },
                { FramebufferAttachment.ColorAttachment1, _tFinal_2 }
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

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _fMotionBlur.id);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment1);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, scene_fbo.id);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

            GL.BlitFramebuffer(0, 0, _resolution.W, _resolution.H,
                    0, 0, _tFinal.width, _tFinal.height,
                    ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);



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
            _tFinal_2.bind(_pBlur.getSamplerUniform(0), 0);

            quad.render();


            //------------------------------------------------------
            // Pass 2
            //------------------------------------------------------
            _fMotionBlur.bind(DrawBuffersEnum.ColorAttachment1);

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

            quad.render();


            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _fMotionBlur.id);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, scene_fbo.id);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

            GL.BlitFramebuffer(0, 0, _resolution.W, _resolution.H,
                    0, 0, _tFinal.width, _tFinal.height,
                    ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);


            _pBlend.bind();
            GL.Viewport(0, 0, _resolution.W, _resolution.H);
            scene_fbo.bind(DrawBuffersEnum.ColorAttachment0);


            velocity_texture.bind(_pBlend.getSamplerUniform(0), 0);
            _tFinal_2.bind(_pBlend.getSamplerUniform(1), 1);
            scene_texture.bind(_pBlend.getSamplerUniform(2), 2);


            GL.Uniform1(_pBlend.getUniform("fps_scaler"), 60.0f);

            quad.render();


        }


    }
}
