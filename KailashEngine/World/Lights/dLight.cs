using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.World.Model;

namespace KailashEngine.World.Lights
{
    class dLight : Light
    {
        private const int _num_cascades = 4;


        private Matrix4[] _shadow_view_matrices;
        public Matrix4[] shadow_view_matrices
        {
            get { return _shadow_view_matrices; }
        }

        private Matrix4[] _shadow_ortho_matrices;
        public Matrix4[] shadow_ortho_matrices
        {
            get { return _shadow_ortho_matrices; }
        }


        private Matrix4[] _cascade_perspective_matrices;

        private float[] _cascade_splits;


        public dLight(string id, bool shadow, Vector3 position, float[] cascade_splits)
            : base(id, type_directional, new Vector3(1.0f), 1.0f, 0.0f, shadow, null, Matrix4.Identity)
        {
            _spatial.position = position;

            // Shadow Data
            _cascade_splits = cascade_splits;

            float default_fov = MathHelper.DegreesToRadians(90.0f);
            float default_aspect = 1.0f;
            _cascade_perspective_matrices = new Matrix4[_num_cascades];
            for (int cascade = 0; cascade < _num_cascades; cascade++)
            {
                float near = _cascade_splits[cascade];
                float far = _cascade_splits[cascade + 1];

                _cascade_perspective_matrices[cascade] = Matrix4.CreatePerspectiveFieldOfView(default_fov, default_aspect, near, far);
            }

            _shadow_view_matrices = new Matrix4[4];
            _shadow_ortho_matrices = new Matrix4[4];
        }

        public void update_Cascades(SpatialData camera_spatial, Vector3 light_direction, float shadow_texture_width)
        {
            Matrix4[] temp_view_matrices = new Matrix4[_num_cascades];
            Matrix4[] temp_ortho_matrices = new Matrix4[_num_cascades];

            float cascade_backup_distance = 20.0f;

            for (int cascade = 0; cascade < _num_cascades; cascade++)
            {
                float near = _cascade_splits[cascade];
                float far = _cascade_splits[cascade + 1];

                //------------------------------------------------------
                // Create Frustum Bounds
                //------------------------------------------------------
                float frustum_near = -1.0f;
                float frustum_far = 1.0f;
                Vector3[] frustum_corners =
                {
                    // Near Plane
                    new Vector3(-1.0f,1.0f,frustum_near),
                    new Vector3(1.0f,1.0f,frustum_near),
                    new Vector3(1.0f,-1.0f,frustum_near),
                    new Vector3(-1.0f,-1.0f,frustum_near),
                    // Far Plane
                    new Vector3(-1.0f,1.0f,frustum_far),
                    new Vector3(1.0f,1.0f,frustum_far),
                    new Vector3(1.0f,-1.0f,frustum_far),
                    new Vector3(-1.0f,-1.0f,frustum_far),
                };

                frustum_corners = frustum_corners.Select(corner =>
                {
                    return Vector3.TransformPerspective(corner, Matrix4.Invert(camera_spatial.model_view * _cascade_perspective_matrices[cascade]));
                }).ToArray();


                //------------------------------------------------------
                // Get Frustum center and radius
                //------------------------------------------------------
                Vector3 frustum_center = Vector3.Zero;
                for (int i = 0; i < 8; i++)
                {
                    frustum_center = frustum_center + frustum_corners[i];
                }
                frustum_center = frustum_center / 8.0f;
                float radius = (frustum_corners[4] - frustum_corners[6]).Length;


                //------------------------------------------------------
                // Trying to fix shimmering
                //------------------------------------------------------
                radius = (float)Math.Floor(radius * shadow_texture_width) / shadow_texture_width;

                float scaler = shadow_texture_width / (radius * 50.0f);
                frustum_center *= scaler;
                frustum_center.X = (float)Math.Floor(frustum_center.X);
                frustum_center.Y = (float)Math.Floor(frustum_center.Y);
                frustum_center.Z = (float)Math.Floor(frustum_center.Z);
                frustum_center /= scaler;


                //------------------------------------------------------
                // Create Matrices
                //------------------------------------------------------

                Vector3 eye = frustum_center + (light_direction * (radius / 2.0f * cascade_backup_distance));
                temp_view_matrices[cascade] = Matrix4.LookAt(eye, frustum_center, Vector3.UnitY);
                temp_ortho_matrices[cascade] = Matrix4.CreateOrthographic(radius, radius, _cascade_splits[0], radius * cascade_backup_distance);
            }

            _shadow_view_matrices = temp_view_matrices;
            _shadow_ortho_matrices = temp_ortho_matrices;
        }


    }
}
