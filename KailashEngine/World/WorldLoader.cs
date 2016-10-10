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


        private string _path_scene;

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


        public WorldLoader(string path_scene, string light_objects_filename)
        {
            // Fill Base Paths
            _path_scene = path_scene;

            // Load standard light object meshes
            Dictionary<string, UniqueMesh> light_objects = new Dictionary<string, UniqueMesh>();
            Dictionary<string, Matrix4> garbage_matrix_collection = new Dictionary<string, Matrix4>();
            try
            {
                DAE_Loader.load(
                    _path_scene + light_objects_filename + "/" + light_objects_filename + ".dae", 
                    out light_objects,
                    out garbage_matrix_collection);
                _sLight_mesh = light_objects["sLight"].mesh;
                _pLight_mesh = light_objects["pLight"].mesh;
                light_objects.Clear();
                garbage_matrix_collection.Clear();
            }
            catch (Exception e)
            {
                Debug.DebugHelper.logError("[ ERROR ] World File: " + light_objects_filename, e.Message);
            }
        }

        private string[] createFilePaths(string filename)
        {
            string mesh_filename = _path_scene + filename + "/" + filename + ".dae";
            string physics_filename = _path_scene + filename + "/" + filename + ".physics";
            string lights_filename = _path_scene + filename + "/" + filename + ".lights";

            return new string[]
            {
                mesh_filename,
                physics_filename,
                lights_filename
            };
        }

        public void createWorld(string filename, out List<UniqueMesh> meshes, out List<Light> lights)
        {
            Debug.DebugHelper.logInfo(1, "Loading World", filename);

            // Build filenames
            string[] filepaths = createFilePaths(filename);
            string mesh_filename = filepaths[0];
            string physics_filename = filepaths[1];
            string lights_filename = filepaths[2];


            Dictionary<string, UniqueMesh> temp_meshes;
            Dictionary<string, Matrix4> light_matrix_collection;

            DAE_Loader.load(
                mesh_filename, 
                out temp_meshes, 
                out light_matrix_collection);
            lights = LightLoader.load(lights_filename, light_matrix_collection, _sLight_mesh, _pLight_mesh);
            meshes = temp_meshes.Values.ToList();

            temp_meshes.Clear();
            light_matrix_collection.Clear();

            Debug.DebugHelper.logInfo(1, "", "");
        }

        //public void createWorld(string filename, out List<UniqueMesh> meshes)
        //{
        //    Debug.DebugHelper.logInfo(1, "Loading World", filename);

        //    // Build filenames
        //    string[] filepaths = createFilePaths(filename);
        //    string mesh_filename = filepaths[0];
        //    string physics_filename = filepaths[1];
        //    string lights_filename = filepaths[2];


        //    Dictionary<string, UniqueMesh> temp_meshes;
        //    Dictionary<string, Matrix4> light_matrix_collection;


        //    DAE_Loader.load(
        //        mesh_filename,
        //        out temp_meshes,
        //        out light_matrix_collection);

        //    meshes = temp_meshes.Values.ToList();


        //    temp_meshes.Clear();
        //    light_matrix_collection.Clear();

        //    Debug.DebugHelper.logInfo(1, "", "");
        //}


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



    }
}
