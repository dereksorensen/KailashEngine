using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.World.Lights
{
    class LightManager
    {

        private int _light_count;


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
            _lights = new Dictionary<int, Light>();
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
    }
}
