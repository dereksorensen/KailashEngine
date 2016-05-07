using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World
{
    class WorldObject
    {

        protected Vector3 _position;
        public Vector3 position
        {
            get { return _position; }
            set { _position = value; }
        }

        protected Vector3 _look;
        public Vector3 look
        {
            get { return _look; }
            set { _look = value; }
        }

        protected Vector3 _up;
        public Vector3 up
        {
            get { return _up; }
            set { _up = value; }
        }

        protected Vector3 _strafe;
        public Vector3 strafe
        {
            get { return _strafe; }
            set { _strafe = value; }
        }


        private Matrix4 _position_matrix;
        public Matrix4 position_matrix
        {
            get { return Matrix4.CreateTranslation(_position); }
        }

        private Matrix4 _rotation_matrix;
        public Matrix4 rotation_matrix
        {
            get { return _rotation_matrix; }
            set
            {
                _rotation_matrix = value;
                _look = _rotation_matrix.Column2.Xyz;
                _strafe = Vector3.Cross(_look, _up);
            }
        }

        protected Matrix4 _matrix;
        public Matrix4 matrix
        {
            get
            {
                return Matrix4.Mult( _position_matrix, _rotation_matrix);
            }
        }



        public WorldObject()
            : this (new Vector3(), new Vector3(), new Vector3())
        { }

        public WorldObject(Vector3 position, Vector3 look, Vector3 up)
        {
            _position = position;
            _look = look;
            _up = up;
        }


        //------------------------------------------------------
        // Rotation
        //------------------------------------------------------

        public void rotate(float x_angle, float y_angle, float z_angle)
        {

            Quaternion x_rotation = Quaternion.FromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), MathHelper.DegreesToRadians(x_angle));
            x_rotation.Normalize();

            Quaternion y_rotation = Quaternion.FromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), MathHelper.DegreesToRadians(y_angle));
            y_rotation.Normalize();

            Quaternion z_rotation = Quaternion.FromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), MathHelper.DegreesToRadians(z_angle));
            z_rotation.Normalize();

            //Quaternion xy_rotation = Quaternion.Multiply(x_rotation, y_rotation);
            Quaternion xyz_rotation = Quaternion.Multiply(Quaternion.Multiply(x_rotation, y_rotation), z_rotation);

            xyz_rotation.Normalize();
            _rotation_matrix = Matrix4.CreateFromQuaternion(xyz_rotation);

            _look = _matrix.Column2.Xyz;
            _strafe = Vector3.Cross(_look, _up);
            //_up = _matrix.Column1.Xyz;
        }

        //------------------------------------------------------
        // View Based Movement
        //------------------------------------------------------

        public void moveForeward(float speed)
        {
            _position += _look * speed;
        }

        public void moveBackward(float speed)
        {
            _position -= _look * speed;
        }

        public void moveUp(float speed)
        {
            _position += _up * speed;
        }

        public void moveDown(float speed)
        {
            _position -= _up * speed;
        }

        public void strafeRight(float speed)
        {
            _position += _strafe * speed;
        }

        public void strafeLeft(float speed)
        {
            _position -= _strafe * speed;
        }

    }
}
