using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.World.Model;

namespace KailashEngine.World.Lights
{
    class pLight : Light
    {
        private static Matrix4[] _shadow_rotation_matrices = new Matrix4[]
            {
                EngineHelper.createRotationMatrix(180.0f, 90.0f, 0.0f),
                EngineHelper.createRotationMatrix(180.0f, -90.0f, 0.0f),
                EngineHelper.createRotationMatrix(-90.0f, 0.0f, 0.0f),
                EngineHelper.createRotationMatrix(90.0f, 0.0f, 0.0f),
                EngineHelper.createRotationMatrix(180.0f, 0.0f, 0.0f),
                EngineHelper.createRotationMatrix(180.0f, -180.0f, 0.0f)
            };
        public Matrix4[] shadow_view_matrices
        {
            get
            {
                return new Matrix4[]
                {
                    Matrix4.CreateTranslation(-_spatial.position) * _shadow_rotation_matrices[0],
                    Matrix4.CreateTranslation(-_spatial.position) * _shadow_rotation_matrices[1],
                    Matrix4.CreateTranslation(-_spatial.position) * _shadow_rotation_matrices[2],
                    Matrix4.CreateTranslation(-_spatial.position) * _shadow_rotation_matrices[3],
                    Matrix4.CreateTranslation(-_spatial.position) * _shadow_rotation_matrices[4],
                    Matrix4.CreateTranslation(-_spatial.position) * _shadow_rotation_matrices[5]
                };
            }
        }

        public Matrix4 shadow_perspective_matrix
        {
            get
            {
                return _spatial.perspective;
            }
        }

        public Matrix4[] viewray_matrices
        {
            get
            {
                return new Matrix4[]
                {
                    Matrix4.Invert(_shadow_rotation_matrices[0] * shadow_perspective_matrix),
                    Matrix4.Invert(_shadow_rotation_matrices[1] * shadow_perspective_matrix),
                    Matrix4.Invert(_shadow_rotation_matrices[2] * shadow_perspective_matrix),
                    Matrix4.Invert(_shadow_rotation_matrices[3] * shadow_perspective_matrix),
                    Matrix4.Invert(_shadow_rotation_matrices[4] * shadow_perspective_matrix),
                    Matrix4.Invert(_shadow_rotation_matrices[5] * shadow_perspective_matrix),
                };
            }
        }



        public pLight(string id, Vector3 color, float intensity, float falloff, bool shadow, Mesh light_mesh, Matrix4 transformation)
            : base(id, type_point, color, intensity, falloff, shadow, light_mesh, transformation)
        {

            // Create Light Object Mesh
            _unique_mesh = new UniqueMesh(id, light_mesh, transformation);

            // Create Light Bounds Mesh
            float point_radius = falloff;
            Vector3 scaler = new Vector3(
                    point_radius,
                    point_radius,
                    point_radius
                );

            // Build full transformation
            _bounds_matrix = Matrix4.CreateScale(scaler);
            transformation = _bounds_matrix * transformation.ClearScale();
            _bounding_unique_mesh = new UniqueMesh(id + "-bounds", light_mesh, transformation);

            // Shadow Matrices
            _spatial.setPerspective(90.0f, 1.0f, 0.1f, 100.0f);
        }
    }
}
