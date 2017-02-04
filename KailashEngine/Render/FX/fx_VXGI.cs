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
using KailashEngine.World.Lights;

namespace KailashEngine.Render.FX
{
    class fx_VXGI : RenderEffect
    {
        // Properties
        private bool _debug_display_voxels = false;
        private int _debug_display_voxels_mip_level = 0;
        private float _vx_volume_dimensions = 128.0f;
        private float _vx_volume_scale = 30.0f;
        private Matrix4 _vx_shift_matrix;

        // Programs
        private Program _pVoxelize;
        private Program _pRayTrace;
        private Program _pConeTrace;
        private Program _pInjection_SPOT;
        private Program _pResetAlpha;
        private Program _pMipMap;

        // Frame Buffers
        private FrameBuffer _fConeTrace;

        // Textures
        private Texture _tVoxelVolume;
        private Texture _tVoxelVolume_Diffuse;

        private Texture _tConeTrace_Diffuse;
        public Texture tConeTrace_Diffuse
        {
            get { return _tConeTrace_Diffuse; }
        }


        public Texture _tTemp;


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
                _pLoader.path_glsl_common_helpers + "positionFromDepth.include",
                _pLoader.path_glsl_common_helpers + "voxelFunctions.include"
            };
            string[] injection_helpers = new string[]
            {
                _pLoader.path_glsl_common_helpers + "positionFromDepth.include",
                _pLoader.path_glsl_common_helpers + "lightingFunctions.include",
                _pLoader.path_glsl_common_helpers + "shadowEvaluation.include",
                _pLoader.path_glsl_common_helpers + "voxelFunctions.include"
            };
            string[] spot_injection_helpers = new string[]
            {
                _pLoader.path_glsl_common_ubo + "shadowMatrices_Spot.ubo"
            };
            spot_injection_helpers = spot_injection_helpers.Concat(injection_helpers).ToArray();


