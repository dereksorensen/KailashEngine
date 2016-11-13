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
    class fx_DepthOfField : RenderEffect
    {

        // Programs
        private Program _pAutoFocus;
        private Program _pCOC;
        private Program _pCOC_Fix;

        // Frame Buffers
        private FrameBuffer _fDepthOfField;

        // Textures
        private const float coc_texture_scale = 0.5f;

        private Texture _tCOC;
        public Texture tCOC
        {
            get { return _tCOC; }
        }

        private Texture _tCOC_Foreground;
        public Texture tCOC_Foreground
        {
            get { return _tCOC_Foreground; }
        }

        private Texture _tCOC_Foreground_2;
        public Texture tCOC_Foreground_2
        {
            get { return _tCOC_Foreground_2; }
        }


        // Other Buffers
        private ShaderStorageBuffer _ssboAutoFocus;


        public fx_DepthOfField(ProgramLoader pLoader, StaticImageLoader tLoader, string resource_folder_name, Resolution full_resolution)
            : base(pLoader, tLoader, resource_folder_name, full_resolution)
        { }

        protected override void load_Programs()
        {
            string[] autofocus_helpers = new string[]
            {
                _pLoader.path_glsl_common_helpers + "interpolation.include"

            };
            _pAutoFocus = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.ComputeShader, _path_glsl_effect + "dof_AutoFocus.comp", autofocus_helpers)
            });
            _pAutoFocus.enable_Samplers(1);
            _pAutoFocus.addUniform("focus_delay");

            _pCOC = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "dof_COC.frag", null)
            });
            _pCOC.enable_Samplers(1);
            _pCOC.addUniform("PPM");
            _pCOC.addUniform("focus_length");
            _pCOC.addUniform("fStop");
            _pCOC.addUniform("max_blur");

            _pCOC_Fix = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "dof_COC_Fix.frag", null)
            });
            _pCOC_Fix.enable_Samplers(2);

        }

        protected override void load_Buffers()
        {
            _ssboAutoFocus = new ShaderStorageBuffer(new EngineHelper.size[]
            {
                EngineHelper.size.vec2
            });


            Vector2 coc_resolution = new Vector2(_resolution.W * coc_texture_scale, _resolution.H * coc_texture_scale);

            _tCOC = new Texture(TextureTarget.Texture2D,
                (int)coc_resolution.X, (int)coc_resolution.Y,
                0, false, false,
                PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tCOC.load();

            _tCOC_Foreground = new Texture(TextureTarget.Texture2D,
                (int)coc_resolution.X, (int)coc_resolution.Y,
                0, false, false,
                PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tCOC_Foreground.load();

            _tCOC_Foreground_2 = new Texture(TextureTarget.Texture2D,
                (int)coc_resolution.X, (int)coc_resolution.Y,
                0, false, false,
                PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tCOC_Foreground_2.load();

            _fDepthOfField = new FrameBuffer("Depth Of Field");
            _fDepthOfField.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.ColorAttachment0, _tCOC },
                { FramebufferAttachment.ColorAttachment1, _tCOC_Foreground },
                { FramebufferAttachment.ColorAttachment2, _tCOC_Foreground_2 }
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

        //------------------------------------------------------
        // Auto Focus
        //------------------------------------------------------
        private void autoFocus(Texture depth_texture)
        {
            _pAutoFocus.bind();

            _ssboAutoFocus.bind(0);
            depth_texture.bind(_pAutoFocus.getSamplerUniform(0), 0);

            GL.DispatchCompute(1, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }

        private void printFocusDistance()
        {
            int exp_size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector2));

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            Vector2 lumRead = new Vector2();

            _ssboAutoFocus.bind();
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)0, exp_size, ref lumRead);

            Debug.DebugHelper.logInfo(1, "Focus Distance", lumRead.ToString());
        }

        //------------------------------------------------------
        // Bokeh
        //------------------------------------------------------
        private void calcCOC(fx_Quad quad, fx_Special special, Texture depth_texture, Texture scene_texture)
        {
            //------------------------------------------------------
            // Calculate COC
            //------------------------------------------------------

            Vector2 coc_resolution = new Vector2(_resolution.W * coc_texture_scale, _resolution.H * coc_texture_scale);

            _fDepthOfField.bind(new DrawBuffersEnum[]
            {
                DrawBuffersEnum.ColorAttachment0,
                DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2
            });
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, (int)coc_resolution.X, (int)coc_resolution.Y);

            _pCOC.bind();

            float max_blur = 80.0f;
            float focus_length = 0.2f;
            float fStop = 0.1f;
            float sensor_width = 33.0f;

            float PPM = (float)Math.Sqrt((coc_resolution.X * coc_resolution.X) + (coc_resolution.Y * coc_resolution.Y)) / sensor_width;

            _ssboAutoFocus.bind(0);
            depth_texture.bind(_pCOC.getSamplerUniform(0), 0);

            GL.Uniform1(_pCOC.getUniform("PPM"), PPM);
            GL.Uniform1(_pCOC.getUniform("focus_length"), focus_length);
            GL.Uniform1(_pCOC.getUniform("fStop"), fStop);
            GL.Uniform1(_pCOC.getUniform("max_blur"), max_blur);

            quad.render();

            special.blur_Guass(quad, 50, _tCOC_Foreground, _fDepthOfField, DrawBuffersEnum.ColorAttachment1, 0.25f);

            //------------------------------------------------------
            // Fix COC
            //------------------------------------------------------

            _fDepthOfField.bind(new DrawBuffersEnum[]
            {
                DrawBuffersEnum.ColorAttachment0,
            });
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, (int)coc_resolution.X, (int)coc_resolution.Y);

            _pCOC_Fix.bind();

            _tCOC_Foreground.bind(_pCOC_Fix.getSamplerUniform(0), 0);
            _tCOC_Foreground_2.bind(_pCOC_Fix.getSamplerUniform(1), 1);

            quad.render();
        }


        //------------------------------------------------------
        // Depth Of Field
        //------------------------------------------------------



        public void render(fx_Quad quad, fx_Special special, Texture depth_texture, Texture scene_texture)
        {
            autoFocus(depth_texture);
            //printFocusDistance();
            calcCOC(quad, special, depth_texture, scene_texture);
        }
    }
}
