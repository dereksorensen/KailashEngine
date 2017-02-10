using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using KailashEngine.Client;
using KailashEngine.Render.Shader;
using KailashEngine.Render.Objects;
using KailashEngine.World;
using KailashEngine.World.Lights;
using KailashEngine.Output;

namespace KailashEngine.Render.FX
{
    class fx_Shadow : RenderEffect
    {
        // Propteries
        private const int _num_spot_shadows = 4;
        private const int _num_point_shadows = 2;
        private const int _num_directional_shadows = 1;
        private const int _max_mipmaps = 4;

        private const float _texture_scale = 0.5f;
        private int _resolution_shadow;

        // Programs
        private Program _pSpot;
        private Program _pPoint;
        private Program _pDirectional;


        // Frame Buffers
        private FrameBuffer _fSpot;
        private FrameBuffer _fPoint;
        private FrameBuffer _fDirectional;

        // Textures
        private Texture _tDepth_Spot;
        private Texture _tSpot;
        public Texture tSpot
        {
            get { return _tSpot; }
        }


        private Texture _tDepth_Point;
        private Texture _tPoint;
        public Texture tPoint
        {
            get { return _tPoint; }
        }

        private Texture _tDepth_Directional;
        private Texture _tDirectional;
        public Texture tDirectional
        {
            get { return _tDirectional; }
        }


        public fx_Shadow(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        {
            _resolution_shadow = (int)(_resolution.W * _texture_scale);
        }

        protected override void load_Programs()
        {
            string[] culling_helpers = new string[]
            {
                EngineHelper.path_glsl_common_helper_culling
            };
            string[] spot_helpers = new string[]
            {
                EngineHelper.path_glsl_common_helper_shadowMapping,
                EngineHelper.path_glsl_common_helper_linearDepth
            };

            _pSpot = _pLoader.createProgram_Geometry(new ShaderFile[]
            {
                new ShaderFile(ShaderType.GeometryShader, _path_glsl_effect + "shadow_Spot.geom", culling_helpers),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "shadow_Spot.frag", spot_helpers)
            });
            _pSpot.enable_MeshLoading();

            _pPoint = _pLoader.createProgram_Geometry(new ShaderFile[]
            {
                new ShaderFile(ShaderType.GeometryShader, _path_glsl_effect + "shadow_Point.geom", culling_helpers),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "shadow_Point.frag", spot_helpers)
            });
            _pPoint.enable_MeshLoading();

            _pDirectional = _pLoader.createProgram_Geometry(new ShaderFile[]
            {
                new ShaderFile(ShaderType.GeometryShader, _path_glsl_effect + "shadow_Directional.geom", culling_helpers),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "shadow_Directional.frag", spot_helpers)
            });
            _pDirectional.enable_MeshLoading();
        }

        protected override void load_Buffers()
        {
            //------------------------------------------------------
            // Spot Light Buffers
            //------------------------------------------------------
            _tDepth_Spot = new Texture(TextureTarget.Texture2DArray,
                _resolution_shadow, _resolution_shadow, _num_spot_shadows,
                false, false,
                PixelInternalFormat.DepthComponent32f, PixelFormat.DepthComponent, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tDepth_Spot.load();

            _tSpot = new Texture(TextureTarget.Texture2DArray,
                _resolution_shadow, _resolution_shadow, _num_spot_shadows,
                true, true,
                PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tSpot.setMaxMipMap(_max_mipmaps);
            _tSpot.load();

            _fSpot = new FrameBuffer("Shadow - Spot");
            _fSpot.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.DepthAttachment, _tDepth_Spot },
                { FramebufferAttachment.ColorAttachment0, _tSpot },
            });


            //------------------------------------------------------
            // Point Light Buffers
            //------------------------------------------------------
            _tDepth_Point = new Texture(TextureTarget.TextureCubeMapArray,
                _resolution_shadow, _resolution_shadow, _num_point_shadows,
                false, false,
                PixelInternalFormat.DepthComponent32f, PixelFormat.DepthComponent, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tDepth_Point.load();

            _tPoint = new Texture(TextureTarget.TextureCubeMapArray,
                _resolution_shadow, _resolution_shadow, _num_point_shadows,
                true, true,
                PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tPoint.setMaxMipMap(_max_mipmaps);
            _tPoint.load();

            _fPoint = new FrameBuffer("Shadow - Point");
            _fPoint.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.DepthAttachment, _tDepth_Point },
                { FramebufferAttachment.ColorAttachment0, _tPoint },
            });

            //------------------------------------------------------
            // Directional Light Buffers
            //------------------------------------------------------
            _tDepth_Directional = new Texture(TextureTarget.Texture2DArray,
                _resolution_shadow, _resolution_shadow, _num_directional_shadows * 4,
                false, false,
                PixelInternalFormat.DepthComponent32f, PixelFormat.DepthComponent, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tDepth_Directional.load();

            _tDirectional = new Texture(TextureTarget.Texture2DArray,
                _resolution_shadow, _resolution_shadow, _num_directional_shadows * 4,
                true, true,
                PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tDirectional.setMaxMipMap(_max_mipmaps);
            _tDirectional.load();

            _fDirectional = new FrameBuffer("Shadow - Directional");
            _fDirectional.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.DepthAttachment, _tDepth_Directional },
                { FramebufferAttachment.ColorAttachment0, _tDirectional },
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


        private void render_Spot(Scene scene)
        {
            _fSpot.bind(DrawBuffersEnum.ColorAttachment0);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, _tSpot.width, _tSpot.height);

            _pSpot.bind();

            scene.renderMeshes_Basic(BeginMode.Triangles, _pSpot);

            _tSpot.generateMipMap();
        }

        private void render_Point(Scene scene)
        {
            _fPoint.bind(DrawBuffersEnum.ColorAttachment0);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, _tPoint.width, _tPoint.height);

            _pPoint.bind();

            scene.renderMeshes_Basic(BeginMode.Triangles, _pPoint);

            _tPoint.generateMipMap();
        }

        private void render_Directional(Scene scene, SpatialData camera_spatial)
        {
            _fDirectional.bind(DrawBuffersEnum.ColorAttachment0);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, _tDirectional.width, _tDirectional.height);

            _pDirectional.bind();

            scene.renderMeshes_Basic(BeginMode.Triangles, _pDirectional);

            _tDirectional.generateMipMap();
        }

        public void render(Scene scene, SpatialData camera_spatial)
        {
            //GL.Enable(EnableCap.PolygonOffsetFill);
            //GL.PolygonOffset(0.5f, 0.2f);

            render_Spot(scene);
            render_Point(scene);
            render_Directional(scene, camera_spatial);

            //GL.Disable(EnableCap.PolygonOffsetFill);
        }


    }
}
