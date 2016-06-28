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

        private Light _flashlight;
        public Light flashlight
        {
            get { return _flashlight; }
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





            // Load Flashlight
            _flashlight = new sLight(
                "flashlight",
                Vector3.Zero, Vector3.Zero,
                1.0f,
                new Vector3(1.0f), 10.0f, 30.0f, MathHelper.DegreesToRadians(70.0f),
                false);

            // Create Light Object Mesh
            _flashlight.unique_mesh = new UniqueMesh(_flashlight.id, _world_loader.sLight_mesh, Matrix4.Identity);

            // Create Light Bounds Mesh
            float spot_depth = _flashlight.falloff / 2.0f;
            float spot_radius = spot_depth * (float)Math.Tan(_flashlight.spot_angle) / 2.0f;
            Vector3 scaler = new Vector3(
                    spot_radius,
                    spot_radius,
                    spot_depth
                );
            Vector3 shifter = new Vector3(
                    0.0f,
                    0.0f,
                    -scaler.Z
                );
            Matrix4 temp_matrix = Matrix4.CreateScale(scaler) * Matrix4.CreateTranslation(shifter);
            _flashlight.bounding_unique_mesh = new UniqueMesh(_flashlight.id + "-bounds", _world_loader.sLight_mesh, temp_matrix);

            _lights.Insert(0, _flashlight);

        }

        public void toggleFlashlight(bool enabled)
        {
            if(enabled)
            {
                _lights.Insert(0, _flashlight);
            }
            else
            {
                _lights.RemoveAt(0);
            }
            
        }


        public void render(Program program)
        {
            // Draw Scene
            WorldDrawer.drawMeshes(_meshes, program, Matrix4.Identity);
            WorldDrawer.drawLights(_lights, program, Matrix4.Identity, false);

        }

    }
}
