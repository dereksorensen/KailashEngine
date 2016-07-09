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
        }

    }
}
