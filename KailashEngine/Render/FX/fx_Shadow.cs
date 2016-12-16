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
    class fx_Shadow : RenderEffect
    {

        // Programs
        private Program _pSpot;
        private Program _pPoint;
        private Program _pDirectional;


        // Frame Buffers


        // Textures
        private Texture _tSpot;
        public Texture tSpot
        {
            get { return _tSpot; }
        }



        public fx_Shadow(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }

        protected override void load_Programs()
        {
            // Render to screen and apply tone mapping and gamma correction
            //_pSpot = _pLoader.createProgram(new ShaderFile[]
            //{
            //    new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "final_Scene.frag", null),
            //    new ShaderFile(ShaderType.GeometryShader, _path_glsl_effect + "final_Scene.frag", null),
            //    new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "final_Scene.frag", null)
            //});
            //_pSpot.enable_Samplers(1);
        }

        protected override void load_Buffers()
        {
            _tSpot = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H, 0, 
                false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tSpot.load();

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

        }


    }
}
