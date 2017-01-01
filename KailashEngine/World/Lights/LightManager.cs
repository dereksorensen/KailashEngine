using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.Render.Objects;

namespace KailashEngine.World.Lights
{
    class LightManager
    {

        private int _light_count;

        private int _max_shadows;
        public int  max_shadows
        {
            get
            {
                return Math.Min(_lights_shadowed.Count, _max_shadows);
            }
        }

        private UniformBuffer _ubo_shadow_spot;
        private UniformBuffer _ubo_shadow_point;
        private UniformBuffer _ubo_shadow_directional;


        private Dictionary<int, Light> _lights;
        public Dictionary<int, Light> lights
        {
            get { return _lights; }
        }

        private List<Light> _lights_enabled;
        public List<Light> lights_enabled
        {
            get { return _lights_enabled; }
        }

        private List<Light> _lights_shadowed;
        public List<Light> lights_shadowed
        {
            get { return _lights_shadowed; }
        }


        public LightManager()
        {
            _light_count = 0;
            _max_shadows = 5;
            _lights = new Dictionary<int, Light>();
            _lights_enabled = new List<Light>();
            _lights_shadowed = new List<Light>();

            _ubo_shadow_spot = new UniformBuffer(OpenTK.Graphics.OpenGL.BufferStorageFlags.DynamicStorageBit, 3, new EngineHelper.size[] {
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.vec4,
            }, 32);

            _ubo_shadow_point = new UniformBuffer(OpenTK.Graphics.OpenGL.BufferStorageFlags.DynamicStorageBit, 4, new EngineHelper.size[] {
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.vec4,
            }, 32);

            _ubo_shadow_directional = new UniformBuffer(OpenTK.Graphics.OpenGL.BufferStorageFlags.DynamicStorageBit, 5, new EngineHelper.size[]
            {
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.vec4,
            }, 1);
        }

        //------------------------------------------------------
        // Build Lists
        //------------------------------------------------------

        public void addLight(Light light)
        {
            light.lid = _light_count;
            light.enabled = true;
            _lights.Add(light.lid, light);
            _light_count++;

            if (light.enabled)
            {
                _lights_enabled.Add(light);
                if(light.shadowed)
                {
                    _lights_shadowed.Add(light);
                }
            }
        }

        public void addLight(List<Light> lights)
        {
            foreach(Light light in lights)
            {
                addLight(light);
            }
        }


        //------------------------------------------------------
        // Updating
        //------------------------------------------------------
        private void updateLists(Vector3 camera_position)
        {
            _lights_enabled = _lights.Select(kp => kp.Value).Where(light => light.enabled).ToList();
            // Get closest N shadow casters to the camera
            _lights_shadowed = _lights_enabled.Where(light => light.shadowed).OrderBy(light => (light.spatial.position - camera_position).Length).ToList();
            _lights_shadowed.ForEach(light => light.sid = -1);
        }

        private void updateUBO_Shadow_Spot(sLight light, int shadow_id)
        {
            int ubo_index = shadow_id * 3;

            _ubo_shadow_spot.update(ubo_index, light.shadow_view_matrix);
            _ubo_shadow_spot.update(ubo_index + 1, light.shadow_perspective_matrix);
            _ubo_shadow_spot.update(ubo_index + 2, light.spatial.position);

            light.sid = shadow_id;
        }

        private void updateUBO_Shadow_Point(pLight light, int shadow_id)
        {
            int ubo_index = shadow_id * 8;

            for(int i = 0; i < 6; i++)
            {
                _ubo_shadow_point.update(ubo_index + i, light.shadow_view_matrices[i]);
            }
            _ubo_shadow_point.update(ubo_index + 6, light.spatial.perspective);
            _ubo_shadow_point.update(ubo_index + 7, light.spatial.position);

            light.sid = shadow_id;
        }

        private void updateUBO_Shadow_Directional(dLight light, int shadow_id)
        {
            int ubo_index = shadow_id * 9;

            for (int i = 0; i < 4; i++)
            {
                _ubo_shadow_directional.update(ubo_index + i, light.shadow_view_matrices[i]);
                _ubo_shadow_directional.update(ubo_index + i + 4, light.shadow_ortho_matrices[i]);
            }
            _ubo_shadow_directional.update(ubo_index + 8, light.spatial.position);

            light.sid = shadow_id;
        }

        private void updateUBO_Shadow()
        {
            int max_shadows_spot = 4;
            int num_shadows_spot = 0;

            int max_shadows_point = 2;
            int num_shadows_point = 0;

            int max_shadows_directional = 1;
            int num_shadows_directional = 0;

            foreach (Light light in _lights_shadowed)
            {
                switch(light.type)
                {
                    case Light.type_spot:
                        if (num_shadows_spot >= max_shadows_spot) break;
                        updateUBO_Shadow_Spot((sLight)light, num_shadows_spot);
                        num_shadows_spot++;
                        break;
                    case Light.type_point:
                        if (num_shadows_point >= max_shadows_point) break;
                        updateUBO_Shadow_Point((pLight)light, num_shadows_point);
                        num_shadows_point++;
                        break;
                    case Light.type_directional:
                        if (num_shadows_directional >= max_shadows_directional) break;
                        updateUBO_Shadow_Directional((dLight)light, num_shadows_directional);
                        num_shadows_directional++;
                        break;
                }
            }
        }

        public void update(Vector3 camera_position)
        {
            updateLists(camera_position);
            updateUBO_Shadow();
        }
    }
}
