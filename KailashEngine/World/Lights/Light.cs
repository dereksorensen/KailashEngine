using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World.Lights
{
    class Light : WorldObject
    {

        private string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }

        private bool _shadow;
        public bool shadow
        {
            get { return _shadow; }
            set { _shadow = value; }
        }

        private float _size;
        public float size
        {
            get { return _size; }
            set { _size = value; }
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



        public Light(string id, Vector3 position, Vector3 rotation, float size, Vector3 color, float intensity, bool shadow)
            : base(position, new Vector3(), new Vector3())
        {
            _id = id;      
            _size = size;
            _color = color;
            _intensity = intensity;
            _shadow = shadow;

            rotate(rotation.X, rotation.Y, rotation.Z);
        }


    }
}
