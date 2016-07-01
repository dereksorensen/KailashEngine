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


        public Display(Resolution resolution)
            : this("", resolution, false)
        { }

        public Display(Resolution resolution, bool fullscreen)
            : this("", resolution, fullscreen)
        { }

        public Display(string title, Resolution resolution)
            : this (title, resolution, false)
        { }

        public Display(string title, Resolution resolution, bool fullscreen)
        {
            _title = title;
            _resolution_windowed = resolution;
            _resolution_fullscreen = new Resolution(DisplayDevice.Default.Width, DisplayDevice.Default.Height);
            _resolution = fullscreen ? _resolution_fullscreen : _resolution_windowed;
            _fullscreen = fullscreen;
        }

    }
}
