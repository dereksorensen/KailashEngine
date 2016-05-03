using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KailashEngine.Control;

namespace KailashEngine.Role
{
    class Player
    {

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

        private string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }



        public Player(string name)
        {
            _name = name;
            _keyboard = new Keyboard(true);
            _mouse = new Mouse();
        }



    }
}
