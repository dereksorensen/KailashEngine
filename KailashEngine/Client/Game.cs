using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.Serialization;
using KailashEngine.Output;
using KailashEngine.Input;

namespace KailashEngine.Client
{
    class Game
    {

        private Serializer _serializer;
        public Serializer serializer
        {
            get { return _serializer; }
        }

        private ClientConfig _config;
        public ClientConfig config
        {
            get { return _config; }
            set { _config = value; }
        }

        private string _title;
        public string title
        {
            get { return _title; }
            set { _title = value; }
        }

        private Display _display;
        public Display display
        {
            get { return _display; }
            set { _display = value; }
        }

        private Keyboard _keyboard;
        public Keyboard keyboard
        {
            get { return _keyboard; }
            set { _keyboard = value; }
        }

        private Mouse _mouse;
        public Mouse mouse
        {
            get { return _mouse; }
            set { _mouse = value; }
        }

        private Player _player;
        public Player player
        {
            get { return _player; }
            set { _player = value; }
        }

        private Scene _scene;
        public Scene scene
        {
            get { return _scene; }
            set { _scene = value; }
        }




        public Game(ClientConfig config)
        {
            _title = config.title;
            _config = config;
            _serializer = new Serializer(_config.path_resources_save_data);
            _display = new Display(_title, _config.default_resolution, _config.default_fullscreen);      
            _scene = new Scene(_config.path_resources_mesh, _config.path_resources_physics, _config.path_resources_lights);

            _player = new Player(
                new World.Role.PlayableCharacter(
                    "Janu Crips",
                    (World.SpatialData)_serializer.Load("player_spatial.dat") ?? new World.SpatialData(new Vector3(), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 1.0f, 0.0f)),
                    _config.default_movement_speed_walk, 
                    _config.default_movement_speed_run,
                    _config.default_look_sensitivity
                )
            );

            _keyboard = new Keyboard();
            _mouse = new Mouse(_player.character.look_sensitivity, true);
        }


        public void load()
        {
            _player.load(_config.fov_radian, _display.resolution.aspect, _config.near_far);
            _scene.load();
            _scene.toggleFlashlight(_player.character.enable_flashlight);
        }


        public void unload()
        {
            _serializer.Save(_player.character.spatial, "player_spatial.dat");
            _keyboard.turnOffCapLock();
        }

    }
}
