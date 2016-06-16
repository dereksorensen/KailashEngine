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
        private Program _pLighting_SL;
        private Program _pLighting_PL;
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



        public fx_gBuffer(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }


        protected override void load_Programs()
        {
            string[] geometry_functions = new string[]
            {
                _path_glsl_effect + "helpers/gBuffer_Functions.include"
            };

            _pGeometry = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "gen_gBuffer.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "gen_gBuffer.frag", geometry_functions)
            });
            _pGeometry.enable_MeshLoading();

            _pStencil = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "gBuffer_Stencil.vert", null)
            });
            _pStencil.addUniform(RenderHelper.uModel);

            _pLighting_SL = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "gBuffer_Stencil.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "gBuffer_Lighting_SL.frag", null)
            });
            _pLighting_SL.addUniform(RenderHelper.uModel);
            _pLighting_SL.enable_LightCalculation();
            _pLighting_SL.enable_Samplers(3);

            _pLighting_PL = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "gBuffer_Stencil.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "gBuffer_Lighting_SL.frag", null)
            });
            _pLighting_PL.addUniform(RenderHelper.uModel);
            _pLighting_PL.enable_LightCalculation();

            _pAccumulation = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _pLoader.path_glsl_common + "render_Texture2D.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "gBuffer_Accumulation.frag", null)
            });
            _pAccumulation.enable_Samplers(1);
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

            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, _resolution_full.W, _resolution_full.H);


            _pGeometry.bind();
            scene.render(_pGeometry);
        }


        private void pass_Stencil(Light l)
        {
            _fGBuffer.bind(DrawBuffersEnum.None);

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
            _fGBuffer.bind(DrawBuffersEnum.ColorAttachment7);

            GL.StencilFunc(StencilFunction.Notequal, 0, 0xFF);


            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            _pLighting_SL.bind();

            // Bind gBuffer Textures
            _tNormal_Depth.bind(_pLighting_SL.uniforms["sampler0"], 0);
            _tSpecular.bind(_pLighting_SL.uniforms["sampler1"], 1);



            WorldDrawer.drawLightBounds(l, _pLighting_SL);

        }


        private void pass_pLight()
        {

        }



        public void pass_DeferredShading(Scene scene)
        {
            //------------------------------------------------------
            // Clear Lighting Buffer from last frame
            //------------------------------------------------------
            _fGBuffer.bind(DrawBuffersEnum.ColorAttachment7);
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
                        pass_sLight(l);
                        break;
                }



            }

            GL.Disable(EnableCap.StencilTest);
            GL.Disable(EnableCap.Blend);
            GL.CullFace(CullFaceMode.Back);
            //GL.Disable(EnableCap.DepthTest);
        }

        public void pass_LightAccumulation()
        {

        }
    }
}
