using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.Output
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


        protected Resolution _resolution_windowed;
        public Resolution resolution_windowed
        {
            get { return _resolution_windowed; }
            set { _resolution_windowed = value; }
        }


        protected Resolution _resolution_fullscreen;
        public Resolution resolution_fullscreen
        {
            get { return _resolution_fullscreen; }
            set { _resolution_fullscreen = value; }
        }


        private Point _screen_center;
        public Point screen_center
        {
            get { return _screen_center; }
            set { _screen_center = value; }
        }



        protected bool _fullscreen;
        public bool fullscreen
        {
            get { return _fullscreen; }
            set { _fullscreen = value; }
        }


        public Display(int width, int height)
            : this("", width, height, false)
        { }

        public Display(int width, int height, bool fullscreen)
            : this("", width, height, fullscreen)
        { }

        public Display(string title, int width, int height)
            : this (title, width, height, false)
        { }

        public Display(string title, int width, int height, bool fullscreen)
        {
            _title = title;
            _resolution_windowed = new Resolution(width, height);
            _resolution_fullscreen = new Resolution(DisplayDevice.Default.Width, DisplayDevice.Default.Height);
            _resolution = fullscreen ? _resolution_fullscreen : _resolution_windowed;
            _fullscreen = fullscreen;
        }

    }
}
