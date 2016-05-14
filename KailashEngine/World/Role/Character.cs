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

        private float _movement_speed_walk;
        public float movement_speed_walk
        {
            get { return _movement_speed_walk; }
            set { _movement_speed_walk = value; }
        }

        private float _movement_speed_run;
        public float movement_speed_run
        {
            get { return _movement_speed_run; }
            set { _movement_speed_run = value; }
        }

        private bool _running;
        public bool running
        {
            get { return _running; }
            set { _running = value; }
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



        public Character(string name, float movement_speed_walk, float movement_speed_run, float look_sensitivity)
            : base (new Vector3(), new Vector3(), new Vector3(0.0f, 1.0f, 0.0f))
        {
            _name = name;
            _movement_speed_walk = movement_speed_walk;
            _movement_speed_run = movement_speed_run;
            _look_sensitivity = look_sensitivity;
        }


        //------------------------------------------------------
        // View Based Movement
        //------------------------------------------------------

        private float getMovementSpeed()
        {
            if(_running)
            {
                return _movement_speed_run;
            }
            else
            {
                return _movement_speed_walk;
            }
        }

        public void moveForeward()
        {
            moveForeward(getMovementSpeed());
        }

        public void moveBackward()
        {
            moveBackward(getMovementSpeed());
        }

        public void moveUp()
        {
            moveUp(getMovementSpeed());
        }

        public void moveDown()
        {
            moveDown(getMovementSpeed());
        }

        public void strafeRight()
        {
            strafeRight(getMovementSpeed());
        }

        public void strafeLeft()
        {
            strafeLeft(getMovementSpeed());
        }


    }
}
