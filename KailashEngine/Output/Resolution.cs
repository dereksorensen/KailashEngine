using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.Output
{
    class Resolution
    {

        private int _width;
        public int W
        {
            get { return _width; }
            set { _width = value; }
        }

        private int _height;
        public int H
        {
            get { return _height; }
            set { _height = value; }
        }

        private Vector2 _dimensions;
        public Vector2 dimensions
        {
            get { return _dimensions; }
            set { _dimensions = value; }
        }

        public float aspect
        {
            get
            {
                return (float)_width / (float)_height;
            }
        }

        public Resolution(int width, int height)
        {
            _width = width;
            _height = height;
            _dimensions = new Vector2(width, height);
        }

    }
}
