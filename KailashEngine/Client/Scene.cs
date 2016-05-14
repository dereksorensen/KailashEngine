using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KailashEngine.World;

namespace KailashEngine.Client
{
    class Scene
    {

        private string _path_mesh;


        // Scene Objects
        private WorldLoader _box;



        public Scene(string path_mesh)
        {
            _path_mesh = path_mesh;

        }

        private WorldLoader loadHelper(string filename)
        {
            try
            {
                return new WorldLoader(_path_mesh + filename);
            }
            catch(Exception e)
            {
                Debug.DebugHelper.logError("World Loading Failed", e.Message);
                return null;
            }
            
        }

        public void load()
        {
            _box = loadHelper("unit_cube.dae");
        }


        public void render()
        {
            
        }

    }
}
