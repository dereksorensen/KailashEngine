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



        public dLight(string id, Vector3 color, float intensity, bool shadow, Vector3 position, float[] cascade_splits)
            : base(id, type_directional, color, intensity, 0.0f, shadow, null, Matrix4.Identity)
        {
            _spatial.position = position;

            // Shadow Data
            _cascade_perspective_matrices = new Matrix4[]
            {
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, cascade_splits[0], cascade_splits[1]),
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, cascade_splits[1], cascade_splits[2]),
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, cascade_splits[2], cascade_splits[3]),
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, cascade_splits[3], cascade_splits[4]),
            };

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

            for (int cascade = 0; cascade < _num_cascades; cascade++)
            {

                //------------------------------------------------------
                // Create Frustum Bounds
                //------------------------------------------------------

                Vector3[] frustum_corners =
                {
                    new Vector3(-1.0f,1.0f,0.0f),
                    new Vector3(1.0f,1.0f,0.0f),
                    new Vector3(1.0f,-1.0f,0.0f),
                    new Vector3(-1.0f,-1.0f,0.0f),
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

                radius = (float)Math.Floor(radius * texture_width) / texture_width;

                float texel_size = texture_width / (radius * 1.0f);
                Vector3 light_position = Vector3.Zero;


                temp_lightDirection = Vector3.TransformVector(temp_lightDirection, Matrix4.CreateScale(texel_size));
                temp_lightDirection.X = (float)Math.Round(temp_lightDirection.X);
                temp_lightDirection.Y = (float)Math.Round(temp_lightDirection.Y);
                temp_lightDirection.Z = (float)Math.Round(temp_lightDirection.Z);
                temp_lightDirection = Vector3.TransformVector(temp_lightDirection, Matrix4.CreateScale(1.0f / texel_size));


                //================================================================================

                //Vector3 fix = camera_spatial.position;

                //texelSnap(
                //    ref fix,
                //    Matrix4.CreateScale(texel_size),
                //    Matrix4.LookAt(light_position, temp_lightDirection, new Vector3(0, 1, 0)));

                //fix = camera_spatial.position - fix;
                //fix = Vector3.Transform(fix, Matrix4.CreateScale(((float)tWidth / (radius * 2.0f))));


                //================================================================================


                //texelSnap(
                //    ref frustum_center,
                //    Matrix4.CreateScale(texel_size),
                //    Matrix4.LookAt(light_position, temp_lightDirection, new Vector3(0, 1, 0)));


                //================================================================================


                //frustum_center += fix;


                Vector3 eye = frustum_center + ((temp_lightDirection) * radius * 2.0f);

                Matrix4 temp_view_matrix = Matrix4.LookAt(eye, frustum_center, new Vector3(0, 1, 0));
                Matrix4 temp_ortho_matrix = Matrix4.CreateOrthographicOffCenter(-radius, radius, -radius, radius, -radius * 6.0f, radius * 6.0f);

                temp_view_matrices[cascade] = temp_view_matrix;
                temp_ortho_matrices[cascade] = temp_ortho_matrix;

            }

            _shadow_view_matrices = temp_view_matrices;
            _shadow_ortho_matrices = temp_ortho_matrices;

        }


    }
}
