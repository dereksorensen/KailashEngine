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
    class fx_Special : RenderEffect
    {

        // Programs
        private Program _pBlur_Guass;
        private Program _pBlur_MovingAverage;
        private Program _pBlur_Streak;

        // Frame Buffers
        private FrameBuffer _fSpecial;

        // Textures
        private Texture _tSpecial;
        public Texture tSpecial
        {
            get { return _tSpecial; }
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
            _pBlur_Guass.enable_Samplers(1);
            _pBlur_Guass.addUniform("blur_amount");
            _pBlur_Guass.addUniform("texture_size");

            _pBlur_MovingAverage = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.ComputeShader, _path_glsl_effect + "special_BlurMovingAverage.comp", null)
            });
            _pBlur_MovingAverage.enable_Samplers(2);
            _pBlur_MovingAverage.addUniform("flip");
            _pBlur_MovingAverage.addUniform("kernel");
            _pBlur_MovingAverage.addUniform("destination_scale");

            _pBlur_Streak = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "special_BlurStreak.frag", null)
            });
            _pBlur_Streak.enable_Samplers(1);
            _pBlur_Streak.addUniform("blur_amount");
            _pBlur_Streak.addUniform("iteration");
            _pBlur_Streak.addUniform("size_and_direction");
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


        //------------------------------------------------------
        // Helpers
        //------------------------------------------------------

        private void clearAndBindSpecialFrameBuffer()
        {
            _fSpecial.bind(DrawBuffersEnum.ColorAttachment0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        private void bindExternalTexture(Texture texture_to_bind)
        {
            _fSpecial.bind(DrawBuffersEnum.ColorAttachment1);
            _fSpecial.bindTexture(FramebufferAttachment.ColorAttachment1, texture_to_bind.id);
        }


        //------------------------------------------------------
        // Guassian Blur Functions
        //------------------------------------------------------

        public void blur_Guass(
            fx_Quad quad,
            int blur_amount, float blur_angle,
            Texture texture_to_blur,
            float destination_scale = 1)
        {
            Vector2 angle_mod = EngineHelper.createRotationVector(blur_angle);

            blur_Guass(
                quad,
                blur_amount,
                angle_mod, angle_mod,
                texture_to_blur, _fSpecial, DrawBuffersEnum.ColorAttachment1,
                destination_scale);
        }

        public void blur_Guass(
            fx_Quad quad,
            int blur_amount, float blur_angle,
            Texture texture_to_blur, FrameBuffer texture_frame_buffer, DrawBuffersEnum attachement,
            float destination_scale = 1)
        {
            Vector2 angle_mod = EngineHelper.createRotationVector(blur_angle);

            blur_Guass(
                quad,
                blur_amount,
                angle_mod, angle_mod,
                texture_to_blur, texture_frame_buffer, attachement,
                destination_scale);
        }


        public void blur_Guass(
            fx_Quad quad,
            int blur_amount,
            Texture texture_to_blur,
            float destination_scale = 1)
        {
            blur_Guass(
                quad,
                blur_amount,
                new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f),
                texture_to_blur, _fSpecial, DrawBuffersEnum.ColorAttachment1,
                destination_scale);
        }

        public void blur_Guass(
            fx_Quad quad,
            int blur_amount,
            Texture texture_to_blur, FrameBuffer texture_frame_buffer, DrawBuffersEnum attachement,
            float destination_scale = 1)
        {
            blur_Guass(
                quad,
                blur_amount,
                new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f),
                texture_to_blur, texture_frame_buffer, attachement,
                destination_scale);
        }


        private void blur_Guass(
            fx_Quad quad,
            int blur_amount,
            Vector2 horizontal_mod, Vector2 vertical_mod,
            Texture texture_to_blur, FrameBuffer texture_frame_buffer, DrawBuffersEnum attachement,
            float destination_scale = 1)
        {
            _pBlur_Guass.bind();


            GL.Uniform1(_pBlur_Guass.getUniform("blur_amount"), blur_amount);


            Vector2 texture_to_blur_size = new Vector2(texture_to_blur.width, texture_to_blur.height) * destination_scale;

            Vector2 horizontal_texture_size = new Vector2(1.0f / texture_to_blur_size.X, 1.0f / texture_to_blur_size.Y);
            Vector2 vertical_texture_size = new Vector2(1.0f / _tSpecial.width, 1.0f / _tSpecial.height);


            //------------------------------------------------------
            // Horizontal
            //------------------------------------------------------
            // Bind special texture and clear it
            clearAndBindSpecialFrameBuffer();
            GL.Viewport(0, 0, (int)texture_to_blur_size.X, (int)texture_to_blur_size.Y);

            GL.Uniform2(_pBlur_Guass.getUniform("texture_size"), horizontal_mod * horizontal_texture_size);

            // Source
            texture_to_blur.bind(_pBlur_Guass.getSamplerUniform(0), 0);

            quad.render();


            //------------------------------------------------------
            // Vertical
            //------------------------------------------------------

            // If client supplies framebuffer for desintation, use that. Otherwise attach destination to special framebuffer
            if(texture_frame_buffer.id != _fSpecial.id)
            {
                texture_frame_buffer.bind(attachement);
            }
            else
            {
                bindExternalTexture(texture_to_blur);
            }
            GL.Viewport(0, 0, (int)(_tSpecial.width / destination_scale), (int)(_tSpecial.height / destination_scale));



            GL.Uniform2(_pBlur_Guass.getUniform("texture_size"), vertical_mod * vertical_texture_size);

            // Source
            _tSpecial.bind(_pBlur_Guass.getSamplerUniform(0), 0);

            quad.render();

        }


        //------------------------------------------------------
        // Moving Average Blur Functions
        //------------------------------------------------------

        public void blur_MovingAverage(int blur_amount, Texture texture_to_blur, float destination_scale = 1)
        {
            int thread_group_size = 32;

            _pBlur_MovingAverage.bind();
            clearAndBindSpecialFrameBuffer();

            Vector2 texture_to_blur_size = new Vector2(texture_to_blur.width * destination_scale, texture_to_blur.height * destination_scale);


            GL.Uniform1(_pBlur_MovingAverage.getUniform("kernel"), blur_amount);         

            //------------------------------------------------------
            // Horizontal - 1
            //------------------------------------------------------
            GL.Uniform1(_pBlur_MovingAverage.getUniform("flip"), 0);
            texture_to_blur.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            _tSpecial.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            GL.Uniform1(_pBlur_MovingAverage.getUniform("destination_scale"), destination_scale);
            GL.DispatchCompute(((int)(texture_to_blur.height * destination_scale) + thread_group_size - 1) / thread_group_size, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);


            //------------------------------------------------------
            // Horizontal - 2
            //------------------------------------------------------
            _tSpecial.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            texture_to_blur.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            GL.Uniform1(_pBlur_MovingAverage.getUniform("destination_scale"), 1.0f);
            GL.DispatchCompute((_tSpecial.height + thread_group_size - 1) / thread_group_size, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);


            //------------------------------------------------------
            // Horizontal - 3
            //------------------------------------------------------
            texture_to_blur.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            _tSpecial.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            GL.Uniform1(_pBlur_MovingAverage.getUniform("destination_scale"), destination_scale);
            GL.DispatchCompute(((int)(texture_to_blur.height * destination_scale) + thread_group_size - 1) / thread_group_size, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);


            //------------------------------------------------------
            // Virtical - 1
            //------------------------------------------------------
            GL.Uniform1(_pBlur_MovingAverage.getUniform("flip"), 1);
            _tSpecial.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            texture_to_blur.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            GL.Uniform1(_pBlur_MovingAverage.getUniform("destination_scale"), 1.0f);
            GL.DispatchCompute((_tSpecial.width + thread_group_size - 1) / thread_group_size, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            //------------------------------------------------------
            // Virtical - 2
            //------------------------------------------------------
            texture_to_blur.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            _tSpecial.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            GL.Uniform1(_pBlur_MovingAverage.getUniform("destination_scale"), destination_scale);
            GL.DispatchCompute(((int)(texture_to_blur.width * destination_scale) + thread_group_size - 1) / thread_group_size, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);


            //------------------------------------------------------
            // Virtical - 3
            //------------------------------------------------------
            _tSpecial.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            texture_to_blur.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            GL.Uniform1(_pBlur_MovingAverage.getUniform("destination_scale"), 1.0f);
            GL.DispatchCompute((_tSpecial.width + thread_group_size - 1) / thread_group_size, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }


        //------------------------------------------------------
        // Streak Blur Functions
        //------------------------------------------------------

        public void blur_Streak(
            fx_Quad quad,
            int blur_amount, float streak_angle,
            Texture texture_to_blur,
            float destination_scale = 1)
        {
            blur_Streak(
                quad,
                blur_amount, streak_angle,
                texture_to_blur, _fSpecial, DrawBuffersEnum.ColorAttachment1,
                destination_scale);
        }

        public void blur_Streak(
            fx_Quad quad, 
            int blur_amount, float streak_angle,
            Texture texture_to_blur, FrameBuffer texture_frame_buffer, DrawBuffersEnum attachement, 
            float destination_scale = 1)
        {
            _pBlur_Streak.bind();

            GL.Uniform1(_pBlur_Streak.getUniform("blur_amount"), blur_amount);
            Vector2 rotation_vector = EngineHelper.createRotationVector(streak_angle);


            Vector2 texture_to_blur_size = new Vector2(texture_to_blur.width, texture_to_blur.height) * destination_scale;
            Vector2 destination_texture_size = new Vector2(1.0f / texture_to_blur_size.X, 1.0f / texture_to_blur_size.Y);
            Vector2 source_texture_size = new Vector2(1.0f / _tSpecial.width, 1.0f / _tSpecial.height);

            //------------------------------------------------------
            // Iteration 1
            //------------------------------------------------------
            // Bind special texture and clear it
            clearAndBindSpecialFrameBuffer();
            GL.Viewport(0, 0, (int)texture_to_blur_size.X, (int)texture_to_blur_size.Y);


            GL.Uniform2(_pBlur_Streak.getUniform("size_and_direction"), rotation_vector * destination_texture_size);
            GL.Uniform1(_pBlur_Streak.getUniform("iteration"), 0);
            texture_to_blur.bind(_pBlur_Streak.getSamplerUniform(0), 0);

            quad.render();


            //------------------------------------------------------
            // Iteration 2
            //------------------------------------------------------
            // Bind special texture and clear it
            if (texture_frame_buffer.id != _fSpecial.id)
            {
                texture_frame_buffer.bind(attachement);
            }
            else
            {
                bindExternalTexture(texture_to_blur);
            }
            GL.Viewport(0, 0, (int)(_tSpecial.width / destination_scale), (int)(_tSpecial.height / destination_scale));


            GL.Uniform2(_pBlur_Streak.getUniform("size_and_direction"), rotation_vector * source_texture_size);
            GL.Uniform1(_pBlur_Streak.getUniform("iteration"), 1);
            _tSpecial.bind(_pBlur_Streak.getSamplerUniform(0), 0);


            quad.render();

        }
    }
}
