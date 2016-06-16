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



        public sLight(string id,  Vector3 position, Vector3 rotation, float size, Vector3 color, float intensity, float falloff, float spot_angle, bool shadow)
            : base(id, type_spot, position, rotation, size, color, intensity, falloff, shadow)
        {
            _spot_angle = spot_angle;
        }

    }
}
