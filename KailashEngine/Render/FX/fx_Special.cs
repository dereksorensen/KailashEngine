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
            _pBlur_Guass.enable_Samplers(2);
            _pBlur_Guass.addUniform("blur_amount");
            _pBlur_Guass.addUniform("texture_size");
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
        // Guassian Blur Functions
        //------------------------------------------------------

        public void blur_Guass(
            fx_Quad quad,
            float blur_amount, float blur_angle,
            Texture texture_to_blur,
            float destination_scale = 1)
        {
            float angle_rad = MathHelper.DegreesToRadians(blur_angle);
            Vector2 angle_mod = new Vector2((float)Math.Sin(angle_rad), (float)-Math.Cos(angle_rad));

            blur_Guass(
                quad,
                blur_amount,
                angle_mod, angle_mod,
                texture_to_blur, _fSpecial, DrawBuffersEnum.ColorAttachment1,
                destination_scale);
        }

        public void blur_Guass(
            fx_Quad quad,
            float blur_amount, float blur_angle,
            Texture texture_to_blur, FrameBuffer texture_frame_buffer, DrawBuffersEnum attachement,
            float destination_scale = 1)
        {
            float angle_rad = MathHelper.DegreesToRadians(blur_angle);
            Vector2 angle_mod = new Vector2((float)Math.Sin(angle_rad), (float)-Math.Cos(angle_rad));

            blur_Guass(
                quad,
                blur_amount,
                angle_mod, angle_mod,
                texture_to_blur, texture_frame_buffer, attachement,
                destination_scale);
        }


        public void blur_Guass(
            fx_Quad quad,
            float blur_amount,
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
            float blur_amount,
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
            float blur_amount,
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
            _fSpecial.bind(DrawBuffersEnum.ColorAttachment0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
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
                _fSpecial.bind(DrawBuffersEnum.ColorAttachment1);
                _fSpecial.bindTexture(FramebufferAttachment.ColorAttachment1, texture_to_blur.id);
            }
            GL.Viewport(0, 0, (int)(_tSpecial.width / destination_scale), (int)(_tSpecial.height / destination_scale));



            GL.Uniform2(_pBlur_Guass.getUniform("texture_size"), vertical_mod * vertical_texture_size);

            // Source
            _tSpecial.bind(_pBlur_Guass.getSamplerUniform(0), 0);

            quad.render();

        }

    }
}
