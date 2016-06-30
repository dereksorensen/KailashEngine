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
    class fx_HDR : RenderEffect
    {

        // Programs
        private Program _pLuminosity;
        private Program _pAutoExposure;

        // Frame Buffers


        // Textures
        private Texture _tLuminosity;
        public Texture tLuminosity
        {
            get { return _tLuminosity; }
            set { _tLuminosity = value; }
        }

        // Other Buffers
        private ShaderStorageBuffer _ssboExposure;


        public fx_HDR(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }

        protected override void load_Programs()
        {
            _pLuminosity = _pLoader.createProgram(new ShaderFile[]{
                new ShaderFile(ShaderType.ComputeShader, _path_glsl_effect  + "hdr_Luminosity.comp", null)
            });
            _pLuminosity.enable_Samplers(2);
        }

        protected override void load_Buffers()
        {
            //_ssboExposure = new ShaderStorageBuffer(new EngineHelper.size[]
            //{
            //    EngineHelper.size.vec2
            //});

            _tLuminosity = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H,
                0, true, false,
                PixelInternalFormat.R16f, PixelFormat.Red, PixelType.Float,
                TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tLuminosity.load();

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


        private void luminosity(Texture scene_texture)
        {
            int wg_size = 32;

            _pLuminosity.bind();
          
            //scene_texture.bindImageUnit(_pLuminosity.getUniform("sampler0"), 0, TextureAccess.ReadOnly);
            _tLuminosity.bindImageUnit(_pLuminosity.getUniform("sampler1"), 1, TextureAccess.WriteOnly);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindImageTexture(
                0,
                scene_texture.id,
                0,
                false,
                0,
                TextureAccess.ReadOnly,
                SizedInternalFormat.Rgba16f);
            GL.Uniform1(_pLuminosity.getUniform("sampler0"), 0);


            GL.DispatchCompute(_resolution.W / wg_size, _resolution.H / wg_size, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            _tLuminosity.generateMipMap();
        }

        private void autoExposure()
        {

        }

        public void autoExposure(Texture scene_texture)
        {
            luminosity(scene_texture);
            autoExposure();
        }


        public void render(fx_Quad quad)
        {
            
        }


    }
}
