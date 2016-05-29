using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using KailashEngine.Client;
using KailashEngine.Output;
using KailashEngine.Render.Shader;
using KailashEngine.Render.Objects;

namespace KailashEngine.Render.FX
{
    class fx_gBuffer : RenderEffect
    {
        // Programs
        private Program _pTest;

        // Frame Buffers
        private FrameBuffer _fGBuffer;

        // Textures
        public Texture _tDepthStencil;

        private Texture _tDiffuse;
        public Texture tDiffuse
        {
            get { return _tDiffuse; }
        }


        public fx_gBuffer(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }


        protected override void load_Programs()
        {
            _pTest = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "/test.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "/test.frag", null)
            });
            _pTest.enable_MeshLoading();
        }

        protected override void load_Buffers()
        {

            _tDepthStencil = new Texture(TextureTarget.Texture2D,
                _resolution_full.W, _resolution_full.H,
                0, false, false,
                PixelInternalFormat.Depth32fStencil8, PixelFormat.DepthComponent, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tDepthStencil.load();


            _tDiffuse = new Texture(TextureTarget.Texture2D,
                _resolution_full.W, _resolution_full.H,
                0, false, false,
                PixelInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tDiffuse.load();


            _fGBuffer = new FrameBuffer("gBuffer");
            _fGBuffer.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.DepthStencilAttachment, _tDepthStencil },
                { FramebufferAttachment.ColorAttachment0, _tDiffuse }
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

        public void render(Scene scene)
        {

            _fGBuffer.bind(DrawBuffersEnum.ColorAttachment0);
            GL.Enable(EnableCap.DepthTest);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, _resolution_full.W, _resolution_full.H);


            _pTest.bind();
            scene.render(_pTest);

        }
    }
}
