using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine
{
    static class EngineHelper
    {

        //------------------------------------------------------
        // Interpolation functions
        //------------------------------------------------------

        public static float lerp(float src0, float src1, float t)
        {
            return src0 + (src1 - src0) * (1.0f / t);
        }
        public static Vector2 lerp(Vector2 src0, Vector2 src1, float t)
        {
            return src0 + (src1 - src0) * (1.0f / t);
        }
        public static Vector3 lerp(Vector3 src0, Vector3 src1, float t)
        {
            return src0 + (src1 - src0) * (1.0f / t);
        }
    }
}
