using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

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

        private MaterialManager _material_manager;

        private LightManager _light_manager;
        public LightManager light_manager
        {
            get { return _light_manager; }
        }


        // Timers
        private Timer _animation_timer;
        public Timer animation_timer
        {
            get { return _animation_timer; }
        }

        private CircadianTimer _circadian_timer;
        public CircadianTimer circadian_timer
        {
            get { return _circadian_timer; }
        }



        // Master Lists
        private List<UniqueMesh> _meshes;
        public List<UniqueMesh> meshes
        {
            get { return _meshes; }
            set { _meshes = value; }
        }


        public List<Light> lights
        {
            get { return _light_manager.light_list; }
        }


        private Light _flashlight;
        public Light flashlight
        {
            get { return _flashlight; }
        }


        //------------------------------------------------------
        // Constructor
        //------------------------------------------------------

        public Scene(string path_scene)
        {
            _path_scene = path_scene;

            _meshes = new List<UniqueMesh>();

            _animation_timer = new Timer();
            _circadian_timer = new CircadianTimer(21.0f, 2.0f);
        }


        //------------------------------------------------------
        // Flashlight
        //------------------------------------------------------

        public void toggleFlashlight(bool enabled)
        {
            //if (enabled)
            //{
            //    _lights.Insert(0, _flashlight);
            //}
            //else
            //{
            //    _lights.RemoveAt(0);
            //}
            _flashlight.enabled = enabled;
        }

        private void load_Flashlight()
        {
            // Load Flashlight
            _flashlight = new sLight(
                "flashlight",
                new Vector3(1.0f), 2.0f, 40.0f, MathHelper.DegreesToRadians(70.0f), 0.1f,
                false,
                _world_loader.sLight_mesh, Matrix4.Identity);

            _light_manager.addLight(_flashlight);
            toggleFlashlight(true);
        }


        //------------------------------------------------------
        // Load Scene
        //------------------------------------------------------

        public void load(PhysicsWorld physics_world)
        {
            _material_manager = new MaterialManager();
            _light_manager = new LightManager();
            _world_loader = new WorldLoader(_path_scene, "light_objects", physics_world, _material_manager);


            // Load Scenes
            _world_loader.addWorldToScene(new string[]
            {
                "test_scene"
            }, _meshes, _light_manager);


            load_Flashlight();


            _animation_timer.start();
            _circadian_timer.start();
        }


        //------------------------------------------------------
        // Render Scene
        //------------------------------------------------------

        public void pauseAnimation()
        {
            _animation_timer.pause();
            _circadian_timer.pause();
        }

        public void resetAnimation()
        {
            _animation_timer.restart();
            _circadian_timer.restart();
        }


        public void render(BeginMode begin_mode, Program program)
        {
            renderMeshes(begin_mode, program);
            renderLightObjects(begin_mode, program);
        }

        public void renderMeshes(BeginMode begin_mode, Program program)
        {
            WorldDrawer.drawMeshes(begin_mode, _meshes, program, Matrix4.Identity, _animation_timer.seconds);
        }

        public void renderLightObjects(BeginMode begin_mode, Program program)
        {
            WorldDrawer.drawLights(begin_mode, lights, program, Matrix4.Identity, false);
        }

    }
}
