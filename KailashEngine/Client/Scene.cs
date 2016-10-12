using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.Render;
using KailashEngine.Animation;
using KailashEngine.Physics;
using KailashEngine.World;
using KailashEngine.World.Model;
using KailashEngine.World.Lights;


namespace KailashEngine.Client
{
    class Scene
    {

        private string _path_scene;

        private MatrixStack _MS;

        private WorldLoader _world_loader;


        // Timers
        private Timer _animation_timer;
        public Timer animation_timer
        {
            get { return _animation_timer; }
        }



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


        public Scene(string path_scene)
        {
            _path_scene = path_scene;
            _MS = new MatrixStack();
        

            _meshes = new List<UniqueMesh>();
            _lights = new List<Light>();

            _animation_timer = new Timer();
        }


        //------------------------------------------------------
        // Flashlight
        //------------------------------------------------------

        public void toggleFlashlight(bool enabled)
        {
            if (enabled)
            {
                _lights.Insert(0, _flashlight);
            }
            else
            {
                _lights.RemoveAt(0);
            }
        }

        private void load_Flashlight()
        {
            // Load Flashlight
            _flashlight = new sLight(
                "flashlight",
                new Vector3(1.0f), 2.0f, 40.0f, MathHelper.DegreesToRadians(70.0f), 0.1f,
                false,
                _world_loader.sLight_mesh, Matrix4.Identity);
            
            toggleFlashlight(true);
        }


        //------------------------------------------------------
        // Load Scene
        //------------------------------------------------------

        public void load(Physics.PhysicsWorld physics_world)
        {
            _world_loader = new WorldLoader(_path_scene, "light_objects", physics_world);


            // Load Scenes
            _world_loader.addWorldToScene(new string[]
            {
                "test_scene"
            }, _meshes, _lights);


            load_Flashlight();


            animation_timer.start();
        }


        //------------------------------------------------------
        // Render Scene
        //------------------------------------------------------

        public void render(Program program)
        {
            // Draw Scene
            WorldDrawer.drawMeshes(_meshes, program, Matrix4.Identity, animation_timer.seconds);
            WorldDrawer.drawLights(_lights, program, Matrix4.Identity, false);

        }

    }
}
