using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.Render;
using KailashEngine.World;
using KailashEngine.World.Model;
using KailashEngine.World.Lights;

namespace KailashEngine.Client
{
    class Scene
    {

        private string _path_mesh;
        private string _path_physics;
        private string _path_lights;

        private MatrixStack _MS;

        private WorldLoader _world_loader;


        // Master Lists
        private List<UniqueMesh> _meshes;
        public List<UniqueMesh> meshes
        {
            get { return _meshes; }
            set { _meshes = value; }
        }

        private List<Light> _lights;
        public List<Light> lights
        {
            get { return _lights; }
            set { _lights = value; }
        }


        public Scene(string path_mesh, string path_physics, string path_lights)
        {
            _path_mesh = path_mesh;
            _path_physics = path_physics;
            _path_lights = path_lights;
            _MS = new MatrixStack();
        

            _meshes = new List<UniqueMesh>();
            _lights = new List<Light>();
        }


        public void load()
        {
            _world_loader = new WorldLoader(_path_mesh, _path_physics, _path_lights, "light_objects.dae");


            // Load Scenes
            _world_loader.addWorldToScene(new string[]
            {
                "test_scene"
            }, _meshes, _lights);


        }


        public void render(Program program)
        {
            // Draw Scene
            WorldDrawer.drawMeshes(_meshes, program, Matrix4.Identity);
            WorldDrawer.drawLights(_lights, program, Matrix4.Identity, true);

        }

    }
}
