using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KailashEngine.Output;
using KailashEngine.Input;

namespace KailashEngine.Client
{
    class Game
    {

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

        private Display _main_display;
        public Display main_display
        {
            get { return _main_display; }
            set { _main_display = value; }
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




        public Game(ClientConfig config, Display default_display, Player main_player)
        {
            _title = config.title;
            _config = config;
            _main_display = default_display;
            _player = main_player;
            _scene = new Scene(_config.path_resources_mesh);

            _keyboard = new Keyboard();
            _mouse = new Mouse(_player.character.look_sensitivity, true);
        }


        public void load()
        {
            _scene.load();
        }


        public void unload()
        {

            _keyboard.turnOffCapLock();
        }

    }
}
