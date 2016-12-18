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
        public UniformBuffer ubo_shadow_spot
        {
            get { return _ubo_shadow_spot; }
        }

        private Dictionary<int, Light> _lights;
        public Dictionary<int, Light> lights
        {
            get { return _lights; }
        }

        public List<Light> light_list
        {
            get { return _lights.Values.ToList(); }
        }


        public LightManager()
        {
            _light_count = 0;
            _max_shadows = 2;
            _lights = new Dictionary<int, Light>();


            _ubo_shadow_spot = new UniformBuffer(OpenTK.Graphics.OpenGL.BufferStorageFlags.DynamicStorageBit, 3, EngineHelper.size.mat4, 4);
        }



        public void addLight(Light light)
        {
            light.lid = _light_count + 1;
            light.enabled = true;
            _lights.Add(light.lid, light);
            _light_count++;
        }

        public void addLight(List<Light> lights)
        {
            foreach(Light light in lights)
            {
                addLight(light);
            }
        }


        public void updateUBO_Shadow(Matrix4 p)
        {
            _max_shadows = Math.Min(_max_shadows, _light_count);
            int index_offset = 0;
            for (int i = 0; i < _max_shadows; i++)
            {
                int index = i + index_offset;

                Matrix4 light_view_matrix_rot = Matrix4.Transpose(light_list[index].spatial.rotation_matrix);
                Matrix4 light_view_matrix_pos = Matrix4.CreateTranslation(-light_list[index].spatial.position);

                _ubo_shadow_spot.update(index * 2, p);
                _ubo_shadow_spot.update(index * 2 + 1, light_view_matrix_pos * light_view_matrix_rot);
            }
        }
    }
}
