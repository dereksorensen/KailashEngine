﻿using System;
using System.IO;
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
        // Project Helpers
        //------------------------------------------------------
        public static string getProjectName()
        {
            return System.Reflection.Assembly.GetCallingAssembly().FullName.Split(',')[0];
        }

        public static string getProjectBasePath()
        {
            bool path_found = false;
            string cur_search = "../";
            string base_path = "";

            while (!path_found)
            {
                base_path = Path.GetFullPath(cur_search);
                string[] dirs = base_path.Split('\\');
                path_found = dirs[dirs.Length - 2] == getProjectName();
                cur_search += "../";
            }

            return base_path;
        }


        //------------------------------------------------------
        // Data functions
        //------------------------------------------------------

        public static float lerp(float src0, float src1, float t)
        {
            return src0 + (src1 - src0) * t;
        }

        public static Matrix4 lerp(Matrix4 src0, Matrix4 src1, float t)
        {
            Vector3 temp_translation = Vector3.Lerp(src0.ExtractTranslation(), src1.ExtractTranslation(), t);
            Vector3 temp_scale = Vector3.Lerp(src0.ExtractScale(), src1.ExtractScale(), t);
            Quaternion temp_rotation = Quaternion.Slerp(src0.ExtractRotation(), src1.ExtractRotation(), t);
            
            return createMatrix(temp_translation, temp_rotation, temp_scale);
        }

        public static float slerp(float src0, float src1, float t)
        {
            src0 = Math.Max(src0, 0.000001f);
            return (float)(Math.Pow(src1 / src0, t) * src0);
        }

        public static Matrix4 rotate(float x_angle, float y_angle, float z_angle)
        {
            Quaternion x_rotation = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(x_angle));
            Quaternion y_rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(y_angle));
            Quaternion z_rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(z_angle));

            Quaternion zyx_rotation = Quaternion.Multiply(Quaternion.Multiply(z_rotation, y_rotation), x_rotation);

            zyx_rotation.Normalize();
            return Matrix4.CreateFromQuaternion(zyx_rotation);
        }



        //------------------------------------------------------
        // Data Helpers
        //------------------------------------------------------
        public static float[] createArray(Vector4 vector)
        {
            return new float[]
            {
                vector.X,
                vector.Y,
                vector.Z,
                vector.W
            };
        }

        public static float[] createArray(Matrix4 matrix)
        {
            List<float> temp_floats = new List<float>();

            temp_floats.AddRange(createArray(matrix.Column0));
            temp_floats.AddRange(createArray(matrix.Column1));
            temp_floats.AddRange(createArray(matrix.Column2));
            temp_floats.AddRange(createArray(matrix.Column3));

            return temp_floats.ToArray();
        }

        public static float[] createArray(Matrix4[] matrices)
        {
            List<float> temp_floats = new List<float>();

            foreach (Matrix4 m in matrices)
            {
                temp_floats.AddRange(createArray(m));
            }

            return temp_floats.ToArray();
        }

        public static Matrix4 createMatrix(float[] matrix_values)
        {
            if (matrix_values.Length != 16)
            {
                throw new Exception("createMatrix(float[] matrix_values) - matrix_values must have length of 16");
            }

            Matrix4 temp_matrix = new Matrix4(
                matrix_values[0], matrix_values[4], matrix_values[8], matrix_values[12],
                matrix_values[1], matrix_values[5], matrix_values[9], matrix_values[13],
                matrix_values[2], matrix_values[6], matrix_values[10], matrix_values[14],
                matrix_values[3], matrix_values[7], matrix_values[11], matrix_values[15]
            );

            return temp_matrix;
        }


        public static Matrix4 createMatrix(Vector3 translation, Vector3 rotation_euler, Vector3 scale)
        {
            // Scale
            Matrix4 temp_scale = Matrix4.CreateScale(scale);

            // Rotation
            Matrix4 temp_rotation = rotate(rotation_euler.X, rotation_euler.Y, rotation_euler.Z);

            // Translation
            Matrix4 temp_translation = Matrix4.CreateTranslation(translation);


            // Build full tranformation matrix
            return temp_scale * (temp_rotation * temp_translation);
        }

        public static Matrix4 createMatrix(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            // Scale
            Matrix4 temp_scale = Matrix4.CreateScale(scale);

            // Rotation
            Matrix4 temp_rotation = Matrix4.CreateFromQuaternion(rotation);

            // Translation
            Matrix4 temp_translation = Matrix4.CreateTranslation(translation);


            // Build full tranformation matrix
            return temp_scale * (temp_rotation * temp_translation);
        }


        //------------------------------------------------------
        // Data Type Converters
        //------------------------------------------------------
        public static BulletSharp.Math.Matrix otk2bullet(OpenTK.Matrix4 matrix)
        {
            return new BulletSharp.Math.Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
        }

        public static OpenTK.Matrix4 bullet2otk(BulletSharp.Math.Matrix matrix)
        {
            return new OpenTK.Matrix4(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
        }

        public static BulletSharp.Math.Vector3 otk2bullet(OpenTK.Vector3 vector)
        {
            return new BulletSharp.Math.Vector3(vector.X, vector.Y, vector.Z);
        }

        public static OpenTK.Vector3 bullet2otk(BulletSharp.Math.Vector3 vector)
        {
            return new OpenTK.Vector3(vector.X, vector.Y, vector.Z);
        }



        //------------------------------------------------------
        // World Object Helpers
        //------------------------------------------------------
        // Rotation Matrix to convert Z-up to Y-up
        public static Matrix4 yup = Matrix4.CreateRotationX((float)(-90.0f * Math.PI / 180.0f));

        public static Matrix4 blender2Kailash(Vector3 translation, Vector3 rotation_euler, Vector3 scale)
        {
            // Build full tranformation matrix
            Matrix4 temp_matrix = createMatrix(translation, rotation_euler, scale);

            // Blender defaults to Z-up. Need to convert to Y-up.
            return temp_matrix * yup;
        }

        public static Matrix4 blender2Kailash(Matrix4 transformation)
        {
            // Blender defaults to Z-up. Need to convert to Y-up.
            return transformation * yup;
        }



    }
}
