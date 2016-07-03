using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World.Lights
{
    class pLight : Light
    {


        public pLight(string id, float size, Vector3 color, float intensity, float falloff, bool shadow, Matrix4 transformation)
            : base(id, type_point, size, color, intensity, falloff, shadow, transformation)
        {

        }

    }
}
