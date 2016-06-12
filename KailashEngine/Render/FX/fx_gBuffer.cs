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
        private Texture _tDepthStencil;
        public Texture tDepthStencil
        {
            get { return _tDepthStencil; }
        }

        private Texture _tDiffuse_ID;
        public Texture tDiffuse_ID
        {
            get { return _tDiffuse_ID; }
        }

        private Texture _tNormal_Depth;
        public Texture tNormal_Depth
        {
            get { return _tNormal_Depth; }
        }

        private Texture _tSpecular;
        public Texture tSpecular
        {
            get { return _tSpecular; }
        }

        private Texture _tLighting;
        public Texture tLighting
        {
            get { return _tLighting; }
        }



        public fx_gBuffer(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }


        protected override void load_Programs()
        {
            string[] functions = new string[]
            {
                _path_glsl_effect + "/helpers/gBuffer_Functions.include"
            };

            _pTest = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "/gen_gBuffer.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "/gen_gBuffer.frag", functions)
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


            _tDiffuse_ID = new Texture(TextureTarget.Texture2D,
                _resolution_full.W, _resolution_full.H,
                0, false, false,
                PixelInternalFormat.Rgba16, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tDiffuse_ID.load();

            _tNormal_Depth = new Texture(TextureTarget.Texture2D,
                _resolution_full.W, _resolution_full.H,
                0, false, false,
                PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tNormal_Depth.load();

            _tSpecular = new Texture(TextureTarget.Texture2D,
                _resolution_full.W, _resolution_full.H,
                0, false, false,
                PixelInternalFormat.Rgba16, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tSpecular.load();

            _tLighting = new Texture(TextureTarget.Texture2D,
                _resolution_full.W, _resolution_full.H,
                0, false, false,
                PixelInternalFormat.Rgba16, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tLighting.load();

            _fGBuffer = new FrameBuffer("gBuffer");
            _fGBuffer.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.DepthStencilAttachment, _tDepthStencil },
                { FramebufferAttachment.ColorAttachment0, _tDiffuse_ID },
                { FramebufferAttachment.ColorAttachment1, _tNormal_Depth },
                { FramebufferAttachment.ColorAttachment2, _tSpecular },
                { FramebufferAttachment.ColorAttachment7, _tLighting }
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

        private void pass_Geometry(Scene scene)
        {
            
            _fGBuffer.bind(new DrawBuffersEnum[] 
            {
                DrawBuffersEnum.ColorAttachment0,
                DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2
            });

            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, _resolution_full.W, _resolution_full.H);


            _pTest.bind();
            scene.render(_pTest);

        }


        private void pass_Stencil()
        {

        }

        private void pass_pLight()
        {

        }

        private void pass_sLight()
        {

        }


        public void pass_DeferredShading(Scene scene)
        {
            pass_Geometry(scene);


        }


    }
}
