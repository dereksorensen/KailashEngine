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


        private List<UniqueMesh> oscar;
        List<Light> light_oscar;

        public Scene(string path_mesh, string path_physics, string path_lights)
        {
            _path_mesh = path_mesh;
            _path_physics = path_physics;
            _path_lights = path_lights;
            _MS = new MatrixStack();
        

            _meshes = new List<UniqueMesh>();
            _lights = new List<Light>();
        }

        private void addWorldToScene(string[] filenames)
        {
            foreach (string filename in filenames)
            {
                try
                {
                    List<UniqueMesh> temp_meshes;
                    List<Light> temp_lights;

                    _world_loader.createWorld(filename, out temp_meshes, out temp_lights);

                    _meshes.AddRange(temp_meshes);
                    _lights.AddRange(temp_lights);

                    temp_meshes.Clear();
                    temp_lights.Clear();
                }
                catch (Exception e)
                {
                    Debug.DebugHelper.logError("[ ERROR ] World File: " + filename, e.Message);
                }
            }
        }

        private void loadWorld(string filename, out List<UniqueMesh> mesh_list, out List<Light> light_list)
        {
            try
            {
                _world_loader.createWorld(filename, out mesh_list, out light_list);
            }
            catch (Exception e)
            {
                Debug.DebugHelper.logError("[ ERROR ] World File: " + filename, e.Message);
                mesh_list = null;
                light_list = null;
            }
        }

        public void load()
        {
            _world_loader = new WorldLoader(_path_mesh, _path_physics, _path_lights, "light_objects.dae");


            // Load Scenes
            addWorldToScene(new string[]
            {
                "test_scene"
            });


            loadWorld("oscar", out oscar, out light_oscar);

        }


        public void render(Program program)
        {
            WorldDrawer.drawMeshes(_meshes, program, Matrix4.Identity);
            WorldDrawer.drawLights(_lights, program, Matrix4.Identity, true);

            WorldDrawer.drawMeshes(oscar, program, Matrix4.Identity);
        }

    }
}
