using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using OpenTK;

using KailashEngine.Output;

namespace KailashEngine.Client
{
    class ClientConfig
    {

        private string _title;
        public string title
        {
            get { return _title; }
            set { _title = value; }
        }

        //------------------------------------------------------
        // OpenGL
        //------------------------------------------------------

        private int _gl_major_version;
        public int gl_major_version
        {
            get { return _gl_major_version; }
            set { _gl_major_version = value; }
        }

        private int _gl_minor_version;
        public int gl_minor_version 
        {
            get { return _gl_minor_version; }
            set { _gl_minor_version = value; }
        }

        private int _glsl_version;
        public int glsl_version
        {
            get
            {
                return int.Parse(_gl_major_version + "" + _gl_minor_version + "0");
            }
            set { _glsl_version = value; }
        }

        public string gl_version_string
        {
            get
            {
                return _gl_major_version + "." + _gl_minor_version;
            }
        }

        //------------------------------------------------------
        // Display
        //------------------------------------------------------

        protected Resolution _default_resolution;
        public Resolution default_resolution
        {
            get { return _default_resolution; }
        }

        protected bool _default_fullscreen;
        public bool default_fullscreen
        {
            get { return _default_fullscreen; }
        }


        //------------------------------------------------------
        // Rendering
        //------------------------------------------------------

        private float _fps_target;
        public float fps_target
        {
            get { return _fps_target; }
            set { _fps_target = value; }
        }

        private float _fov;
        public float fov
        {
            get { return _fov; }
            set { _fov = value; }
        }
        public float fov_radian
        {
            get
            {
                return MathHelper.DegreesToRadians(_fov);
            }
            set
            {
                _fov = MathHelper.RadiansToDegrees(value);
            }
        }

        private Vector2 _near_far;
        public Vector2 near_far
        {
            get { return _near_far; }
            set { _near_far = value; }
        }

        public Vector2 near_far_projection
        {
            get
            {
                return new Vector2(
                        _near_far.Y / (_near_far.Y - _near_far.X),
                        (_near_far.Y * _near_far.X) / (_near_far.Y - _near_far.X)
                    );
            }
        }

        public Vector4 near_far_full
        {
            get
            {
                return new Vector4(
                        _near_far.X,
                        _near_far.Y,
                        near_far_projection.X,
                        near_far_projection.Y
                    );
            }
        }


        //------------------------------------------------------
        // Gameplay
        //------------------------------------------------------

        private float _smooth_mouse_delay;
        public float smooth_mouse_delay
        {
            get { return _smooth_mouse_delay; }
            set { _smooth_mouse_delay = value; }
        }

        private float _smooth_keyboard_delay;
        public float smooth_keyboard_delay
        {
            get { return _smooth_keyboard_delay; }
            set { _smooth_keyboard_delay = value; }
        }


        //------------------------------------------------------
        // Player
        //------------------------------------------------------

        private float _default_movement_speed_walk;
        public float default_movement_speed_walk
        {
            get { return _default_movement_speed_walk; }
            set { _default_movement_speed_walk = value; }
        }

        private float _default_movement_speed_run;
        public float default_movement_speed_run
        {
            get { return _default_movement_speed_run; }
            set { _default_movement_speed_run = value; }
        }

        private float _default_look_sensitivity;
        public float default_look_sensitivity
        {
            get { return _default_look_sensitivity; }
            set { _default_look_sensitivity = value; }
        }



        public ClientConfig(
            string title,
            int gl_major_version, int gl_minor_version, 
            float target_fps, 
            float fov, float near_plane, float far_plane,
            float smooth_mouse_delay, float smooth_keyboard_delay,
            int width, int height, bool fullscreen,
            float movement_speed_walk, float movement_speed_run, float look_sensitivity)
        {
            _title = title;

            _gl_major_version = gl_major_version;
            _gl_minor_version = gl_minor_version;

            _fps_target = target_fps;

            _fov = fov;
            _near_far = new Vector2(near_plane, far_plane);

            _smooth_mouse_delay = smooth_mouse_delay;
            _smooth_keyboard_delay = smooth_keyboard_delay;

            _default_resolution = new Resolution(width, height);
            _default_fullscreen = fullscreen;


            _default_movement_speed_walk = movement_speed_walk;
            _default_movement_speed_run = movement_speed_run;
            _default_look_sensitivity = look_sensitivity;

        }

    }
}
