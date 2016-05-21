using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
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


            Dictionary<string, Mesh> mesh_dictionary = new Dictionary<string, Mesh>();

            // Change Blender mesh's to y axis up
            Matrix4 yup = Matrix4.CreateRotationX((float)(-90.0f * Math.PI / 180.0f));

            //------------------------------------------------------
            // Create Image Dictionary
            //------------------------------------------------------           
            Dictionary<string, string> image_collection = new Dictionary<string, string>();
            try
            {
                Debug.DebugHelper.logInfo(2, "\tCreating Image Dictionary", dae_file.Library_Images.Image.Count() + " images found");
                foreach (Grendgine_Collada_Image i in dae_file.Library_Images.Image)
                {
                    string id = i.ID;
                    string path = i.Init_From;

                    image_collection.Add(id, path.Substring(1, path.Length - 1));
                }
            }
            catch
            {
                Debug.DebugHelper.logInfo(2, "\tCreating Image Dictionary", "0 images found :<");
            }


            //------------------------------------------------------
            // Create Material Dictionary
            //------------------------------------------------------
            
            Dictionary<string, DAE_Material> material_collection = new Dictionary<string, DAE_Material>();
            try
            {
                Debug.DebugHelper.logInfo(2, "\tCreating Material Dictionary", dae_file.Library_Effects.Effect.Count() + " effects found");
                foreach (Grendgine_Collada_Effect e in dae_file.Library_Effects.Effect)
                {
                    string id = e.ID;

                    DAE_Material temp_material = new DAE_Material(id);
                    temp_material.load(e, image_collection);

                    material_collection.Add(temp_material.id, temp_material);
                }
            }
            catch
            {
                Debug.DebugHelper.logInfo(2, "\tCreating Material Dictionary", "0 effects found :<");
            }

            //------------------------------------------------------
            // Create Visual Scene Dictionary
            //------------------------------------------------------
            Debug.DebugHelper.logInfo(2, "\tCreating Visual Scene Dictionary", "");
            Dictionary<string, Grendgine_Collada_Node> scene_collection = new Dictionary<string, Grendgine_Collada_Node>();
            foreach (Grendgine_Collada_Node n in dae_file.Library_Visual_Scene.Visual_Scene[0].Node)
            {
                string id = n.ID;

                scene_collection.Add(id, n);
            }

            //------------------------------------------------------
            // Create Mesh Dictionary
            //------------------------------------------------------
            Debug.DebugHelper.logInfo(2, "\tCreating Mesh Dictionary", "");
            foreach (Grendgine_Collada_Geometry g in dae_file.Library_Geometries.Geometry)
            {
                // Load Mesh
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

                // Load mesh's VAO
                foreach(Mesh mesh in temp_mesh.submeshes)
                {
                    mesh.setBufferIDs(initVAO(mesh));
                }

                // Load mesh's transformation
                Grendgine_Collada_Node node_match;
                string node_match_string = g.Name.Replace('.', '_');
                if (scene_collection.TryGetValue(node_match_string, out node_match))
                {
                    float[] m_array = node_match.Matrix[0].Value();

                    Matrix4 temp_mat = Matrix4.Identity;
                    temp_mat = new Matrix4(
                        m_array[0], m_array[4], m_array[8], m_array[12],
                        m_array[1], m_array[5], m_array[9], m_array[13],
                        m_array[2], m_array[6], m_array[10], m_array[14],
                        m_array[3], m_array[7], m_array[11], m_array[15]
                                        );
                    temp_mat = temp_mat * yup;

                    temp_mesh.pre_transformation = temp_mat;
                }

                // Add to mesh collection
                mesh_dictionary.Add(temp_mesh.id, temp_mesh);
                
            }

            // Clear dictionaries. dat mesh is loooaded
            image_collection.Clear();
            material_collection.Clear();
            scene_collection.Clear();

            return mesh_dictionary;
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
