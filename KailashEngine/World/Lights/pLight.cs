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

        public struct ViewMatrices
        {
            public Matrix4[] matrices;
        }

        private ViewMatrices _shadow_view_matrices;
        public ViewMatrices shadow_view_matrices
        {
            get
            {

                _shadow_view_matrices.matrices = new Matrix4[]
                {
                    EngineHelper.createMatrix(_spatial.position, new Vector3(180.0f, -90.0f, 0.0f), Vector3.One),
                    EngineHelper.createMatrix(_spatial.position, new Vector3(180.0f, 90.0f, 0.0f), Vector3.One),
                    EngineHelper.createMatrix(_spatial.position, new Vector3(-90.0f, 0.0f, 0.0f), Vector3.One),
                    EngineHelper.createMatrix(_spatial.position, new Vector3(90.0f, 0.0f, 0.0f), Vector3.One),
                    EngineHelper.createMatrix(_spatial.position, new Vector3(180.0f, 0.0f, 0.0f), Vector3.One),
                    EngineHelper.createMatrix(_spatial.position, new Vector3(180.0f, 180.0f, 0.0f), Vector3.One)
                };
                return _shadow_view_matrices;
            }
        }


        public Matrix4 shadow_perspective_matrix
        {
            get
            {
                return _spatial.perspective;
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
            transformation = Matrix4.CreateScale(scaler) * transformation.ClearScale();
            _bounding_unique_mesh = new UniqueMesh(id + "-bounds", light_mesh, transformation);


            _spatial.setPerspective(90.0f, 1.0f, 0.1f, 100.0f);
            _shadow_view_matrices.matrices = new Matrix4[6];
        }

    }
}
