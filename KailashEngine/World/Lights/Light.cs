using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.World.Model;
using KailashEngine.Animation;

namespace KailashEngine.World.Lights
{
    class Light : WorldObject
    {

        public const string type_spot = "SPOT";
        public const string type_point = "POINT";
        public const string type_directional = "DIRECTIONAL";


        private int _lid;
        public int lid
        {
            get { return _lid; }
            set { _lid = value; }
        }


        private int _sid;
        public int sid
        {
            get { return _sid; }
            set { _sid = value; }
        }


        private bool _enabled;
        public bool enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }


        private bool _shadowed;
        public bool shadowed
        {
            get { return _shadowed; }
            set { _shadowed = value; }
        }

        private ObjectAnimator _animator;
        public ObjectAnimator animator
        {
            get { return _animator; }
            set
            {
                animated = true;
                _animator = value;
            }
        }

        private bool _animated;
        public bool animated
        {
            get { return _animated; }
            set { _animated = value; }
        }


        private string _type;
        public string type
        {
            get { return _type; }
            set { _type = value; }
        }

        private Vector3 _color;
        public Vector3 color
        {
            get { return _color; }
            set { _color = value; }
        }

        private float _intensity;
        public float intensity
        {
            get { return _intensity; }
            set { _intensity = value; }
        }

        private float _object_emission;
        public float object_emission
        {
            get { return _object_emission; }
            set { _object_emission = value; }
        }


        private float _falloff;
        public float falloff
        {
            get { return _falloff; }
            set { _falloff = value; }
        }

        protected float _spot_angle;
        public float spot_angle
        {
            get { return _spot_angle; }
            set { _spot_angle = value; }
        }

        protected float _spot_blur;
        public float spot_blur
        {
            get { return _spot_blur; }
            set { _spot_blur = value; }
        }

        protected UniqueMesh _unique_mesh;
        public UniqueMesh unique_mesh
        {
            get { return _unique_mesh; }
        }

        protected UniqueMesh _bounding_unique_mesh;
        public UniqueMesh bounding_unique_mesh
        {
            get { return _bounding_unique_mesh; }
        }

        protected Matrix4 _bounds_matrix;
        public Matrix4 bounds_matrix
        {
            get { return _bounds_matrix; }
        }



        public Light(string id, string type, Vector3 color, float intensity, float falloff, bool shadow, Mesh light_mesh, Matrix4 transformation)
            : base(id, new SpatialData(transformation))
        {
            _lid = _sid = -1;
            _enabled = false;
            _animated = false;
            _type = type;
            _color = color;
            _intensity = intensity;
            _falloff = falloff;
            _shadowed = shadow;
            _bounds_matrix = Matrix4.Identity;

            _object_emission = _intensity;
        }

    }
}
