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

        private float _current_animation_time = 0.0f;
        public float current_animation_time
        {
            get
            {
                return _current_animation_time;
            }
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
            get { return _light_manager.lights_enabled; }
        }


        private Light _flashlight;
        public Light flashlight
        {
            get { return _flashlight; }
        }

        private dLight _sun;
        public dLight sun
        {
            get { return _sun; }
            set { _sun = value; }
        }



        //------------------------------------------------------
        // Constructor
        //------------------------------------------------------

        public Scene(string path_scene)
        {
            _path_scene = path_scene;

            _meshes = new List<UniqueMesh>();

            _animation_timer = new Timer();
            _circadian_timer = new CircadianTimer(1.0f, 2.0f);
        }


        //------------------------------------------------------
        // Flashlight
        //------------------------------------------------------

        public void toggleFlashlight(bool enabled)
        {
            _flashlight.enabled = enabled;
        }

        private void load_Flashlight()
        {
            // Load Flashlight
            _flashlight = new sLight(
                "flashlight",
                new Vector3(1.0f), 2.0f, 40.0f, MathHelper.DegreesToRadians(70.0f), 0.1f,
                true,
                _world_loader.sLight_mesh, Matrix4.Identity);

            _light_manager.addLight(_flashlight);
            toggleFlashlight(true);
        }

        private void load_SunLight(float near_plane)
        {
            _sun = new dLight(
                "sun",
                true,
                _circadian_timer.position,
                new float[]
                {
                    near_plane, 5.0f, 20.0f, 50.0f, 100.0f
                });

            _light_manager.addLight(_sun);
        }

        //------------------------------------------------------
        // Load Scene
        //------------------------------------------------------

        public void load(PhysicsWorld physics_world, float near_plane)
        {
            _material_manager = new MaterialManager();
            _light_manager = new LightManager();
            _world_loader = new WorldLoader(_path_scene, "light_objects", physics_world, _material_manager);


            // Load Scenes
            _world_loader.addWorldToScene(new string[]
            {
                //"sponza",
                "test_scene"
            }, _meshes, _light_manager);


            load_Flashlight();
            load_SunLight(near_plane);

            _animation_timer.start();
            _circadian_timer.start();
        }


        //------------------------------------------------------
        // Update Scene
        //------------------------------------------------------
        private void update_Sun(SpatialData camera_spatial)
        {
            _sun.spatial.position = _circadian_timer.position;
            _sun.update_Cascades(camera_spatial, Vector3.Normalize(_circadian_timer.position));
        }

        public void update(SpatialData camera_spatial)
        {
            update_Sun(camera_spatial);

            _light_manager.update(-camera_spatial.position);
            _current_animation_time = _animation_timer.seconds;
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
            WorldDrawer.drawMeshes(begin_mode, _meshes, program, Matrix4.Identity, _current_animation_time, 10);
        }

        public void renderMeshes_WithMaterials(BeginMode begin_mode, Program program)
        {
            WorldDrawer.drawMeshes(begin_mode, _meshes, program, Matrix4.Identity, _current_animation_time, 1);
        }

        public void renderMeshes_Basic(BeginMode begin_mode, Program program)
        {
            WorldDrawer.drawMeshes(begin_mode, _meshes, program, Matrix4.Identity, _current_animation_time, 0);
        }

        public void renderLightObjects(BeginMode begin_mode, Program program)
        {
            WorldDrawer.drawLights(begin_mode, lights, program, Matrix4.Identity, false);
        }

    }
}
