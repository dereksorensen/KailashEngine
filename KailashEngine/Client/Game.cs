using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KailashEngine.UI;
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

        private Player _main_player;
        public Player main_player
        {
            get { return _main_player; }
            set { _main_player = value; }
        }

        private string _title;
        public string title
        {
            get { return _title; }
            set { _title = value; }
        }



        public Game(ClientConfig config, Display default_display, Player main_player)
        {
            _title = config.title;
            _config = config;
            _main_display = default_display;
            _main_player = main_player;

            _keyboard = new Keyboard();
            _mouse = new Mouse(_main_player.character.look_sensitivity, true);
        }



        public void unload()
        {

            _keyboard.turnOffCapLock();
        }

    }
}
