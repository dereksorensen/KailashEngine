using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;


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

        private bool _sprinting;
        public bool sprinting
        {
            get { return _sprinting; }
            set { _sprinting = value; }
        }

        private bool _enable_flashlight;
        public bool enable_flashlight
        {
            get { return _enable_flashlight; }
            set { _enable_flashlight = value; }
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



        public Character(string name, SpatialData spatial_data, float movement_speed_walk, float movement_speed_run, float look_sensitivity)
            : base (spatial_data)
        {
            _name = name;
            _movement_speed_walk = movement_speed_walk;
            _movement_speed_run = movement_speed_run;
            _look_sensitivity = look_sensitivity;

            _position_current = _spatial.position;
        }


        //------------------------------------------------------
        // Rotation Override
        //------------------------------------------------------

        public override void rotate(float x_angle, float y_angle, float z_angle)
        {
            _spatial.rotation_angles = new Vector3(x_angle, y_angle, z_angle);

            Quaternion x_rotation = Quaternion.FromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), MathHelper.DegreesToRadians(x_angle));
            x_rotation.Normalize();

            Quaternion y_rotation = Quaternion.FromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), MathHelper.DegreesToRadians(y_angle));
            y_rotation.Normalize();

            Quaternion z_rotation = Quaternion.FromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), MathHelper.DegreesToRadians(z_angle));
            z_rotation.Normalize();

            Quaternion xy_rotation = Quaternion.Multiply(x_rotation, y_rotation);
            Quaternion xyz_rotation = Quaternion.Multiply(Quaternion.Multiply(x_rotation, z_rotation), y_rotation);

            xyz_rotation.Normalize();
            _spatial.rotation_matrix = Matrix4.CreateFromQuaternion(xyz_rotation);

            // Added bit to set look and up based on xy rotation only
            xy_rotation.Normalize();
            Matrix4 temp_xy_rotation = Matrix4.CreateFromQuaternion(xy_rotation);
            _spatial.look = _spatial.rotation_matrix.Column2.Xyz;
            _spatial.up = temp_xy_rotation.Column1.Xyz;
            _spatial.strafe = Vector3.Cross(_spatial.look, _spatial.up);

        }

        //------------------------------------------------------
        // View Based Movement
        //------------------------------------------------------


        private float getMovementSpeed()
        {
            if(_sprinting)
            {
                return _movement_speed_run * 10.0f;
            }
            else if(_running)
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
