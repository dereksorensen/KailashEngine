using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using KailashEngine.Render;
using KailashEngine.World.Model;
using KailashEngine.World.Lights;

namespace KailashEngine.World
{
    class WorldLoader
    {

        
        private string _path_mesh;
        private string _path_physics;
        private string _path_lights;

        private Mesh _sLight_mesh;
        public Mesh sLight_mesh
        {
            get { return _sLight_mesh; }
        }

        private Mesh _pLight_mesh;
        public Mesh pLight_mesh
        {
            get { return _pLight_mesh; }
        }


        public WorldLoader(string path_mesh, string path_physics, string path_lights, string light_objects_filename)
        {
            // Fill Base Paths
            _path_mesh = path_mesh;
            _path_physics = path_physics;
            _path_lights = path_lights;


            // Load standard light object meshes
            Dictionary<string, Matrix4> dirt;
            Dictionary<string, UniqueMesh> light_objects;
            DAE_Loader.load(_path_mesh + light_objects_filename, out light_objects, out dirt);
            _sLight_mesh = light_objects["sLight"].mesh;
            _pLight_mesh = light_objects["pLight"].mesh;
            light_objects.Clear();
        }


        public void createWorld(string filename, out List<UniqueMesh> meshes, out List<Light> lights)
        {
            Debug.DebugHelper.logInfo(1, "Loading World", filename);

            // Build filenames
            string mesh_filename = _path_mesh + filename + ".dae";
            string physics_filename = _path_physics + filename + ".physics";
            string lights_filename = _path_lights + filename + ".lights";

            
            Dictionary<string, UniqueMesh> temp_meshes;
            Dictionary<string, Matrix4> light_matrix_collection;

            DAE_Loader.load(mesh_filename, out temp_meshes, out light_matrix_collection);
            lights = LightLoader.load(lights_filename, light_matrix_collection, _sLight_mesh, _pLight_mesh);
            meshes = temp_meshes.Values.ToList();

            temp_meshes.Clear();
            light_matrix_collection.Clear();

            Debug.DebugHelper.logInfo(1, "", "");
        }

        public void createWorld(string filename, out List<UniqueMesh> meshes)
        {
            Debug.DebugHelper.logInfo(1, "Loading World", filename);

            // Build filenames
            string mesh_filename = _path_mesh + filename + ".dae";
            string physics_filename = _path_physics + filename + ".physics";
            string lights_filename = _path_lights + filename + ".lights";


            Dictionary<string, UniqueMesh> temp_meshes;
            Dictionary<string, Matrix4> light_matrix_collection;


            DAE_Loader.load(
                mesh_filename,
                out temp_meshes,
                out light_matrix_collection);

            meshes = temp_meshes.Values.ToList();

            temp_meshes.Clear();
            light_matrix_collection.Clear();


            Debug.DebugHelper.logInfo(1, "", "");
        }


        public void addWorldToScene(string[] filenames, List<UniqueMesh> meshes, List<Light> lights)
        {
            foreach (string filename in filenames)
            {
                try
                {
                    List<UniqueMesh> temp_meshes;
                    List<Light> temp_lights;

                    createWorld(filename, out temp_meshes, out temp_lights);

                    meshes.AddRange(temp_meshes);
                    lights.AddRange(temp_lights);

                    temp_meshes.Clear();
                    temp_lights.Clear();
                }
                catch (Exception e)
                {
                    Debug.DebugHelper.logError("[ ERROR ] World File: " + filename, e.Message);
                }
            }
        }


        public void loadWorld(string filename, out List<UniqueMesh> mesh_list, out List<Light> light_list)
        {
            try
            {
                createWorld(filename, out mesh_list, out light_list);
            }
            catch (Exception e)
            {
                Debug.DebugHelper.logError("[ ERROR ] World File: " + filename, e.Message);
                mesh_list = null;
                light_list = null;
            }
        }

    }
}
