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
            get { return _max_shadows; }
        }


        private UniformBuffer _ubo_shadow_spot;


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
            _max_shadows = 4;
            _lights = new Dictionary<int, Light>();
            _lights_enabled = new List<Light>();
            _lights_shadowed = new List<Light>();

            _ubo_shadow_spot = new UniformBuffer(OpenTK.Graphics.OpenGL.BufferStorageFlags.DynamicStorageBit, 3, new EngineHelper.size[] {
                EngineHelper.size.mat4,
                EngineHelper.size.mat4,
                EngineHelper.size.vec4,
            }, 32);
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
        private void updateLists()
        {
            _lights_enabled = _lights.Select(kp => kp.Value).Where(light => light.enabled).ToList();
            _lights_shadowed = _lights_enabled.Where(light => light.shadowed).ToList();
        }

        private void updateUBO_Shadow_Spot(sLight light, int shadow_id)
        {
            int ubo_index = shadow_id * 3;

            _ubo_shadow_spot.update(ubo_index, light.shadow_view_matrix);
            _ubo_shadow_spot.update(ubo_index + 1, light.spatial.perspective);
            _ubo_shadow_spot.update(ubo_index + 2, light.spatial.position);

            light.sid = shadow_id;
        }

        private void updateUBO_Shadow_Point()
        {

        }

        private void updateUBO_Shadow()
        {
            int max_shadows_spot = Math.Min(_max_shadows, _light_count);
            int num_shadows_spot = 0;

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

                        break;
                }
            }
        }

        public void update()
        {
            updateLists();
            updateUBO_Shadow();
            updateUBO_Shadow_Point();
        }
    }
}
