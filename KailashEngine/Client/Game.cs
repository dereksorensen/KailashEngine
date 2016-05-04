using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KailashEngine.UI;
using KailashEngine.World.Role;

namespace KailashEngine.Client
{
    class Game
    {

        private OpenGLVersion _gl_version;
        public OpenGLVersion gl_version
        {
            get { return _gl_version; }
            set { _gl_version = value; }
        }

        private Display _main_display;
        public Display main_display
        {
            get { return _main_display; }
            set { _main_display = value; }
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



        public Game(string title, OpenGLVersion gl_version, Display default_display, Player main_player)
        {
            _title = title;
            _gl_version = gl_version;
            _main_display = default_display;
            _main_player = main_player;
        }




    }
}
