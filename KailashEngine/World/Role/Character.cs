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
    class Character : WorldObject
    {


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

        private Vector3 _position_current;
        public Vector3 position_current
        {
            get { return _position_current; }
            set { _position_current = value; }
        }

        private Vector3 _position_previous;
        public Vector3 position_previous
        {
            get { return _position_previous; }
            set { _position_previous = value; }
        }



        public Character(string name, float movement_speed, float look_sensitivity)
            : base (new Vector3(), new Vector3(), new Vector3(0.0f, 1.0f, 0.0f))
        {
            _name = name;
            _movement_speed = movement_speed;
            _look_sensitivity = look_sensitivity;
        }


        //------------------------------------------------------
        // View Based Movement
        //------------------------------------------------------

        public void moveForeward()
        {
            moveForeward(_movement_speed);
        }

        public void moveBackward()
        {
            moveBackward(_movement_speed);
        }

        public void moveUp()
        {
            moveUp(_movement_speed);
        }

        public void moveDown()
        {
            moveDown(_movement_speed);
        }

        public void strafeRight()
        {
            strafeRight(_movement_speed);
        }

        public void strafeLeft()
        {
            strafeLeft(_movement_speed);
        }


    }
}
