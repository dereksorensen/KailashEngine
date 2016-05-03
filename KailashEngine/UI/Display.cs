using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.UI
{
    class Display
    {

        public struct Resolution
        {
            public int W;
            public int H;
            public Vector2 vSize;
            public float aspect
            {
                get
                {
                    return (float)W / (float)H;
                }
            }

            public Resolution(int width, int height)
            {
                W = width;
                H = height;
                vSize = new Vector2(width, height);
            }
        }

        protected string _title;
        public string title
        {
            get { return _title; }
            set { _title = value; }
        }


        protected Resolution _resolution;
        public Resolution resolution
        {
            get { return _resolution; }
            set { _resolution = value; }
        }


        protected bool _fullscreen;
        public bool fullscreen
        {
            get { return _fullscreen; }
            set { _fullscreen = value; }
        }


        public Display(string title, int width, int height)
            : this (title, width, height, false)
        { }

        public Display(string title, int width, int height, bool fullscreen)
        {
            _title = title;
            _resolution = new Resolution(width, height);
            _fullscreen = fullscreen;
        }

    }
}
