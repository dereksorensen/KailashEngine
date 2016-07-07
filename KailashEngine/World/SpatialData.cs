using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World
{
    [Serializable]
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
            set
            {
                _look = value;
                //_rotation_matrix = Matrix4.LookAt(Vector3.Zero, -_look, _up);               
                _strafe = Vector3.Cross(_look, _up);
            }
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

        private Vector3 _scale;
        public Vector3 scale
        {
            get { return _scale; }
        }



        private Vector3 _rotation_angles;
        public Vector3 rotation_angles
        {
            get { return _rotation_angles; }
            set { _rotation_angles = value; }
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
                //Console.WriteLine("=======================");
                
                _look = _rotation_matrix.Column2.Xyz;
                //Console.WriteLine(_look);
                //_look = -_rotation_matrix.Row2.Xyz;
                //_look = Vector3.Normalize(Vector3.Transform(new Vector3(0.0f, 0.0f, -1.0f), _rotation_matrix));
                //Console.WriteLine(_look);
                _up = _rotation_matrix.Column1.Xyz;
                _strafe = _rotation_matrix.Column0.Xyz;
            }
        }


        public Matrix4 scale_matrix
        {
            get { return Matrix4.CreateScale(_scale); }
        }



        public Matrix4 transformation
        {
            get
            {
                return (position_matrix * rotation_matrix);
            }
            set
            {
                _position = value.ExtractTranslation();
                _scale = value.ExtractScale();
                rotation_matrix = Matrix4.CreateFromQuaternion(value.ExtractRotation());
            }
        }


        private Matrix4 _perspective;
        public Matrix4 perspective
        {
            get { return _perspective; }
        }



        public SpatialData()
            : this(new Vector3(), new Vector3(), new Vector3())
        { }

        public SpatialData(Vector3 position, Vector3 look, Vector3 up)
        {
            _position = position;
            _look = look;
            _up = up;
            _scale = new Vector3(1.0f);

            rotation_matrix = Matrix4.LookAt(Vector3.Zero, _look, _up);
        }

        public SpatialData(Matrix4 transformation)
        {
            this.transformation = transformation;
        }


        public void setPerspective(float fov, float aspect, Vector2 near_far)
        {
            _perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), aspect, near_far.X, near_far.Y);
        }


    }
}
