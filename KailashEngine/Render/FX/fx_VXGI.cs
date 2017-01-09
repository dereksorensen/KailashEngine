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
        private float _vx_volume_dimensions = 128.0f;
        private float _vx_volume_scale = 50.0f;
        private Matrix4 _vx_shift_matrix;

        // Programs
        private Program _pVoxelize;
        private Program _pConeTrace;

        // Frame Buffers
        private FrameBuffer _fConeTrace;

        // Textures
        public Texture _tVoxelVolume;

        private Texture _tConeTrace;
        public Texture tConeTrace
        {
            get { return _tConeTrace; }
        }



        public fx_VXGI(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        {
            // Shifts the voxel volume for cone tracing
            float vx_shift_scaler = 2.0f;
            float vx_shift_translation = -_vx_volume_dimensions / vx_shift_scaler;
            _vx_shift_matrix = Matrix4.CreateTranslation(new Vector3(vx_shift_translation)) *
                Matrix4.CreateScale(_vx_volume_scale / _vx_volume_dimensions * vx_shift_scaler);
        }

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
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "vxgi_ConeTrace.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "vxgi_ConeTrace.frag", cone_trace_helpers)
            });
            _pConeTrace.enable_Samplers(4);
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
                PixelInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tVoxelVolume.load();


            _tConeTrace = new Texture(TextureTarget.Texture2D,
                _resolution.H, _resolution.W, 0,
                false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
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




        private Vector3 voxelSnap(Vector3 vector)
        {
            Vector3 temp_vector = vector;
            float scaler = (_vx_volume_dimensions)/ (_vx_volume_scale * 10.0f);
            temp_vector *= scaler;
            temp_vector.X = (float)Math.Floor(temp_vector.X);
            temp_vector.Y = (float)Math.Floor(temp_vector.Y);
            temp_vector.Z = (float)Math.Floor(temp_vector.Z);
            temp_vector /= scaler;

            //return temp_vector;
            return Vector3.Zero;
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


            Matrix4 voxel_volume_position = Matrix4.CreateTranslation(voxelSnap(camera_position));
            GL.UniformMatrix4(_pVoxelize.getUniform("vx_volume_position"), false, ref voxel_volume_position);


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
            Debug.DebugHelper.time_function("Mip Mapping", 2, () =>
            {
                _tVoxelVolume.generateMipMap();
            });



            Debug.DebugHelper.time_function("Cone Tracing", 3, () =>
            {
                _fConeTrace.bind(DrawBuffersEnum.ColorAttachment0);

                GL.Viewport(0, 0, _tConeTrace.width, _tConeTrace.height);

                _pConeTrace.bind();

                GL.Uniform1(_pConeTrace.getUniform("vx_volume_dimensions"), _vx_volume_dimensions);
                GL.Uniform1(_pConeTrace.getUniform("vx_volume_scale"), _vx_volume_scale);


                Matrix4 voxel_volume_position = Matrix4.CreateTranslation(-voxelSnap(camera_spatial.position));
                GL.Uniform3(_pConeTrace.getUniform("vx_volume_position"), -voxelSnap(camera_spatial.position));

                Matrix4 invMVP = Matrix4.Invert(_vx_shift_matrix * voxel_volume_position * camera_spatial.model_view * camera_spatial.perspective);
                GL.UniformMatrix4(_pConeTrace.getUniform("vx_inv_view_perspective"), false, ref invMVP);


                GL.Uniform1(_pConeTrace.getUniform("displayVoxels"), 0);
                GL.Uniform1(_pConeTrace.getUniform("displayMipLevel"), 0);
                GL.Uniform1(_pConeTrace.getUniform("maxMipLevels"), _tVoxelVolume.getMaxMipMap());



                normal_texture.bind(_pConeTrace.getSamplerUniform(0), 0);
                specular_texture.bind(_pConeTrace.getSamplerUniform(1), 1);

                _tVoxelVolume.bind(_pConeTrace.getSamplerUniform(2), 2);



                quad.renderFullQuad();
            });
        }

    }
}
