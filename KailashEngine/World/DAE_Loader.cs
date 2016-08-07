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
                if (dae_file.Library_Images.Image != null)
                {
                    Debug.DebugHelper.logInfo(2, "\tCreating Image Dictionary", dae_file.Library_Images.Image.Count() + " images found");
                    foreach (Grendgine_Collada_Image i in dae_file.Library_Images.Image)
                    {
                        string id = i.ID;
                        string path = i.Init_From;

                        image_collection.Add(id, path.Substring(1, path.Length - 1));
                    }
                }
                else
                {
                    Debug.DebugHelper.logInfo(2, "\tCreating Image Dictionary", "0 images found :<");
                }
            }
            catch (Exception e)
            {
                Debug.DebugHelper.logError("\tCreating Image Dictionary", e.Message);
            }


            //------------------------------------------------------
            // Create Material Dictionary
            //------------------------------------------------------
            Dictionary<string, DAE_Material> material_collection = new Dictionary<string, DAE_Material>();
            try
            {
                if (dae_file.Library_Effects != null)
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
                else
                {
                    Debug.DebugHelper.logInfo(2, "\tCreating Material Dictionary", "0 effects found :<");
                }
            }
            catch (Exception e)
            {
                Debug.DebugHelper.logError("\tCreating Material Dictionary", e.Message);
            }


            //------------------------------------------------------
            // Create Visual Scene + Skeleton Dictionary
            //------------------------------------------------------
            Debug.DebugHelper.logInfo(2, "\tCreating Visual Scene Dictionary", dae_file.Library_Visual_Scene.Visual_Scene[0].Node.Count() + " visuals found");
            Dictionary<string[], Matrix4> visual_scene_collection = new Dictionary<string[], Matrix4>();
            Dictionary<string, DAE_Skeleton> skeleton_dictionary = new Dictionary<string, DAE_Skeleton>();
            light_matrix_collection = new Dictionary<string, Matrix4>();
            foreach (Grendgine_Collada_Node n in dae_file.Library_Visual_Scene.Visual_Scene[0].Node)
            {
                string id = n.ID;


                

                // Convert Blender transformation to Kailash
                Matrix4 temp_matrix = EngineHelper.createMatrix(
                    new Vector3(n.Translate[0].Value()[0], n.Translate[0].Value()[1], n.Translate[0].Value()[2]),
                    new Vector3(n.Rotate[2].Value()[3], n.Rotate[1].Value()[3], n.Rotate[0].Value()[3]),
                    new Vector3(n.Scale[0].Value()[0], n.Scale[0].Value()[1], n.Scale[0].Value()[2])
                );

                Matrix4 temp_corrected_matrix = EngineHelper.blender2Kailash(temp_matrix);


                // Skeletons
                if (n.node != null)
                {
                    skeleton_dictionary.Add(id, new DAE_Skeleton(id, temp_matrix, n.node));
                }

                // Meshes
                if (n.Instance_Geometry != null)
                {
                    string mesh_id = n.Instance_Geometry[0].URL.Replace("#", "");
                    // Add to scene collection
                    visual_scene_collection.Add(new string[]{ id, mesh_id }, temp_corrected_matrix);
                }

                // Lights
                if (n.Instance_Light != null)
                {
                    string light_id = n.Instance_Light[0].URL.Replace("#", "");
                    // Add to light matrix collection
                    light_matrix_collection.Add(light_id, temp_corrected_matrix);
                }

            }


            //------------------------------------------------------
            // Load Skinning Data
            //------------------------------------------------------
            Dictionary<string, DAE_Skeleton> skin_collection = new Dictionary<string, DAE_Skeleton>();
            try
            {
                if (dae_file.Library_Controllers.Controller != null)
                {
                    Debug.DebugHelper.logInfo(2, "\tLoading Skinning Data", dae_file.Library_Controllers.Controller.Count() + " skins found");
                    foreach (Grendgine_Collada_Controller c in dae_file.Library_Controllers.Controller)
                    {
                        string skin_id = c.ID;
                        string skeleton_id = c.Name.Replace(".", "_");
;

                        DAE_Skeleton temp_skeleton;
                        if(skeleton_dictionary.TryGetValue(skeleton_id, out temp_skeleton))
                        {
                            Grendgine_Collada_Skin temp_skin = c.Skin;
                            string mesh_id = temp_skin.source.Replace("#", "");

                            //------------------------------------------------------
                            // Load BSM into skeleton
                            //------------------------------------------------------
                            Matrix4 BSM = EngineHelper.createMatrix(temp_skin.Bind_Shape_Matrix.Value());
                            temp_skeleton.BSM = BSM;

                            //------------------------------------------------------
                            // Load Sources
                            //------------------------------------------------------
                            Dictionary<string, Grendgine_Collada_Source> source_dictionary = temp_skin.Source.ToDictionary(s => s.ID, s => s);

                            string joint_source_id = "";
                            string IBM_source_id = "";
                            string weights_source_id = "";

                            foreach(Grendgine_Collada_Input_Unshared i in temp_skin.Joints.Input)
                            {
                                switch(i.Semantic.ToString())
                                {
                                    case "JOINT":
                                        joint_source_id = i.source.Replace("#", "");
                                        break;
                                    case "INV_BIND_MATRIX":
                                        IBM_source_id = i.source.Replace("#", "");
                                        break;
                                }
                            }
                            foreach (Grendgine_Collada_Input_Shared i in temp_skin.Vertex_Weights.Input)
                            {
                                switch (i.Semantic.ToString())
                                {
                                    case "WEIGHT":
                                        weights_source_id = i.source.Replace("#", "");
                                        break;
                                }
                            }

                            Grendgine_Collada_Source IBM_source = source_dictionary[IBM_source_id];
                            Grendgine_Collada_Source weights_source = source_dictionary[weights_source_id];

                            //------------------------------------------------------
                            // Load Bone IDs
                            //------------------------------------------------------
                            string[] bone_names = source_dictionary[joint_source_id].Name_Array.Value();
                            int[] bone_ids = new int[bone_names.Length];
                            for (int i = 0; i < bone_names.Length; i++)
                            {
                                bone_ids[i] = temp_skeleton.bone_ids[bone_names[i]];
                            }

                            //------------------------------------------------------
                            // Load IBMs
                            //------------------------------------------------------
                            int IBM_stride = 16;                     
                            for (int i = 0; i < bone_names.Length; i++)
                            {
                                float[] temp_IBM = new float[IBM_stride];
                                Array.Copy(IBM_source.Float_Array.Value(), i * IBM_stride, temp_IBM, 0, IBM_stride);

                                string current_bone_name = bone_names[i];
                                int current_bone_id = temp_skeleton.bone_ids[current_bone_name];

                                temp_skeleton.bones[current_bone_id].IBM = EngineHelper.createMatrix(temp_IBM);
                            }

                            //------------------------------------------------------
                            // Load Vertex Weights
                            //------------------------------------------------------
                            float[] vertex_weights = weights_source.Float_Array.Value();
                            int[] bones_and_weights = temp_skin.Vertex_Weights.V.Value();
                            string vcount_string = temp_skin.Vertex_Weights.VCount.Value_As_String;
                            int[] vcount = Grendgine_Collada_Parse_Utils.String_To_Int(vcount_string.Remove(vcount_string.Length-1, 1));

                            int current_weight_index = 0;
                            for (int i = 0; i < vcount.Length; i ++)
                            {
                                int num_bones = vcount[i];
                                List<DAE_Skeleton.VertexWeight> temp_weights = new List<DAE_Skeleton.VertexWeight>();

                                for (int j = 0; j < num_bones*2; j+=2)
                                {
                                    int bone_index = bones_and_weights[current_weight_index + j];
                                    int weight_index = bones_and_weights[current_weight_index + j + 1];

                                    temp_weights.Add(new DAE_Skeleton.VertexWeight(bone_ids[bone_index], i, vertex_weights[weight_index]));
                                }
                                current_weight_index += num_bones*2;

                                temp_skeleton.vertex_weights.Add(i, temp_weights.ToArray());
                            }


                            //------------------------------------------------------
                            // Map mesh_id to the mesh's skeleton
                            //------------------------------------------------------
                            skin_collection.Add(mesh_id, temp_skeleton);
                        }
                    }
                }
                else
                {
                    Debug.DebugHelper.logInfo(2, "\tLoading Skinning Data", "0 skins found :<");
                }
            }
            catch (Exception e)
            {
                Debug.DebugHelper.logError("\tLoading Skinning Data", e.Message);
            }



            //------------------------------------------------------
            // Create Mesh Dictionary
            //------------------------------------------------------
            Dictionary<string, DAE_Mesh> mesh_collection = new Dictionary<string, DAE_Mesh>();
            try
            {
                if (dae_file.Library_Geometries != null)
                {
                    Debug.DebugHelper.logInfo(2, "\tCreating Mesh Dictionary", dae_file.Library_Geometries.Geometry.Count() + " meshes found");
                    foreach (Grendgine_Collada_Geometry g in dae_file.Library_Geometries.Geometry)
                    {
                        // Load Mesh
                        string mesh_id = g.Name.Replace('.', '_') + "-mesh";

                        DAE_Mesh temp_mesh = new DAE_Mesh(mesh_id, g);
                        try
                        {
                            DAE_Skeleton temp_skeleton;
                            if(skin_collection.TryGetValue(mesh_id, out temp_skeleton))
                            {
                                temp_mesh.load(material_collection, temp_skeleton);
                            }
                            else
                            {
                                temp_mesh.load(material_collection);
                            }
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
                else
                {
                    Debug.DebugHelper.logInfo(2, "\tCreating Mesh Dictionary", "0 meshes found :<");
                }
            }
            catch (Exception e)
            {
                Debug.DebugHelper.logError("\tCreating Mesh Dictionary", e.Message);
            }


            //------------------------------------------------------
            // Create Animation Dictionary
            //------------------------------------------------------
            Dictionary<string, Animator> animator_collection = new Dictionary<string, Animator>();
            try
            {
                if (dae_file.Library_Animations != null)
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

                            switch (action)
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
                            if (animator_collection.TryGetValue(id, out temp_animator))
                            {
                                animator_collection.Remove(id);
                            }
                            else
                            {
                                temp_animator = new Animator(id);
                            }

                            // Create source dictionary
                            Dictionary<string, Grendgine_Collada_Float_Array> source_float_dictionary = a.Source.ToDictionary(s => s.ID, s => s.Float_Array);
                            Dictionary<string, Grendgine_Collada_Name_Array> source_string_dictionary = a.Source.ToDictionary(s => s.ID, s => s.Name_Array);

                            // Create sampler dictionary
                            Dictionary<string, string> sampler_dictionary = a.Sampler[0].Input.ToDictionary(i => i.Semantic.ToString(), i => i.source);


                            // Get Key interpolation method
                            string source_interpolation_id = sampler_dictionary["INTERPOLATION"].Replace("#", "");
                            string[] key_frame_interpolations = source_string_dictionary[source_interpolation_id].Value();

                            // Get Key Frame times
                            string source_input_id = sampler_dictionary["INPUT"].Replace("#", "");
                            float[] key_frame_times = source_float_dictionary[source_input_id].Value();

                            // Get Key Frame data
                            string source_output_id = sampler_dictionary["OUTPUT"].Replace("#", "");
                            float[] key_frame_data = source_float_dictionary[source_output_id].Value();

                            // Get Key Frame in tangent data
                            string source_t1_id = sampler_dictionary["IN_TANGENT"].Replace("#", "");
                            float[] key_frame_data_b1 = source_float_dictionary[source_t1_id].Value();

                            // Get Key Frame out tangent data
                            string source_t2_id = sampler_dictionary["OUT_TANGENT"].Replace("#", "");
                            float[] key_frame_data_b2 = source_float_dictionary[source_t2_id].Value();


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

                            source_float_dictionary.Clear();
                            source_string_dictionary.Clear();
                            sampler_dictionary.Clear();
                        }
                    }

                    foreach (Animator a in animator_collection.Values)
                    {
                        a.calcLastFrame();
                    }
                }
                else
                {
                    Debug.DebugHelper.logInfo(2, "\tCreating Animation Dictionary", "0 animations found :<");
                }
            }
            catch (Exception e)
            {
                Debug.DebugHelper.logError("\tCreating Animation Dictionary", e.Message);
            }



            //------------------------------------------------------
            // Run Through Visual Scenes
            //------------------------------------------------------
            Debug.DebugHelper.logInfo(2, "\tLoading Visual Scenes", "");
            unique_mesh_collection = new Dictionary<string, UniqueMesh>();
            foreach (KeyValuePair<string[], Matrix4> keypair in visual_scene_collection)
            {
                string id = keypair.Key[0];
                string object_id = keypair.Key[1];

                Debug.DebugHelper.logInfo(3, "\t\tLoading Visual Scene", id);

                Console.WriteLine(object_id);

                Matrix4 temp_matrix = keypair.Value;

                // Load unique Mesh and its transformation
                DAE_Mesh m;
                if (mesh_collection.TryGetValue(object_id, out m))
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

            DAE_Mesh m2;
            if (mesh_collection.TryGetValue("Cube_002-mesh", out m2))
            {
                UniqueMesh temp_unique_mesh_test = new UniqueMesh("Cube", m2, EngineHelper.yup);
                unique_mesh_collection.Add("Cube", temp_unique_mesh_test);
            }
            DAE_Mesh m3;
            if (mesh_collection.TryGetValue("Icosphere-mesh", out m3))
            {
                UniqueMesh temp_unique_mesh_test = new UniqueMesh("Icosphere", m3, EngineHelper.yup);
                unique_mesh_collection.Add("Icosphere", temp_unique_mesh_test);
            }


            // Clear dictionaries. dat mesh is loooaded
            image_collection.Clear();
            material_collection.Clear();
            mesh_collection.Clear();
            animator_collection.Clear();
            visual_scene_collection.Clear();
            skeleton_dictionary.Clear();
            skin_collection.Clear();
        }



        private static int[] initVAO(Mesh mesh)
        {

            int temp_vbo, temp_ibo, temp_vao = 0;
            
            
            // Create and fill Vertex Buffer
            GL.GenBuffers(1, out temp_vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, temp_vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)mesh.vertex_data_size, mesh.vertex_data, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Create and fill Index Buffer
            GL.GenBuffers(1, out temp_ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, temp_ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)mesh.index_data_size, mesh.index_data, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);


            // Create and fill Vertex Array Buffer
            GL.GenVertexArrays(1, out temp_vao);
            GL.BindVertexArray(temp_vao);

            int stride = Marshal.SizeOf(typeof(Mesh.Vertex));

            GL.BindBuffer(BufferTarget.ArrayBuffer, temp_vbo);

            // Get position data into attrib 0
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, Marshal.OffsetOf(typeof(Mesh.Vertex), "position"));

            // Get normal data into attrib 1
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, Marshal.OffsetOf(typeof(Mesh.Vertex), "normal"));

            // Get tangent data into attrib 2
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, Marshal.OffsetOf(typeof(Mesh.Vertex), "tangent"));

            // Get uv data into attrib 3
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, stride, Marshal.OffsetOf(typeof(Mesh.Vertex), "uv"));

            // Get bone ids data into attrib 4
            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, stride, Marshal.OffsetOf(typeof(Mesh.Vertex), "bone_ids"));

            // Get bone weights data into attrib 5
            GL.EnableVertexAttribArray(5);
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, stride, Marshal.OffsetOf(typeof(Mesh.Vertex), "bone_weights"));


            // Unbind all buffers
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            int[] temp_gl_vars = { temp_vbo, temp_ibo, temp_vao };
            return temp_gl_vars;
        }


    }
}
