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
        // Constants
        //------------------------------------------------------
        public enum size : int
        {
            f = sizeof(float),
            vec2 = sizeof(float) * 2,
            vec3 = sizeof(float) * 3,
            vec4 = sizeof(float) * 4,
            mat2 = sizeof(float) * 4,
            mat3 = sizeof(float) * 9,
            mat4 = sizeof(float) * 16,
        }

        //------------------------------------------------------
        // Interpolation functions
        //------------------------------------------------------

        public static float lerp(float src0, float src1, float t)
        {
            return src0 + (src1 - src0) * t;
        }
        public static Vector2 lerp(Vector2 src0, Vector2 src1, float t)
        {
            return src0 + (src1 - src0) * t;
        }
        public static Vector3 lerp(Vector3 src0, Vector3 src1, float t)
        {
            return src0 + (src1 - src0) * t;
        }

        public static float slerp(float src0, float src1, float t)
        {
            src0 = Math.Max(src0, 0.000001f);
            return (float)(Math.Pow(src1 / src0, t) * src0);
        }


        //------------------------------------------------------
        // World Object Helpers
        //------------------------------------------------------
        // Rotation Matrix to convert Z-up to Y-up
        public static Matrix4 yup = Matrix4.CreateRotationX((float)(-90.0f * Math.PI / 180.0f));

        public static Matrix4 blender2Kailash(Vector3 translation, Vector3 rotation_euler, Vector3 scale)
        {
            // Scale
            Matrix4 temp_scale = Matrix4.CreateScale(scale);

            // Rotation
            Quaternion x_rotation = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(rotation_euler.X));
            Quaternion y_rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(rotation_euler.Y));
            Quaternion z_rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(rotation_euler.Z));
            Quaternion zyx_rotation = Quaternion.Multiply(Quaternion.Multiply(z_rotation, y_rotation), x_rotation);

            zyx_rotation.Normalize();
            Matrix4 temp_rotation = Matrix4.CreateFromQuaternion(zyx_rotation);

            // Translation
            Matrix4 temp_translation = Matrix4.CreateTranslation(translation);


            // Build full tranformation matrix
            Matrix4 temp_matrix = temp_scale * (temp_rotation * temp_translation);

            // Blender defaults to Z-up. Need to convert to Y-up.
            return temp_matrix * yup;
        }



    }
}
