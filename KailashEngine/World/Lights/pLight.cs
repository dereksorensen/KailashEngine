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


        public pLight(string id, Vector3 position, float size, Vector3 color, float intensity, float falloff, bool shadow)
            : base(id, position, new Vector3(), size, color, intensity, falloff, shadow)
        {

        }

    }
}
