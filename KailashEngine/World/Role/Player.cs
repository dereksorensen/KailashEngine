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
    class Player : WorldObject
    {

        //------------------------------------------------------
        // Player Objects
        //------------------------------------------------------

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

        //------------------------------------------------------
        // Player Properties
        //------------------------------------------------------

        private string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        private float _movement_speed;
        public float movement_speed
        {
            get { return _movement_speed; }
            set { _movement_speed = value; }
        }

        private float _look_sensitivity;
        public float look_sensitivity
        {
            get { return _look_sensitivity; }
            set { _look_sensitivity = value; }
        }



        public Player(string name)
            : base (new Vector3(), new Vector3(), new Vector3())
        {
            _name = name;
            _keyboard = new Keyboard(false);
            _mouse = new Mouse();
        }


        public void input_Buffer()
        {
            _keyboard.buffer();
            _mouse.buffer();
        }

    }
}
