using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using grendgine_collada;

using KailashEngine.World.Model;
using KailashEngine.Animation;

namespace KailashEngine.World
{
    static class DAE_Loader
    {

        


        public static void load(string filename, out Dictionary<string, UniqueMesh> unique_mesh_collection, out Dictionary<string, Matrix4> light_matrix_collection)
        {
            if(!File.Exists(filename))
            {
                throw new Exception("World File Not Found\n" + filename);
            }

            Debug.DebugHelper.logInfo(1, "Loading Collada File", Path.GetFileName(filename));
            Grendgine_Collada dae_file = Grendgine_Collada.Grendgine_Load_File(filename);


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
            // Create Mesh Dictionary
            //------------------------------------------------------
            Dictionary<string, DAE_Mesh> mesh_collection = new Dictionary<string, DAE_Mesh>();
            try
            {
                Debug.DebugHelper.logInfo(2, "\tCreating Mesh Dictionary", dae_file.Library_Geometries.Geometry.Count() + " meshes found");            
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
                    foreach (Mesh mesh in temp_mesh.submeshes)
                    {
                        mesh.setBufferIDs(initVAO(mesh));
                    }

                    // Add to Mesh Collection
                    mesh_collection.Add(temp_mesh.id, temp_mesh);

                }
            }
            catch
            {
                Debug.DebugHelper.logInfo(2, "\tCreating Mesh Dictionary", "0 meshes found :<");
            }

            //------------------------------------------------------
            // Create Animation Dictionary
            //------------------------------------------------------
            Dictionary<string, Animator> animator_collection = new Dictionary<string, Animator>();
            try
            {
                Debug.DebugHelper.logInfo(2, "\tCreating Animation Dictionary", dae_file.Library_Animations.Animation.Count() + " animations found");
                foreach (Grendgine_Collada_Animation a in dae_file.Library_Animations.Animation)
                {
 
                    Match match = Regex.Match(a.ID, "(.+)_(location|rotation_euler|scale)_(X|Y|Z)");

                    if (match.Groups.Count == 4)
                    {
                        string id = match.Groups[1].ToString();
                        string action = match.Groups[2].ToString();
                        string channel = match.Groups[3].ToString();

                        switch(action)
                        {
                            case "location":
                                action = AnimationHelper.translate;
                                break;
                            case "rotation_euler":
                                action = AnimationHelper.rotation_euler;
                                break;
                            case "scale":
                                action = AnimationHelper.scale;
                                break;
                        }

                        // Create new or use existing Animator
                        Animator temp_animator;
                        if(animator_collection.TryGetValue(id, out temp_animator))
                        {
                            animator_collection.Remove(id);
                        }
                        else
                        {
                            temp_animator = new Animator(id);
                        }

                        // Create source dictionary
                        Dictionary<string, Grendgine_Collada_Float_Array> source_dictionary = a.Source.ToDictionary(s => s.ID, s => s.Float_Array);

                        // Create sampler dictionary
                        Dictionary<string, string> sampler_dictionary = a.Sampler[0].Input.ToDictionary(i => i.Semantic.ToString(), i => i.source);


                        // Get Key Frame times
                        string source_input_id = sampler_dictionary["INPUT"].Replace("#", "");
                        float[] key_frame_times = source_dictionary[source_input_id].Value();

                        // Get Key Frame data
                        string source_output_id = sampler_dictionary["OUTPUT"].Replace("#", "");
                        float[] key_frame_data = source_dictionary[source_output_id].Value();

                        // Get Key Frame in tangent data
                        string source_t1_id = sampler_dictionary["IN_TANGENT"].Replace("#", "");
                        float[] key_frame_data_b1 = source_dictionary[source_t1_id].Value();

                        // Get Key Frame out tangent data
                        string source_t2_id = sampler_dictionary["OUT_TANGENT"].Replace("#", "");
                        float[] key_frame_data_b2 = source_dictionary[source_t2_id].Value();

                     
                        // Loop through key frames and add to animator
                        for (int i = 0; i < key_frame_times.Length; i++)
                        {
                            float current_frame_time = key_frame_times[i];
                            float current_frame_data = key_frame_data[i];
                            int j = i * 2;
                            Vector4 current_frame_data_bezier = new Vector4(
                                key_frame_data_b1[j],
                                key_frame_data_b1[j + 1],
                                key_frame_data_b2[j],
                                key_frame_data_b2[j + 1]
                            );

                            temp_animator.addKeyFrame(current_frame_time, action, channel, current_frame_data, current_frame_data_bezier);
                        }

                        animator_collection.Add(id, temp_animator);

                        source_dictionary.Clear();
                        sampler_dictionary.Clear();
                    }

                }
            }
            catch
            {
                Debug.DebugHelper.logInfo(2, "\tCreating Animation Dictionary", "0 animations found :<");
            }



            //------------------------------------------------------
            // Run Through Visual Scenes
            //------------------------------------------------------
            Debug.DebugHelper.logInfo(2, "\tCreating Visual Scene Dictionary", "");
            unique_mesh_collection = new Dictionary<string, UniqueMesh>();
            light_matrix_collection = new Dictionary<string, Matrix4>();
            foreach (Grendgine_Collada_Node n in dae_file.Library_Visual_Scene.Visual_Scene[0].Node)
            {
                string id = n.ID;
                Debug.DebugHelper.logInfo(3, "\tLoading Visual Scene", id);


                // Convert Blender transformation to Kailash
                Matrix4 temp_matrix = EngineHelper.blender2Kailash(
                    new Vector3(n.Translate[0].Value()[0], n.Translate[0].Value()[1], n.Translate[0].Value()[2]),
                    new Vector3(n.Rotate[2].Value()[3], n.Rotate[1].Value()[3], n.Rotate[0].Value()[3]),
                    new Vector3(n.Scale[0].Value()[0], n.Scale[0].Value()[1], n.Scale[0].Value()[2])
                );


                // Meshes
                if(n.Instance_Geometry != null)
                {
                    string mesh_id = n.Instance_Geometry[0].URL.Replace("#", "");
                    // Load unique Mesh and its transformation
                    DAE_Mesh m;
                    if (mesh_collection.TryGetValue(mesh_id, out m))
                    {
                        UniqueMesh temp_unique_mesh = new UniqueMesh(id, m, temp_matrix);

                        // Add animator to unique mesh if one exists
                        Animator temp_animator;
                        if (animator_collection.TryGetValue(id, out temp_animator))
                        {
                            temp_unique_mesh.animator = temp_animator;
                        }

                        // Add to mesh collection
                        unique_mesh_collection.Add(id, temp_unique_mesh);
                    }
                }

                // Lights
                if (n.Instance_Light != null)
                {
                    string light_id = n.Instance_Light[0].URL.Replace("#", "");
                    // Add to light matrix collection
                    light_matrix_collection.Add(light_id, temp_matrix);
                }

            }


            // Clear dictionaries. dat mesh is loooaded
            image_collection.Clear();
            material_collection.Clear();
            mesh_collection.Clear();
            animator_collection.Clear();

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


            // Unbind all buffers
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            int[] temp_gl_vars = { temp_vbo, temp_ibo, temp_vao };
            return temp_gl_vars;
        }


    }
}
