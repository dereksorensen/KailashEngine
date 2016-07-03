using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World.Lights
{
    class sLight : Light
    {



        public sLight(string id, float size, Vector3 color, float intensity, float falloff, float spot_angle, bool shadow, Matrix4 transformation)
            : base(id, type_spot, size, color, intensity, falloff, shadow, transformation)
        {
            _spot_angle = spot_angle;
        }

    }
}
