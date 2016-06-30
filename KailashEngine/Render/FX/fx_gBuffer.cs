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

using KailashEngine.World;
using KailashEngine.World.Lights;

namespace KailashEngine.Render.FX
{
    class fx_gBuffer : RenderEffect
    {
        // Programs
        private Program _pGeometry;
        private Program _pStencil;
        private Program _pLighting_SPOT;
        private Program _pLighting_POINT;
        private Program _pAccumulation;

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

        private Texture _tLighting_Specular;
        public Texture tLighting_Specular
        {
            get { return _tLighting_Specular; }
        }



        public fx_gBuffer(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }


        protected override void load_Programs()
        {
            string[] geometry_helpers = new string[]
            {
                _path_glsl_effect + "helpers/gBuffer_Functions.include",
                _pLoader.path_glsl_common_helpers + "linearDepth.include"

            };
            string[] lighting_helpers = new string[]
            {
                _path_glsl_effect + "helpers/gBuffer_Lighting.include",
                _pLoader.path_glsl_common_helpers + "linearDepth.include",
                _pLoader.path_glsl_common_helpers + "positionFromDepth.include"
            };

            // Rendering Geometry into gBuffer
            _pGeometry = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "gen_gBuffer.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "gen_gBuffer.frag", geometry_helpers)
            });
            _pGeometry.enable_MeshLoading();

