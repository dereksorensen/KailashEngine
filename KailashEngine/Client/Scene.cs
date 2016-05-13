using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KailashEngine.World.Scene;

namespace KailashEngine.Client
{
    class Scene
    {

        private string _path_mesh;


        // Scene Objects
        private SceneLoader _box;



        public Scene(string path_mesh)
        {
            _path_mesh = path_mesh;

        }


        public void load()
        {
            _box = new SceneLoader(_path_mesh + "unit_cube.daee");
        }


        public void render()
        {
            
        }

    }
}
