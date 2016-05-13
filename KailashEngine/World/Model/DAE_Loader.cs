using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace KailashEngine.World.Model
{
    static class DAE_Loader
    {



        public static Dictionary<string, Mesh> load(string filename)
        {
            return null;
        }



        private static int[] initVAO(Mesh mesh)
        {

            int temp_vbo, temp_ibo, temp_vao = 0;


            //Create and fill Vertex Buffer
            GL.GenBuffers(1, out temp_vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, temp_vbo);
            GL.BufferData<Mesh.Vertex>(BufferTarget.ArrayBuffer, (IntPtr)mesh.vertex_data_size, mesh.vertex_data, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            //Create and fill Index Buffer
            GL.GenBuffers(1, out temp_ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, temp_ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)mesh.index_data_size, mesh.index_data, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);


            //Create and fill Vertex Array Buffer
            GL.GenVertexArrays(1, out temp_vao);
            GL.BindVertexArray(temp_vao);

            int stride = Marshal.SizeOf(typeof(Mesh.Vertex));

            GL.BindBuffer(BufferTarget.ArrayBuffer, temp_vbo);

            //Get vertex data into attrib 0
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, Marshal.OffsetOf(typeof(Mesh.Vertex), "position"));

            //Get normal data into attrib 1
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, Marshal.OffsetOf(typeof(Mesh.Vertex), "normal"));

            //Get tangent data into attrib 2
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, Marshal.OffsetOf(typeof(Mesh.Vertex), "tangent"));


            //Get uv data into attrib 3
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, stride, Marshal.OffsetOf(typeof(Mesh.Vertex), "uv"));


            GL.BindBuffer(BufferTarget.ElementArrayBuffer, temp_ibo);
            GL.BindVertexArray(0);


            int[] temp_gl_vars = { temp_vbo, temp_ibo, temp_vao };
            return temp_gl_vars;
        }


    }
}
