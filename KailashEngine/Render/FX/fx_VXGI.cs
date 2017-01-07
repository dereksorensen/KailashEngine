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
using KailashEngine.Output;
using KailashEngine.World;

namespace KailashEngine.Render.FX
{
    class fx_VXGI : RenderEffect
    {
        // Properties
        private float _vx_volume_dimensions = 256;
        private float _vx_volume_scale = 1.0f;

        // Programs
        private Program _pVoxelize;
        private Program _pConeTrace;

        // Frame Buffers
        private FrameBuffer _fConeTrace;

        // Textures
        private Texture _tVoxelVolume;

        private Texture _tConeTrace;

        public Texture tConeTrace
        {
            get { return _tConeTrace; }
            set { _tConeTrace = value; }
        }



        public fx_VXGI(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }

        protected override void load_Programs()
        {
            string[] geometry_extensions = new string[]
            {
                "#extension GL_ARB_bindless_texture : require"
            };
            string[] cone_trace_helpers = new string[]
            {
                _pLoader.path_glsl_common_helpers + "positionFromDepth.include"
            };

            // Rendering Geometry into voxel volume
            _pVoxelize = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "vxgi_Voxelize.vert", null),
                new ShaderFile(ShaderType.GeometryShader, _path_glsl_effect + "vxgi_Voxelize.geom", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "vxgi_Voxelize.frag", null, geometry_extensions)
            });
            _pVoxelize.enable_MeshLoading();
            _pVoxelize.enable_Samplers(1);
            _pVoxelize.addUniform("vx_volume_dimensions");
            _pVoxelize.addUniform("vx_volume_scale");
            _pVoxelize.addUniform("vx_volume_position");
            _pVoxelize.addUniform("vx_projection");


            // Rendering Geometry into voxel volume
            _pConeTrace = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _pLoader.path_glsl_common + "render_TextureCube.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "vxgi_ConeTrace.frag", cone_trace_helpers)
            });
            _pConeTrace.enable_Samplers(6);
            _pConeTrace.addUniform("vx_volume_dimensions");
            _pConeTrace.addUniform("vx_volume_scale");
            _pConeTrace.addUniform("vx_volume_position");
            _pConeTrace.addUniform("vx_inv_view_perspective");
            _pConeTrace.addUniform("displayVoxels");
            _pConeTrace.addUniform("displayMipLevel");
            _pConeTrace.addUniform("maxMipLevels");
        }

        protected override void load_Buffers()
        {
            _tVoxelVolume = new Texture(TextureTarget.Texture3D,
                (int)_vx_volume_dimensions, (int)_vx_volume_dimensions, (int)_vx_volume_dimensions,
                true, true, 
                PixelInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte, 
                TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tVoxelVolume.load();

            _tConeTrace = new Texture(TextureTarget.Texture2D,
                _resolution.H, _resolution.W, 0,
                false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tConeTrace.load();

            _fConeTrace = new FrameBuffer("VXGI - Cone Trace");
            _fConeTrace.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.ColorAttachment0, _tConeTrace }
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


        public void voxelizeScene(Scene scene, Vector3 camera_position)
        {
            _tVoxelVolume.clear();


            GL.ColorMask(false, false, false, false);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthClamp);


            GL.Viewport(0, 0, _tVoxelVolume.width, _tVoxelVolume.height);

            _pVoxelize.bind();


            float radius = _vx_volume_dimensions;
            Matrix4 voxel_projection = Matrix4.CreateOrthographicOffCenter(-radius, radius, -radius, radius, -radius, radius);
            GL.UniformMatrix4(_pVoxelize.getUniform("vx_projection"), false, ref voxel_projection);

            GL.Uniform1(_pVoxelize.getUniform("vx_volume_dimensions"), _vx_volume_dimensions);
            GL.Uniform1(_pVoxelize.getUniform("vx_volume_scale"), _vx_volume_scale);

            Matrix4 voxel_position = Matrix4.CreateTranslation(Vector3.Zero);
            GL.UniformMatrix4(_pVoxelize.getUniform("vx_volume_position"), false, ref voxel_position);


            _tVoxelVolume.bindImageUnit(_pVoxelize.getSamplerUniform(0), 0, TextureAccess.WriteOnly);



            scene.renderMeshes(BeginMode.Triangles, _pVoxelize);



            GL.DepthMask(true);
            GL.ColorMask(true, true, true, true);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthClamp);
        }


        public void coneTracing(fx_Quad quad, Texture diffuse_texture, Texture normal_texture, Texture specular_texture, SpatialData camera_spatial)
        {
            _fConeTrace.bind(DrawBuffersEnum.ColorAttachment0);

            GL.Viewport(0, 0, _tConeTrace.width, _tConeTrace.height);

            _pConeTrace.bind();

            GL.Uniform1(_pConeTrace.getUniform("vx_volume_dimensions"), _vx_volume_dimensions);
            GL.Uniform1(_pConeTrace.getUniform("vx_volume_scale"), _vx_volume_scale);

            Vector3 voxel_position = camera_spatial.position;
            GL.Uniform3(_pConeTrace.getUniform("vx_volume_position"), voxel_position);


            float scaler = 2.0f * ((float)Math.Pow(2.0, 0.0f));
            Matrix4 temp_voxelViewShift = Matrix4.CreateTranslation(
                new Vector3(-_vx_volume_dimensions / scaler, -_vx_volume_dimensions / scaler, -_vx_volume_dimensions / scaler)) *
                Matrix4.CreateScale(_vx_volume_scale / _vx_volume_dimensions * scaler);


            Matrix4 modelView2 = Matrix4.CreateTranslation(Vector3.Zero + camera_spatial.position);
            Matrix4 invMVP = Matrix4.Invert(temp_voxelViewShift * modelView2 * camera_spatial.model_view * camera_spatial.perspective);
            GL.UniformMatrix4(_pConeTrace.getUniform("vx_inv_view_perspective"), false, ref invMVP);



            GL.Uniform1(_pConeTrace.getUniform("displayVoxels"), 1);
            GL.Uniform1(_pConeTrace.getUniform("displayMipLevel"), 0);
            GL.Uniform1(_pConeTrace.getUniform("maxMipLevels"), _tVoxelVolume.getMaxMipMap());


            diffuse_texture.bind(_pConeTrace.getSamplerUniform(0), 0);
            normal_texture.bind(_pConeTrace.getSamplerUniform(1), 1);
            specular_texture.bind(_pConeTrace.getSamplerUniform(2), 2);

            _tVoxelVolume.bind(_pConeTrace.getSamplerUniform(3), 3);



            quad.renderFullQuad();
        }

    }
}
