using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World
{
    class SpatialData
    {

        private Vector3 _position;
        public Vector3 position
        {
            get { return _position; }
            set { _position = value; }
        }

        private Vector3 _look;
        public Vector3 look
        {
            get { return _look; }
            set { _look = value; }
        }

        private Vector3 _up;
        public Vector3 up
        {
            get { return _up; }
            set { _up = value; }
        }

        private Vector3 _strafe;
        public Vector3 strafe
        {
            get { return _strafe; }
            set { _strafe = value; }
        }

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
                _up = _rotation_matrix.Column1.Xyz;
                _strafe = Vector3.Cross(_look, _up);
            }
        }

        //private Matrix4 _matrix;
        public Matrix4 view
        {
            get
            {
                return Matrix4.Mult(position_matrix, _rotation_matrix);
            }
        }

        private Matrix4 _perspective;
        public Matrix4 perspective
        {
            get { return _perspective; }
            set { _perspective = value; }
        }


        public SpatialData(Vector3 position, Vector3 look, Vector3 up)
        {
            _position = position;
            _look = look;
            _up = up;
        }

    }
}
