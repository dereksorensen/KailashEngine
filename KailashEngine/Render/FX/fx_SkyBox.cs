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
    class fx_SkyBox : RenderEffect
    {

        // Programs
        private Program _pSkyBox;

        // Frame Buffers
        private FrameBuffer _fSkyBox;

        // Textures
        private Image _iSkyBox;
        public Image iSkyBox
        {
            get { return _iSkyBox; }
        }


        public fx_SkyBox(ProgramLoader pLoader, StaticImageLoader tLoader, string resource_folder_name, Resolution full_resolution)
            : base(pLoader, tLoader, resource_folder_name, full_resolution)
        { }

        protected override void load_Programs()
        {
            _pSkyBox = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _pLoader.path_glsl_common + "render_TextureCube.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "skybox_Render.frag", null)
            });
            _pSkyBox.enable_Samplers(1);
        }

        protected override void load_Buffers()
        {
            // Load Lens Images
            _iSkyBox = _tLoader.createImage(
                new string[]{
                    _path_static_textures + "space_right1.png",
                    _path_static_textures + "space_left2.png",
                    _path_static_textures + "space_top3.png",
                    _path_static_textures + "space_bottom4.png",
                    _path_static_textures + "space_front5.png",
                    _path_static_textures + "space_back6.png"
                }, TextureTarget.TextureCubeMap, TextureWrapMode.ClampToEdge, true);
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


        public void render(fx_Quad quad, FrameBuffer scene_fbo)
        {
            scene_fbo.bind(DrawBuffersEnum.ColorAttachment0);
            GL.Viewport(0, 0, _resolution.W, _resolution.H);

            _pSkyBox.bind();

            _iSkyBox.bind(_pSkyBox.getSamplerUniform(0), 0);

            quad.render();
        }
    }
}