            // Rendering Geometry into voxel volume
            _pVoxelize = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "vxgi_Voxelize.vert", null),
                new ShaderFile(ShaderType.GeometryShader, _path_glsl_effect + "vxgi_Voxelize.geom", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "vxgi_Voxelize.frag", null, geometry_extensions)
            });
            _pVoxelize.enable_MeshLoading();
            _pVoxelize.enable_Samplers(2);
            _pVoxelize.addUniform("vx_volume_dimensions");
            _pVoxelize.addUniform("vx_volume_scale");
            _pVoxelize.addUniform("vx_volume_position");
            _pVoxelize.addUniform("vx_projection");


            // Cone Trace through voxel volume
            _pConeTrace = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "vxgi_Trace.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "vxgi_ConeTrace.frag", cone_trace_helpers)
            });
            _pConeTrace.enable_Samplers(4);
            _pConeTrace.addUniform("vx_volume_dimensions");
            _pConeTrace.addUniform("vx_volume_scale");
            _pConeTrace.addUniform("vx_volume_position");
            _pConeTrace.addUniform("maxMipLevels");

            // Cone Trace through voxel volume
            _pRayTrace = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "vxgi_Trace.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "vxgi_RayTrace.frag", cone_trace_helpers)
            });
            _pRayTrace.enable_Samplers(1);
            _pRayTrace.addUniform("vx_volume_dimensions");
            _pRayTrace.addUniform("vx_inv_view_perspective");
            _pRayTrace.addUniform("displayMipLevel");

            // Light Injection
            _pInjection_SPOT = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.ComputeShader, _path_glsl_effect + "vxgi_Injection_SPOT.comp", spot_injection_helpers)
            });
            _pInjection_SPOT.enable_Samplers(4);
            _pInjection_SPOT.enable_LightCalculation();
            _pInjection_SPOT.addUniform("texture_size");
            _pInjection_SPOT.addUniform("light_shadow_id");
            _pInjection_SPOT.addUniform("vx_volume_dimensions");
            _pInjection_SPOT.addUniform("vx_volume_scale");
            _pInjection_SPOT.addUniform("vx_volume_position");

            // Reset Alpha
            _pResetAlpha = _pLoader.createProgram_PostProcessing(new ShaderFile[]
            {
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "vxgi_ResetAlpha.frag", null)
            });
            _pResetAlpha.enable_Samplers(2);

            // MipMap
            _pMipMap = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.ComputeShader, _path_glsl_effect + "vxgi_MipMap.comp", null)
            });
            _pMipMap.enable_Samplers(2);
            _pMipMap.addUniform("source_mip_level");
            _pMipMap.addUniform("direction");

        }

        protected override void load_Buffers()
        {
            //------------------------------------------------------
            // Voxel Volumes
            //------------------------------------------------------
            _tVoxelVolume = new Texture(TextureTarget.Texture3D,
                (int)_vx_volume_dimensions, (int)_vx_volume_dimensions, (int)_vx_volume_dimensions,
                true, true,
                PixelInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte,
                TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tVoxelVolume.load();

            _tVoxelVolume_Diffuse = new Texture(TextureTarget.Texture3D,
                (int)_vx_volume_dimensions, (int)_vx_volume_dimensions, (int)_vx_volume_dimensions,
                false, false,
                PixelInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tVoxelVolume_Diffuse.load();

            //------------------------------------------------------
            // Cone Traced Lighting
            //------------------------------------------------------
            _tConeTrace_Diffuse = new Texture(TextureTarget.Texture2D,
                _resolution.W, _resolution.H, 0,
                false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tConeTrace_Diffuse.load();


            _fConeTrace = new FrameBuffer("VXGI - Cone Trace");
            _fConeTrace.load(new Dictionary<FramebufferAttachment, Texture>()
            {
                { FramebufferAttachment.ColorAttachment0, _tConeTrace_Diffuse }
            });

            _tTemp = new Texture(TextureTarget.Texture2D,
                (int)_vx_volume_dimensions, (int)_vx_volume_dimensions, 0,
                false, false,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float,
                TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Clamp);
            _tTemp.load();
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

        //------------------------------------------------------
        // Helpers
        //------------------------------------------------------

        private Vector3 voxelSnap(Vector3 vector)
        {
            Vector3 temp_vector = vector;
            float scaler = (_vx_volume_dimensions) / (_vx_volume_scale * (float)Math.Pow(2.0f, _tVoxelVolume.getMaxMipMap()/1.0f));
            temp_vector *= scaler;
            temp_vector.X = (float)Math.Floor(temp_vector.X);
            temp_vector.Y = (float)Math.Floor(temp_vector.Y);
            temp_vector.Z = (float)Math.Floor(temp_vector.Z);
            temp_vector /= scaler;

            return temp_vector;
            //return Vector3.Zero;
        }


        private void clearVoxelVolumes()
        {
            _tVoxelVolume.clear();
            _tVoxelVolume_Diffuse.clear();
        }


        private void resetVoxelAlpha(fx_Quad quad)
        {
            GL.Viewport(0, 0, _tVoxelVolume.width, _tVoxelVolume.height);

            _pResetAlpha.bind();

            _tVoxelVolume_Diffuse.bind(_pResetAlpha.getSamplerUniform(0), 0);
            _tVoxelVolume.bindImageUnit(_pResetAlpha.getSamplerUniform(1), 1, TextureAccess.ReadWrite);

            quad.render3D((int)_vx_volume_dimensions);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }


        private void mipMap()
        {
            _tVoxelVolume.generateMipMap();


            //_pMipMap.bind();

            //int[] workGroupSize = new int[3];
            //GL.GetProgram(_pMipMap.pID, (GetProgramParameterName)All.ComputeWorkGroupSize, workGroupSize);
            //if (workGroupSize[0] * workGroupSize[1] * workGroupSize[2] == 0) return;

            //for (int direction = 0; direction < _tVoxelVolumes.Length; direction++)
            //{
            //    for (int mip_level = 1; mip_level < _tVoxelVolume.getMaxMipMap(); mip_level++)
            //    {
            //        GL.Uniform1(_pMipMap.getUniform("direction"), direction);
            //        GL.Uniform1(_pMipMap.getUniform("source_mip_level"), mip_level - 1);

            //        _tVoxelVolumes[direction].bind(_pMipMap.getSamplerUniform(0), 0);

            //        _tVoxelVolumes[direction].bindImageUnit(_pMipMap.getSamplerUniform(1), 1, TextureAccess.WriteOnly, mip_level);

            //        GL.DispatchCompute(
            //            ((_tVoxelVolume.width >> mip_level) + workGroupSize[0] - 1) / workGroupSize[0],
            //            ((_tVoxelVolume.width >> mip_level) + workGroupSize[1] - 1) / workGroupSize[1],
            //            ((_tVoxelVolume.width >> mip_level) + workGroupSize[2] - 1) / workGroupSize[2]);

            //        GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit | MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            //    }
            //}
        }


        //------------------------------------------------------
        // Main Functions
        //------------------------------------------------------


        public void voxelizeScene(Scene scene, Vector3 camera_position)
        {

            clearVoxelVolumes();


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
            _tVoxelVolume_Diffuse.bindImageUnit(_pVoxelize.getSamplerUniform(1), 1, TextureAccess.WriteOnly);


            scene.renderMeshes_WithMaterials(BeginMode.Triangles, _pVoxelize);
            scene.renderLightObjects(BeginMode.Triangles, _pVoxelize);


            GL.DepthMask(true);
            GL.ColorMask(true, true, true, true);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthClamp);

        }




        public void lightInjection(Scene scene, fx_Shadow shadow, SpatialData camera_spatial)
        {
            Debug.DebugHelper.time_function("injection", 3, () =>
            {

                int workgroup_size = 32;
                int texture_size = (int)_vx_volume_dimensions;


                _tTemp.clear();

                Light[] lights_spot = scene.light_manager.lights_shadows_spot;


                foreach (Light light in scene.light_manager.lights_shadowed)
                {
                    switch (light.type)
                    {
                        case Light.type_spot:
                            _pInjection_SPOT.bind();

                            sLight temp_sLight = (sLight)light;

                            GL.Uniform3(_pInjection_SPOT.getUniform(RenderHelper.uLightPosition), light.spatial.position);
                            GL.Uniform3(_pInjection_SPOT.getUniform(RenderHelper.uLightDirection), light.spatial.look);
                            GL.Uniform3(_pInjection_SPOT.getUniform(RenderHelper.uLightColor), light.color);
                            GL.Uniform1(_pInjection_SPOT.getUniform(RenderHelper.uLightIntensity), light.intensity);
                            GL.Uniform1(_pInjection_SPOT.getUniform(RenderHelper.uLightFalloff), light.falloff);
                            GL.Uniform1(_pInjection_SPOT.getUniform(RenderHelper.uLightSpotAngle), light.spot_angle);
                            GL.Uniform1(_pInjection_SPOT.getUniform(RenderHelper.uLightSpotBlur), light.spot_blur);

                            GL.Uniform1(_pInjection_SPOT.getUniform("light_shadow_id"), temp_sLight.sid);

                            GL.Uniform2(_pInjection_SPOT.getUniform("texture_size"), new Vector2(texture_size));

                            GL.Uniform1(_pInjection_SPOT.getUniform("vx_volume_dimensions"), _vx_volume_dimensions);
                            GL.Uniform1(_pInjection_SPOT.getUniform("vx_volume_scale"), _vx_volume_scale);
                            GL.Uniform3(_pInjection_SPOT.getUniform("vx_volume_position"), -voxelSnap(camera_spatial.position));

                            _tVoxelVolume.bindImageUnit(_pInjection_SPOT.getSamplerUniform(0), 0, TextureAccess.ReadWrite);
                            _tVoxelVolume_Diffuse.bind(_pInjection_SPOT.getSamplerUniform(1), 1);
                            shadow.tSpot.bind(_pInjection_SPOT.getSamplerUniform(2), 2);
                            _tTemp.bindImageUnit(_pInjection_SPOT.getSamplerUniform(3), 3, TextureAccess.WriteOnly);

                            GL.DispatchCompute((texture_size / workgroup_size), (texture_size / workgroup_size), 1);

                            break;
                        case Light.type_point:

                            break;
                        case Light.type_directional:

                            break;
                    }
                }

                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            });

        }



        public void coneTracing(fx_Quad quad, Texture diffuse_texture, Texture normal_texture, Texture specular_texture, SpatialData camera_spatial)
        {

            Debug.DebugHelper.time_function("Mip Mapping", 1, () =>
            {
                mipMap();
            });


            Debug.DebugHelper.time_function("Cone Tracing", 2, () =>
            {
                _fConeTrace.bind(DrawBuffersEnum.ColorAttachment0);

                GL.Viewport(0, 0, _tConeTrace_Diffuse.width, _tConeTrace_Diffuse.height);

                _pConeTrace.bind();

                GL.Uniform1(_pConeTrace.getUniform("vx_volume_dimensions"), _vx_volume_dimensions);
                GL.Uniform1(_pConeTrace.getUniform("vx_volume_scale"), _vx_volume_scale);

                Vector3 vx_position_snapped = -voxelSnap(camera_spatial.position);
                Matrix4 voxel_volume_position = Matrix4.CreateTranslation(vx_position_snapped);
                GL.Uniform3(_pConeTrace.getUniform("vx_volume_position"), vx_position_snapped);

                GL.Uniform1(_pConeTrace.getUniform("maxMipLevels"), _tVoxelVolume.getMaxMipMap());

                normal_texture.bind(_pConeTrace.getSamplerUniform(0), 0);
                specular_texture.bind(_pConeTrace.getSamplerUniform(1), 1);
                diffuse_texture.bind(_pConeTrace.getSamplerUniform(2), 2);

                _tVoxelVolume.bind(_pConeTrace.getSamplerUniform(3), 3);


                quad.renderFullQuad();
            });
        }


        public void rayTracing(fx_Quad quad, SpatialData camera_spatial)
        {
            if (_debug_display_voxels)
            {

                mipMap();

                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                GL.Viewport(0, 0, _resolution.W, _resolution.H);


                _pRayTrace.bind();

                GL.Uniform1(_pRayTrace.getUniform("vx_volume_dimensions"), _vx_volume_dimensions);

                Vector3 vx_position_snapped = -voxelSnap(camera_spatial.position);
                Matrix4 voxel_volume_position = Matrix4.CreateTranslation(vx_position_snapped);

                Matrix4 invMVP = Matrix4.Invert(_vx_shift_matrix * voxel_volume_position * camera_spatial.model_view * camera_spatial.perspective);
                GL.UniformMatrix4(_pRayTrace.getUniform("vx_inv_view_perspective"), false, ref invMVP);

                GL.Uniform1(_pRayTrace.getUniform("displayMipLevel"), _debug_display_voxels_mip_level);


                _tVoxelVolume.bind(_pRayTrace.getSamplerUniform(0), 0);


                quad.renderFullQuad();

            }
        }


    }
}
