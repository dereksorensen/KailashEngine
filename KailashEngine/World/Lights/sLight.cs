using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.World.Model;

namespace KailashEngine.World.Lights
{
    class sLight : Light
    {



        public sLight(string id,  Vector3 color, float intensity, float falloff, float spot_angle, bool shadow, Mesh light_mesh, Matrix4 transformation)
            : base(id, type_spot, color, intensity, falloff, shadow, light_mesh, transformation)
        {
            _spot_angle = spot_angle;


            // Create Light Object Mesh
            _unique_mesh = new UniqueMesh(id, light_mesh, transformation);

            // Create Light Bounds Mesh
            float spot_depth = falloff / 2.0f;
            float spot_radius = spot_depth * (float)Math.Tan(spot_angle) / 2.0f;
            Vector3 scaler = new Vector3(
                    spot_radius,
                    spot_radius,
                    spot_depth
                );
            Vector3 shifter = new Vector3(
                    0.0f,
                    0.0f,
                    -scaler.Z
                );

            // Build full transformation
            transformation = Matrix4.CreateScale(scaler) * Matrix4.CreateTranslation(shifter) * transformation.ClearScale();
            _bounding_unique_mesh = new UniqueMesh(id + "-bounds", light_mesh, transformation);

        }

    }
}
