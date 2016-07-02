using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.World.Role;

namespace KailashEngine.World.View
{
    class Camera : WorldObject
    {
        private float _default_fov;
        private float _fov_current;
        private float _fov_previous;

        private float _default_aspect_ratio;
        private Vector2 _default_near_far;

        private Vector3 _position_current;
        private Vector3 _position_previous;



        public Camera(float fov, float aspect_ratio, Vector2 near_far)
            : this(fov, aspect_ratio, near_far, new SpatialData())
        { }

        public Camera(float fov, float aspect_ratio, Vector2 near_far, SpatialData spatial_data)
            : base (spatial_data)
        {
            _default_fov = _fov_current = _fov_previous = fov;
            _default_aspect_ratio = aspect_ratio;
            _default_near_far = near_far;

            _position_current = _spatial.position;

            //updatePerspective(_fov_current, _default_aspect_ratio, _default_near_far);
        }


        public void updatePerspective(float fov, float aspect, Vector2 near_far)
        {
            _spatial.perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), aspect, near_far.X, near_far.Y);
        }


        public void followCharacter(Character character)
        {
            try
            {
                _spatial = character.spatial;
                _position_current = _spatial.position;
                updatePerspective(_fov_current, _default_aspect_ratio, _default_near_far);
            }
            catch(Exception e)
            {
                Debug.DebugHelper.logError("Error Following Character (" + character.name + ")", e.Message);
            }       
        }




        public void smoothMovement(float smooth_movement_delay)
        {
            _position_current = _spatial.position - _position_current;
            _position_current = EngineHelper.lerp(_position_previous, _position_current, smooth_movement_delay);
            _position_previous = _position_current;
            _spatial.position += _position_current;
            _position_current = _spatial.position;
        }


        public void zoom(bool zoom_in, float current_fps)
        {
            float zoom_speed = 500.0f;
            float max_zoom = 0.35f * _default_fov;
            float zoom_delay = 0.6f;

            if (zoom_in)
            {
                _fov_current -= zoom_speed / current_fps;
                _fov_current = Math.Max(_fov_current, max_zoom);
            }
            else
            {
                _fov_current += zoom_speed / current_fps;
                _fov_current = Math.Min(_fov_current, _default_fov);
            }

            _fov_current = EngineHelper.slerp(_fov_previous, _fov_current, zoom_delay);
            _fov_previous = _fov_current;

            updatePerspective(_fov_current, _default_aspect_ratio, _default_near_far);
        }




    }
}
