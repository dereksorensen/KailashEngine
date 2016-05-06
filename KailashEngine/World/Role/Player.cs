using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.Input;
using KailashEngine.World.View;

namespace KailashEngine.World.Role
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

        private Camera _camera;
        public Camera camera
        {
            get { return _camera; }
            set { _camera = value; }
        }

        private string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        private Vector3 _position;
        public Vector3 position
        {
            get { return _position; }
            set { _position = value; }
        }




        public Player(string name)
        {
            _name = name;
            _keyboard = new Keyboard(true);
            _mouse = new Mouse();
        }


        public void input_Buffer()
        {
            _keyboard.buffer();
            _mouse.buffer();
        }

    }
}
