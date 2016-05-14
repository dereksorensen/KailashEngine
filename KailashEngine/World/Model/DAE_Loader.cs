using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

using grendgine_collada;

namespace KailashEngine.World.Model
{
    static class DAE_Loader
    {

        


        public static Dictionary<string, Mesh> load(string filename)
        {
            if(!File.Exists(filename))
            {
                throw new Exception("World File Not Found\n" + filename);
            }

            Debug.DebugHelper.logInfo(1, "Loading Collada File", Path.GetFileName(filename));
            Grendgine_Collada dae_file = Grendgine_Collada.Grendgine_Load_File(filename);


            int geometry_count = dae_file.Library_Geometries.Geometry.Count();
            Debug.DebugHelper.logInfo(2, "\tNumber of Meshes", geometry_count.ToString());

            //------------------------------------------------------
            // Create Image Dictionary
            //------------------------------------------------------
            Debug.DebugHelper.logInfo(2, "\tCreating Image Dictionary", "");
            Dictionary<string, string> image_collection = new Dictionary<string, string>();

            Grendgine_Collada_Library_Images images = dae_file.Library_Images;
            foreach (Grendgine_Collada_Image i in images.Image)
            {
                string id = i.ID;
                string path = i.Init_From.ToString();

                image_collection.Add(id, path.Substring(1, path.Length - 1));
            }

            //------------------------------------------------------
            // Create Material Dictionary
            //------------------------------------------------------
            Debug.DebugHelper.logInfo(2, "\tCreating Material Dictionary", "");
            Dictionary<string, DAE_Material> material_collection = new Dictionary<string, DAE_Material>();
            foreach (Grendgine_Collada_Effect e in dae_file.Library_Effects.Effect)
            {
                string id = e.ID;

                DAE_Material temp_material = new DAE_Material(id);
                temp_material.load(e, image_collection);

                material_collection.Add(temp_material.id, temp_material);
            }

            //------------------------------------------------------
            // Create Mesh Dictionary
            //------------------------------------------------------
            Debug.DebugHelper.logInfo(2, "\tCreating Mesh Dictionary", "");
            foreach (Grendgine_Collada_Geometry g in dae_file.Library_Geometries.Geometry)
            {
                DAE_Mesh temp_mesh = new DAE_Mesh(g.Name.Replace('.', '_') + "-mesh", g);

                try
                {
                    temp_mesh.load(material_collection);
                }
                catch (Exception e)
                {
                    Debug.DebugHelper.logError(e.Message, "");
                    continue;
                }

                foreach (Mesh m in temp_mesh.submeshes)
                {
                    Console.WriteLine(m.id);
                }
            }

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
