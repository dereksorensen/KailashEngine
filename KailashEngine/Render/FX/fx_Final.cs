using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

using KailashEngine.Render.Shader;
using KailashEngine.Render.Objects;
using KailashEngine.Output;

namespace KailashEngine.Render.FX
{
    class fx_Final : RenderEffect
    {

        // Programs
        private Program _pFinalScene;

        // Frame Buffers
        private FrameBuffer _fFinalScene;
        public FrameBuffer fFinalScene
        {
            get
            {
                return _fFinalScene;
            }
        }
        
        // Textures
        private Texture _tFinalScene;
        public Texture tFinalScene
        {
            get
            {
                return _tFinalScene;
            }
        }


        public fx_Final(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }

        protected override void load_Programs()
        {
            // Accumulate Lighting
            _pFinalScene = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _pLoader.path_glsl_common + "render_Texture2D.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "final_Scene.frag", null)
            });
            _pFinalScene.enable_Samplers(1);
        }

        protected override void load_Buffers()
        {
            _tFinalScene = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tFinalScene.load();

            _fFinalScene = new FrameBuffer("Final Scene");
            _fFinalScene.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.ColorAttachment0, _tFinalScene }
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
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Viewport(0, 0, _resolution.W, _resolution.H);

            _pFinalScene.bind();

            _tFinalScene.bind(_pFinalScene.getUniform("sampler0"), 0);

            quad.render();
        }


    }
}
