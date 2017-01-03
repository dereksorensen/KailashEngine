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
            float default_fov = MathHelper.DegreesToRadians(90.0f);
            float default_aspect = 1.0f;
            _cascade_splits = cascade_splits;

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


        private void texelSnap(ref Vector3 vec, Matrix4 scaler, Matrix4 lookat)
        {
            lookat = Matrix4.Mult(scaler, lookat);
            Matrix4 lookatInv = Matrix4.Invert(lookat);

            Vector3 fix = vec;
            fix = Vector3.TransformVector(fix, lookat);
            fix.X = (float)Math.Floor(fix.X);
            fix.Y = (float)Math.Floor(fix.Y);
            fix.Z = (float)Math.Floor(fix.Z);
            fix = Vector3.TransformVector(fix, lookatInv);

            vec = fix;
        }

        public void update_Cascades(float texture_width, Vector3 light_direction, SpatialData camera_spatial)
        {
            Matrix4[] temp_view_matrices = new Matrix4[_num_cascades];
            Matrix4[] temp_ortho_matrices = new Matrix4[_num_cascades];

            Vector3 temp_lightDirection = light_direction;
            float cascade_length = 10.0f;

            Console.WriteLine("========");
            for (int cascade = 0; cascade < _num_cascades; cascade++)
            {


                //------------------------------------------------------
                // Create Frustum Bounds
                //------------------------------------------------------

                Vector3[] frustum_corners =
                {
                    // Near Plane
                    new Vector3(-1.0f,1.0f,0.0f),
                    new Vector3(1.0f,1.0f,0.0f),
                    new Vector3(1.0f,-1.0f,0.0f),
                    new Vector3(-1.0f,-1.0f,0.0f),
                    // Far Plane
                    new Vector3(-1.0f,1.0f,1.0f),
                    new Vector3(1.0f,1.0f,1.0f),
                    new Vector3(1.0f,-1.0f,1.0f),
                    new Vector3(-1.0f,-1.0f,1.0f)
                };

                Matrix4 camera_vp = camera_spatial.model_view * _cascade_perspective_matrices[cascade];
                Matrix4 inv_camera_vp = Matrix4.Invert(camera_vp);

                for (int i = 0; i < 8; i++)
                {
                    frustum_corners[i] = Vector3.TransformPerspective(frustum_corners[i], inv_camera_vp);
                }
                
                Vector3 frustum_center = Vector3.Zero;
                for (int i = 0; i < 8; i++)
                {
                    frustum_center = frustum_center + frustum_corners[i];
                }
                frustum_center = frustum_center * (1.0f / 8.0f);


                float radius = (frustum_corners[0] - frustum_corners[6]).Length / 2.0f;

                //Console.WriteLine((float)Math.IEEERemainder(radius, 1.0f / texture_width));
                //radius -= (float)Math.IEEERemainder(radius, 1.0f / texture_width);
                //radius = (float)Math.Floor(radius * texture_width) / texture_width;

                float texel_size = texture_width / (radius * 100.0f);
                Vector3 light_position = camera_spatial.position;


                //temp_lightDirection = Vector3.TransformVector(temp_lightDirection, Matrix4.CreateScale(texel_size));
                //temp_lightDirection.X = (float)Math.Floor(temp_lightDirection.X);
                //temp_lightDirection.Y = (float)Math.Floor(temp_lightDirection.Y);
                //temp_lightDirection.Z = (float)Math.Floor(temp_lightDirection.Z);
                //temp_lightDirection = Vector3.TransformVector(temp_lightDirection, Matrix4.CreateScale(1.0f / texel_size));


                //float scaler = ((cascade_length * 10.0f) / ((float)Math.Floor(radius)));
                //frustum_center *= scaler;
                //frustum_center.X = (float)Math.Floor(frustum_center.X);
                //frustum_center.Y = (float)Math.Floor(frustum_center.Y);
                //frustum_center.Z = (float)Math.Floor(frustum_center.Z);
                //frustum_center /= scaler;



                //Vector3 eye = frustum_center + ((temp_lightDirection) * radius * (cascade_length / 2.0f));
                //Matrix4 temp_view_matrix = Matrix4.LookAt(eye, frustum_center, Vector3.UnitY);
                ////Matrix4 temp_ortho_matrix = Matrix4.CreateOrthographicOffCenter(-radius, radius, -radius, radius, -radius * 6.0f, radius * 6.0f);
                //Matrix4 temp_ortho_matrix = Matrix4.CreateOrthographic(radius, radius, -radius * cascade_length, radius * cascade_length);

                Vector3 eye = frustum_center + temp_lightDirection;
                Matrix4 temp_view_matrix = Matrix4.LookAt(eye, frustum_center, Vector3.UnitY);
                //Matrix4 temp_ortho_matrix = Matrix4.CreateOrthographicOffCenter(-radius, radius, -radius, radius, -radius * 6.0f, radius * 6.0f);
                Matrix4 temp_ortho_matrix = Matrix4.CreateOrthographic(radius, radius, -radius * cascade_length, radius * cascade_length);

                temp_view_matrices[cascade] = temp_view_matrix;
                temp_ortho_matrices[cascade] = temp_ortho_matrix;

            }

            _shadow_view_matrices = temp_view_matrices;
            _shadow_ortho_matrices = temp_ortho_matrices;

        }


        public void update_Cascades2(SpatialData camera_spatial, Vector3 light_direction)
        {
            Matrix4[] temp_view_matrices = new Matrix4[_num_cascades];
            Matrix4[] temp_ortho_matrices = new Matrix4[_num_cascades];

            float cascade_length = 6.0f;

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
                frustum_center = frustum_center * (1.0f / 8.0f);
                float radius = (frustum_corners[0] - frustum_corners[6]).Length / 2.0f;

                //------------------------------------------------------
                // Create Matrices
                //------------------------------------------------------

                Vector3 eye = frustum_center + (light_direction * ((radius / 2.0f) * cascade_length));
                temp_view_matrices[cascade] = Matrix4.LookAt(eye, frustum_center, Vector3.UnitY);
                temp_ortho_matrices[cascade] = Matrix4.CreateOrthographic((radius * cascade_length), (radius * cascade_length), near, (radius * cascade_length));
            }

            _shadow_view_matrices = temp_view_matrices;
            _shadow_ortho_matrices = temp_ortho_matrices;
        }


    }
}
