using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.Serialization;
using KailashEngine.Output;
using KailashEngine.Input;
using KailashEngine.World.View;
using KailashEngine.World.Role;

namespace KailashEngine.Client
{
    class Game
    {
        private string _title;
        public string title
        {
            get { return _title; }
        }

        private ClientConfig _config;
        public ClientConfig config
        {
            get { return _config; }
        }

        private Serializer _serializer;
        public Serializer serializer
        {
            get { return _serializer; }
        }

        private Display _display;
        public Display display
        {
            get { return _display; }
        }

        private Scene _scene;
        public Scene scene
        {
            get { return _scene; }
        }

        private Keyboard _keyboard;
        public Keyboard keyboard
        {
            get { return _keyboard; }
        }

        private Mouse _mouse;
        public Mouse mouse
        {
            get { return _mouse; }
        }

        private Player _player;
        public Player player
        {
            get { return _player; }
        }


        private Camera _camera_1;
        private PlayableCharacter _character_1;


        public Game(ClientConfig config)
        {
            _title = config.title;
            _config = config;
            _serializer = new Serializer(_config.path_resources_save_data);
            _display = new Display(_title, _config.default_resolution, _config.default_fullscreen);      
            _scene = new Scene(_config.path_resources_scene);

            _player = new Player();


            _camera_1 = new Camera("camera_1", _config.fov, _display.resolution.aspect, _config.near_far);
            _character_1 = new PlayableCharacter(
                    "Janu Crips",
                    (World.SpatialData)_serializer.Load("player_spatial.dat") ?? new World.SpatialData(new Vector3(), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 1.0f, 0.0f)),
                    _config.default_movement_speed_walk,
                    _config.default_movement_speed_run
            );

            _keyboard = new Keyboard();
            _mouse = new Mouse(_config.default_look_sensitivity, true);
        }


        public void load()
        {
            _player.controlAndWatch(_character_1, _camera_1);
            _scene.load();
            _scene.toggleFlashlight(_player.enable_flashlight);
        }


        public void unload()
        {
            _serializer.Save(_player.character.spatial, "player_spatial.dat");
            _keyboard.turnOffCapLock();
        }

    }
}
