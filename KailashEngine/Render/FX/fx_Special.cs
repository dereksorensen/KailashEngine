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
        private Program _pBlur_Gauss;
        private Program _pBlur_GaussCompute;
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

            _pBlur_Gauss = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "special_BlurGauss.frag", null)
            });
            _pBlur_Gauss.enable_Samplers(1);
            _pBlur_Gauss.addUniform("blur_amount");
            _pBlur_Gauss.addUniform("texture_size");

            _pBlur_GaussCompute = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.ComputeShader, _path_glsl_effect + "special_BlurGauss.comp", null)
            });
            _pBlur_GaussCompute.enable_Samplers(2);
            _pBlur_GaussCompute.addUniform("counter");
            _pBlur_GaussCompute.addUniform("blur_amount");
            _pBlur_GaussCompute.addUniform("texture_size");
            _pBlur_GaussCompute.addUniform("direction_selector");


            _pBlur_MovingAverage = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.ComputeShader, _path_glsl_effect + "special_BlurMovingAverage.comp", null)
            });
            _pBlur_MovingAverage.enable_Samplers(2);
            _pBlur_MovingAverage.addUniform("flip");
            _pBlur_MovingAverage.addUniform("kernel");
            _pBlur_MovingAverage.addUniform("texture_size");

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
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
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
        // Gaussian Blur Functions
        //------------------------------------------------------

        public void blur_Gauss(
            fx_Quad quad,
            int blur_amount, float blur_angle,
            Texture texture_to_blur,
            float destination_scale = 1)
        {
            Vector2 angle_mod = EngineHelper.createRotationVector(blur_angle);

            blur_Gauss(
                quad,
                blur_amount,
                angle_mod, angle_mod,
                texture_to_blur, _fSpecial, DrawBuffersEnum.ColorAttachment1,
                destination_scale);
        }

        public void blur_Gauss(
            fx_Quad quad,
            int blur_amount, float blur_angle,
            Texture texture_to_blur, FrameBuffer texture_frame_buffer, DrawBuffersEnum attachement,
            float destination_scale = 1)
        {
            Vector2 angle_mod = EngineHelper.createRotationVector(blur_angle);

            blur_Gauss(
                quad,
                blur_amount,
                angle_mod, angle_mod,
                texture_to_blur, texture_frame_buffer, attachement,
                destination_scale);
        }


        public void blur_Gauss(
            fx_Quad quad,
            int blur_amount,
            Texture texture_to_blur,
            float destination_scale = 1)
        {
            blur_Gauss(
                quad,
                blur_amount,
                new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f),
                texture_to_blur, _fSpecial, DrawBuffersEnum.ColorAttachment1,
                destination_scale);
        }

        public void blur_Gauss(
            fx_Quad quad,
            int blur_amount,
            Texture texture_to_blur, FrameBuffer texture_frame_buffer, DrawBuffersEnum attachement,
            float destination_scale = 1)
        {
            blur_Gauss(
                quad,
                blur_amount,
                new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f),
                texture_to_blur, texture_frame_buffer, attachement,
                destination_scale);
        }


        private void blur_Gauss(
            fx_Quad quad,
            int blur_amount,
            Vector2 horizontal_mod, Vector2 vertical_mod,
            Texture texture_to_blur, FrameBuffer texture_frame_buffer, DrawBuffersEnum attachement,
            float destination_scale = 1)
        {
            _pBlur_Gauss.bind();


            GL.Uniform1(_pBlur_Gauss.getUniform("blur_amount"), blur_amount);


            Vector2 texture_to_blur_size = new Vector2(texture_to_blur.width, texture_to_blur.height) * destination_scale;

            Vector2 horizontal_texture_size = new Vector2(1.0f / texture_to_blur_size.X, 1.0f / texture_to_blur_size.Y);
            Vector2 vertical_texture_size = new Vector2(1.0f / _tSpecial.width, 1.0f / _tSpecial.height);


            //------------------------------------------------------
            // Horizontal
            //------------------------------------------------------
            // Bind special texture and clear it
            clearAndBindSpecialFrameBuffer();
            GL.Viewport(0, 0, (int)texture_to_blur_size.X, (int)texture_to_blur_size.Y);

            GL.Uniform2(_pBlur_Gauss.getUniform("texture_size"), horizontal_mod * horizontal_texture_size);

            // Source
            texture_to_blur.bind(_pBlur_Gauss.getSamplerUniform(0), 0);

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



            GL.Uniform2(_pBlur_Gauss.getUniform("texture_size"), vertical_mod * vertical_texture_size);

            // Source
            _tSpecial.bind(_pBlur_Gauss.getSamplerUniform(0), 0);

            quad.render();

        }


        //------------------------------------------------------
        // Gaussian Blur Compute Functions
        //------------------------------------------------------

        public void blur_GaussCompute(
            int blur_amount,
            Texture texture_to_blur)
        {

            clearAndBindSpecialFrameBuffer();


            _pBlur_GaussCompute.bind();
            GL.Uniform1(_pBlur_GaussCompute.getUniform("blur_amount"), blur_amount / 2);
            GL.Uniform2(_pBlur_GaussCompute.getUniform("texture_size"), texture_to_blur.dimensions.Xy);

            //------------------------------------------------------
            // Horizontal - 1
            //------------------------------------------------------
            GL.Uniform1(_pBlur_GaussCompute.getUniform("direction_selector"), 0);
            texture_to_blur.bind(_pBlur_GaussCompute.getSamplerUniform(0), 0);
            _tSpecial.bindImageUnit(_pBlur_GaussCompute.getSamplerUniform(1), 1, TextureAccess.WriteOnly);


            GL.Uniform1(_pBlur_GaussCompute.getUniform("counter"), 0);
            GL.DispatchCompute((int)texture_to_blur.dimensions.Y, 1, 1);

            GL.Uniform1(_pBlur_GaussCompute.getUniform("counter"), 1);
            GL.DispatchCompute((int)texture_to_blur.dimensions.Y, 1, 1);

            //GL.Uniform1(_pBlur_GaussCompute.getUniform("counter"), 2);
            //GL.DispatchCompute((int)texture_to_blur.dimensions.Y, 1, 1);

            //GL.Uniform1(_pBlur_GaussCompute.getUniform("counter"), 3);
            //GL.DispatchCompute((int)texture_to_blur.dimensions.Y, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);


            //------------------------------------------------------
            // Vertical
            //------------------------------------------------------
            GL.Uniform1(_pBlur_GaussCompute.getUniform("direction_selector"), 1);
            _tSpecial.bind(_pBlur_GaussCompute.getSamplerUniform(0), 0);
            texture_to_blur.bindImageUnit(_pBlur_GaussCompute.getSamplerUniform(1), 1, TextureAccess.WriteOnly);

            GL.Uniform1(_pBlur_GaussCompute.getUniform("counter"), 0);
            GL.DispatchCompute((int)texture_to_blur.dimensions.X, 1, 1);

            GL.Uniform1(_pBlur_GaussCompute.getUniform("counter"), 1);
            GL.DispatchCompute((int)texture_to_blur.dimensions.X, 1, 1);

            //GL.Uniform1(_pBlur_GaussCompute.getUniform("counter"), 2);
            //GL.DispatchCompute((int)texture_to_blur.dimensions.X, 1, 1);

            //GL.Uniform1(_pBlur_GaussCompute.getUniform("counter"), 3);
            //GL.DispatchCompute((int)texture_to_blur.dimensions.X, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

        }


        //------------------------------------------------------
        // Moving Average Blur Functions
        //------------------------------------------------------

        public void blur_MovingAverage(int blur_amount, Texture texture_to_blur)
        {
            _pBlur_MovingAverage.bind();
            clearAndBindSpecialFrameBuffer();


            int thread_group_size = 16;
            Vector2 num_compute_groups_destination = new Vector2(
                ((texture_to_blur.width) + thread_group_size - 1) / thread_group_size,
                ((texture_to_blur.height) + thread_group_size - 1) / thread_group_size);
            Vector2 num_compute_groups_special = new Vector2(
                ((texture_to_blur.width) + thread_group_size - 1) / thread_group_size,
                ((texture_to_blur.height) + thread_group_size - 1) / thread_group_size);


            GL.Uniform1(_pBlur_MovingAverage.getUniform("kernel"), blur_amount);
            GL.Uniform2(_pBlur_MovingAverage.getUniform("texture_size"), texture_to_blur.dimensions.Xy);

            //------------------------------------------------------
            // Horizontal - 1
            //------------------------------------------------------
            GL.Uniform1(_pBlur_MovingAverage.getUniform("flip"), 0);
            texture_to_blur.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            _tSpecial.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);
            
            GL.DispatchCompute((int)num_compute_groups_destination.Y, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);


            //------------------------------------------------------
            // Horizontal - 2
            //------------------------------------------------------
            _tSpecial.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            texture_to_blur.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);
            
            GL.DispatchCompute((int)num_compute_groups_special.Y, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);


            //------------------------------------------------------
            // Horizontal - 3
            //------------------------------------------------------
            texture_to_blur.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            _tSpecial.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);
            
            GL.DispatchCompute((int)num_compute_groups_destination.Y, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);


            //------------------------------------------------------
            // Virtical - 1
            //------------------------------------------------------
            GL.Uniform1(_pBlur_MovingAverage.getUniform("flip"), 1);
            _tSpecial.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            texture_to_blur.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);
            
            GL.DispatchCompute((int)num_compute_groups_special.X, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            //------------------------------------------------------
            // Virtical - 2
            //------------------------------------------------------
            texture_to_blur.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            _tSpecial.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);
            
            GL.DispatchCompute((int)num_compute_groups_destination.X, 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);


            //------------------------------------------------------
            // Virtical - 3
            //------------------------------------------------------
            _tSpecial.bind(_pBlur_MovingAverage.getSamplerUniform(0), 0);
            texture_to_blur.bindImageUnit(_pBlur_MovingAverage.getSamplerUniform(1), 1, TextureAccess.WriteOnly);
            
            GL.DispatchCompute((int)num_compute_groups_special.X, 1, 1);

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