            // Stencil light bounds for lighting pass
            _pStencil = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "gBuffer_Stencil.vert", null)
            });
            _pStencil.addUniform(RenderHelper.uModel);

            // Calculate Lighting for Spot Lights
            _pLighting_SPOT = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "gBuffer_Stencil.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "gBuffer_Lighting_SPOT.frag", lighting_helpers)
            });
            _pLighting_SPOT.addUniform(RenderHelper.uModel);
            _pLighting_SPOT.enable_LightCalculation();
            _pLighting_SPOT.enable_Samplers(3);

            // Calculate Lighting for Point Lights
            _pLighting_POINT = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "gBuffer_Stencil.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "gBuffer_Lighting_POINT.frag", lighting_helpers)
            });
            _pLighting_POINT.addUniform(RenderHelper.uModel);
            _pLighting_POINT.enable_LightCalculation();
            _pLighting_POINT.enable_Samplers(3);

            // Accumulate Lighting
            _pAccumulation = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _pLoader.path_glsl_common + "render_Texture2D.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "gBuffer_Accumulation.frag", null)
            });
            _pAccumulation.enable_Samplers(3);
        }

        protected override void load_Buffers()
        {

            _tDepthStencil = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, false, false,
                PixelInternalFormat.Depth32fStencil8, PixelFormat.DepthComponent, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tDepthStencil.load();

            _tDiffuse_ID = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tDiffuse_ID.load();

            _tNormal_Depth = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, false, false,
                PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tNormal_Depth.load();

            _tSpecular = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, false, false,
                PixelInternalFormat.Rgba16, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tSpecular.load();

            _tLighting = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tLighting.load();

            _tLighting_Specular = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tLighting_Specular.load();

            _fGBuffer = new FrameBuffer("gBuffer");
            _fGBuffer.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.DepthStencilAttachment, _tDepthStencil },
                { FramebufferAttachment.ColorAttachment0, _tDiffuse_ID },
                { FramebufferAttachment.ColorAttachment1, _tNormal_Depth },
                { FramebufferAttachment.ColorAttachment2, _tSpecular },
                { FramebufferAttachment.ColorAttachment6, _tLighting },
                { FramebufferAttachment.ColorAttachment7, _tLighting_Specular }
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

            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, _resolution.W, _resolution.H);


            _pGeometry.bind();
            scene.render(_pGeometry);
        }



        private void pass_Stencil(Light l)
        {
            _fGBuffer.bindAttachements(DrawBuffersEnum.None);

            GL.DepthMask(false);
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.Clear(ClearBufferMask.StencilBufferBit);

            GL.StencilFunc(StencilFunction.Always, 0, 0);
            GL.StencilOpSeparate(StencilFace.Back, StencilOp.Keep, StencilOp.IncrWrap, StencilOp.Keep);
            GL.StencilOpSeparate(StencilFace.Front, StencilOp.Keep, StencilOp.DecrWrap, StencilOp.Keep);

            _pStencil.bind();
            WorldDrawer.drawLightBounds(l, _pStencil);
        }

        private void pass_sLight(Light l)
        {
            _fGBuffer.bindAttachements(new DrawBuffersEnum[] {
                DrawBuffersEnum.ColorAttachment6,
                DrawBuffersEnum.ColorAttachment7
            });
            GL.StencilFunc(StencilFunction.Notequal, 0, 0xFF);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            _pLighting_SPOT.bind();

            // Bind gBuffer Textures
            _tNormal_Depth.bind(_pLighting_SPOT.getUniform("sampler0"), 0);
            _tSpecular.bind(_pLighting_SPOT.getUniform("sampler1"), 1);


            GL.Uniform3(_pLighting_SPOT.getUniform("light_position"), l.spatial.position);
            GL.Uniform3(_pLighting_SPOT.getUniform("light_direction"), l.spatial.look);
            GL.Uniform3(_pLighting_SPOT.getUniform("light_color"), l.color);
            GL.Uniform1(_pLighting_SPOT.getUniform("light_intensity"), l.intensity);
            GL.Uniform1(_pLighting_SPOT.getUniform("light_falloff"), l.falloff);


            WorldDrawer.drawLightBounds(l, _pLighting_SPOT);

        }

        private void pass_pLight(Light l)
        {
            _fGBuffer.bindAttachements(new DrawBuffersEnum[] {
                DrawBuffersEnum.ColorAttachment6,
                DrawBuffersEnum.ColorAttachment7
            });
            GL.StencilFunc(StencilFunction.Notequal, 0, 0xFF);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            _pLighting_POINT.bind();

            // Bind gBuffer Textures
            _tNormal_Depth.bind(_pLighting_POINT.getUniform("sampler0"), 0);
            _tSpecular.bind(_pLighting_POINT.getUniform("sampler1"), 1);

            GL.Uniform3(_pLighting_POINT.getUniform("light_position"), l.spatial.position);
            GL.Uniform3(_pLighting_POINT.getUniform("light_direction"), l.spatial.look);
            GL.Uniform3(_pLighting_POINT.getUniform("light_color"), l.color);
            GL.Uniform1(_pLighting_POINT.getUniform("light_intensity"), l.intensity);
            GL.Uniform1(_pLighting_POINT.getUniform("light_falloff"), l.falloff);

            WorldDrawer.drawLightBounds(l, _pLighting_POINT);
        }



        public void pass_DeferredShading(Scene scene)
        {
            //------------------------------------------------------
            // Clear Lighting Buffer from last frame
            //------------------------------------------------------
            _fGBuffer.bind(new DrawBuffersEnum[] {
                DrawBuffersEnum.ColorAttachment6,
                DrawBuffersEnum.ColorAttachment7
            });
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //------------------------------------------------------
            // Fill gBuffer with Scene
            //------------------------------------------------------
            pass_Geometry(scene);

            //------------------------------------------------------
            // Accumulate Lighting from Scene
            //------------------------------------------------------
            GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
            foreach (Light l in scene.lights)
            {
                pass_Stencil(l);
           
                switch (l.type)
                {
                    case Light.type_spot:
                        pass_sLight(l);
                        break;

                    case Light.type_point:
                        pass_pLight(l);
                        break;                  
                }
            }


            GL.Disable(EnableCap.StencilTest);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            //GL.Disable(EnableCap.DepthTest);
        }

        public void pass_LightAccumulation(fx_Quad quad, FrameBuffer fFinalScene)
        {
            fFinalScene.bind(DrawBuffersEnum.ColorAttachment0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Viewport(0, 0, _resolution.W, _resolution.H);

            _pAccumulation.bind();

            _tLighting.bind(_pAccumulation.getUniform("sampler0"), 0);
            _tLighting_Specular.bind(_pAccumulation.getUniform("sampler1"), 1);
            _tDiffuse_ID.bind(_pAccumulation.getUniform("sampler2"), 2);

            quad.render();
        }
    }
}
