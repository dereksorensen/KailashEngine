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

        


        public static void load(string filename, MaterialManager material_manager, out Dictionary<string, UniqueMesh> unique_mesh_collection, out Dictionary<string, LightLoader.LightLoaderExtras> light_extras)
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
                        path = path.Substring(1, path.Length - 1);
                        path = EngineHelper.getPath_MaterialTextures(path);

                        image_collection.Add(id, path);
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
                        temp_material.load(e, image_collection, material_manager);

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
            // Create Animation Dictionary
            //------------------------------------------------------
            Dictionary<string, ObjectAnimator> object_animator_collection = new Dictionary<string, ObjectAnimator>();
            Dictionary<string, SkeletonAnimator> skeleton_animator_collection = new Dictionary<string, SkeletonAnimator>();
            try
            {
                if (dae_file.Library_Animations != null)
                {
                    Debug.DebugHelper.logInfo(2, "\tCreating Animation Dictionary", dae_file.Library_Animations.Animation.Count() + " animations found");
                    foreach (Grendgine_Collada_Animation a in dae_file.Library_Animations.Animation)
                    {

                        //------------------------------------------------------
                        // Get match strings to decide what kind of Animation this is
                        //------------------------------------------------------
                        // Get the ID of the object or bone we are animating
                        Match object_match = Regex.Match(a.Channel[0].Target, "(.+)/.+");
                        string object_id = object_match.Groups[1].ToString();

                        // Match if this is an object animation
                        Match object_animation_match = Regex.Match(a.ID, object_id + "_(location|rotation_euler|scale)_(X|Y|Z)");

                        // Match if this is a skeletal animation
                        Match skeleton_animation_match = Regex.Match(a.ID, "(.+)_" + object_id + "_pose_matrix");


                        //------------------------------------------------------
                        // Create Source and Sampler Dictionaries
                        //------------------------------------------------------
                        // Create source dictionary
                        Dictionary<string, Grendgine_Collada_Source> source_dictionary = a.Source.ToDictionary(s => s.ID, s => s);

                        // Create sampler dictionary
                        Dictionary<string, string> sampler_dictionary = a.Sampler[0].Input.ToDictionary(i => i.Semantic.ToString(), i => i.source);


                        //------------------------------------------------------
                        // Get Common Animation Data
                        //------------------------------------------------------
                        // Get Key interpolation method
                        string source_interpolation_id = sampler_dictionary["INTERPOLATION"].Replace("#", "");
                        string[] key_frame_interpolations = source_dictionary[source_interpolation_id].Name_Array.Value();

                        // Get Key Frame times
                        string source_input_id = sampler_dictionary["INPUT"].Replace("#", "");
                        float[] key_frame_times = source_dictionary[source_input_id].Float_Array.Value();

                        // Get Key Frame data
                        string source_output_id = sampler_dictionary["OUTPUT"].Replace("#", "");
                        float[] key_frame_data = source_dictionary[source_output_id].Float_Array.Value();
                        int key_frame_data_stride = (int)source_dictionary[source_output_id].Technique_Common.Accessor.Stride;


                        //------------------------------------------------------
                        // Object Animation
                        //------------------------------------------------------
                        if (object_animation_match.Success && !skeleton_animation_match.Success)
                        {
                            string id = object_id;
                            string action = object_animation_match.Groups[1].ToString();
                            string channel = object_animation_match.Groups[2].ToString();

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
                            ObjectAnimator temp_animator;
                            if (object_animator_collection.TryGetValue(id, out temp_animator))
                            {
                                object_animator_collection.Remove(id);
                            }
                            else
                            {
                                temp_animator = new ObjectAnimator(id);
                            }


                            // Get Key Frame in tangent data
                            string source_t1_id = sampler_dictionary["IN_TANGENT"].Replace("#", "");
                            float[] key_frame_data_b1 = source_dictionary[source_t1_id].Float_Array.Value();

                            // Get Key Frame out tangent data
                            string source_t2_id = sampler_dictionary["OUT_TANGENT"].Replace("#", "");
                            float[] key_frame_data_b2 = source_dictionary[source_t2_id].Float_Array.Value();


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

                            object_animator_collection.Add(id, temp_animator);
                        }


                        //------------------------------------------------------
                        // Skeletal Animation
                        //------------------------------------------------------
                        if (skeleton_animation_match.Success && !object_animation_match.Success)
                        {
                            string id = skeleton_animation_match.Groups[1].ToString();
                            string bone_name = object_id;

                            

                            // Create new or use existing Animator
                            SkeletonAnimator temp_animator;
                            if (skeleton_animator_collection.TryGetValue(id, out temp_animator))
                            {
                                skeleton_animator_collection.Remove(id);
                            }
                            else
                            {
                                temp_animator = new SkeletonAnimator(id);
                            }

                            // Loop through key frames and add to animator
                            for (int i = 0; i < key_frame_times.Length; i++)
                            {
                                int current_frame_data_index = i * key_frame_data_stride;

                                float current_frame_time = key_frame_times[i];
                                float[] current_frame_data = new float[16];

                                // Create Matrix from each 16 strided float
                                Array.Copy(key_frame_data, current_frame_data_index, current_frame_data, 0, key_frame_data_stride);
                                Matrix4 current_frame_data_matrix = EngineHelper.createMatrix(current_frame_data);

                                temp_animator.addKeyFrame(bone_name, current_frame_time, current_frame_data_matrix);
                            }

                            skeleton_animator_collection.Add(id, temp_animator);

                        }

                        source_dictionary.Clear();
                        sampler_dictionary.Clear();
                    }


                    // Get last key frame for each animator
                    foreach (ObjectAnimator a in object_animator_collection.Values)
                    {
                        a.calcLastFrame();
                    }
                    foreach (SkeletonAnimator a in skeleton_animator_collection.Values)
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
            // Create Visual Scene + Skeleton Dictionary
            //------------------------------------------------------
            Debug.DebugHelper.logInfo(2, "\tCreating Visuals Dictionary", dae_file.Library_Visual_Scene.Visual_Scene[0].Node.Count() + " visuals found");
            List<Grendgine_Collada_Node> mesh_visual_collection = new List<Grendgine_Collada_Node>();
            List<Grendgine_Collada_Node> controlled_visual_collection = new List<Grendgine_Collada_Node>();
            List<Grendgine_Collada_Node> light_visual_collection = new List<Grendgine_Collada_Node>();
            Dictionary<string, DAE_Skeleton> skeleton_dictionary = new Dictionary<string, DAE_Skeleton>();
            light_extras = new Dictionary<string, LightLoader.LightLoaderExtras>();
            foreach (Grendgine_Collada_Node node in dae_file.Library_Visual_Scene.Visual_Scene[0].Node)
            {
                string id = node.ID;
          
                // Convert Blender transformation to Kailash
                Matrix4 temp_matrix = EngineHelper.createMatrix(
                    new Vector3(node.Translate[0].Value()[0], node.Translate[0].Value()[1], node.Translate[0].Value()[2]),
                    new Vector3(node.Rotate[2].Value()[3], node.Rotate[1].Value()[3], node.Rotate[0].Value()[3]),
                    new Vector3(node.Scale[0].Value()[0], node.Scale[0].Value()[1], node.Scale[0].Value()[2])
                );
                Matrix4 temp_corrected_matrix = EngineHelper.blender2Kailash(temp_matrix);

                // Skeletons
                if (node.node != null && node.node[0].Instance_Geometry == null && node.node[0].Instance_Geometry == null)
                {
                    DAE_Skeleton temp_skeleton = new DAE_Skeleton(id, temp_matrix, node.node);
                    SkeletonAnimator temp_animator;
                    if(skeleton_animator_collection.TryGetValue(temp_skeleton.id, out temp_animator))
                    {
                        temp_skeleton.animator = temp_animator;
                    }
                    skeleton_dictionary.Add(id, temp_skeleton);
                    continue;
                }

                // Lights
                if (node.Instance_Light != null)
                {
                    string light_id = node.Instance_Light[0].URL.Replace("#", "");
                    ObjectAnimator temp_animator = null;
                    object_animator_collection.TryGetValue(light_id.Replace("-light", ""), out temp_animator);
                    LightLoader.LightLoaderExtras temp_light_extras = new LightLoader.LightLoaderExtras(temp_corrected_matrix, temp_animator);
                    // Add to light matrix collection
                    light_extras.Add(light_id, temp_light_extras);
                    continue;
                }

                // Meshes
                if (node.Instance_Geometry != null)
                {
                    string mesh_id = node.Instance_Geometry[0].URL.Replace("#", "");
                    // Add to scene collection
                    mesh_visual_collection.Add(node);
                    continue;
                }

                // Controlled Objects
                if (node.Instance_Controller != null)
                {
                    string controller_id = node.Instance_Controller[0].URL.Replace("#", "");
                    // Add to scene collection
                    controlled_visual_collection.Add(node);
                    continue;
                }

            }


            //------------------------------------------------------
            // Load Skinning Data
            //------------------------------------------------------
            Dictionary<string, string> mesh_2_skeleton = new Dictionary<string, string>();
            Dictionary<string, string> skin_2_mesh = new Dictionary<string, string>();
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
                            // Map mesh_id to the Mesh's skeleton_id
                            //------------------------------------------------------
                            mesh_2_skeleton.Add(mesh_id, skeleton_id);
                            skin_2_mesh.Add(skin_id, mesh_id);
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
                            string skeleton_id;
                            if(mesh_2_skeleton.TryGetValue(mesh_id, out skeleton_id))
                            {
                                temp_mesh.load(material_collection, skeleton_dictionary[skeleton_id]);
                            }
                            else
                            {
                                temp_mesh.load(material_collection);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.DebugHelper.logError("[ FAILED ] Loading Mesh: " + mesh_id, e.Message);
                            continue;
                        }
                        
                        // Load mesh's VAO
                        foreach (Mesh mesh in temp_mesh.submeshes)
                        {
                            mesh.setBufferIDs(loadVAO(mesh));
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
            // Create Mesh Visuals
            //------------------------------------------------------
            Debug.DebugHelper.logInfo(2, "\tLoading Mesh Visuals", "");
            unique_mesh_collection = new Dictionary<string, UniqueMesh>();
            foreach (Grendgine_Collada_Node node in mesh_visual_collection)
            {
                string visual_id = node.ID;
                string mesh_id = node.Instance_Geometry[0].URL.Replace("#", "");

                Debug.DebugHelper.logInfo(3, "\t\tLoading Visual Scene", visual_id);

                // Convert Blender transformation to Kailash
                Matrix4 temp_matrix = EngineHelper.blender2Kailash(EngineHelper.createMatrix(
                    new Vector3(node.Translate[0].Value()[0], node.Translate[0].Value()[1], node.Translate[0].Value()[2]),
                    new Vector3(node.Rotate[2].Value()[3], node.Rotate[1].Value()[3], node.Rotate[0].Value()[3]),
                    new Vector3(node.Scale[0].Value()[0], node.Scale[0].Value()[1], node.Scale[0].Value()[2])
                ));

                // Load unique Mesh and its transformation
                DAE_Mesh m;
                if (mesh_collection.TryGetValue(mesh_id, out m))
                {
                    UniqueMesh temp_unique_mesh = new UniqueMesh(visual_id, m, temp_matrix);

                    // Add animator to unique mesh if one exists
                    ObjectAnimator temp_animator;
                    if (object_animator_collection.TryGetValue(visual_id, out temp_animator))
                    {
                        temp_unique_mesh.animator = temp_animator;
                    }

                    // Add to mesh collection
                    unique_mesh_collection.Add(visual_id, temp_unique_mesh);
                }

            }

            //------------------------------------------------------
            // Create Skin Visuals
            //------------------------------------------------------
            Debug.DebugHelper.logInfo(2, "\tLoading Controlled Visuals", "");
            foreach (Grendgine_Collada_Node node in controlled_visual_collection)
            {
                string visual_id = node.ID;
                string controller_id = node.Instance_Controller[0].URL.Replace("#", "");
                string mesh_id;

                if (skin_2_mesh.TryGetValue(controller_id, out mesh_id))
                {
                    Debug.DebugHelper.logInfo(3, "\t\tLoading Visual Scene", visual_id + " (" + controller_id + " )");

                    // Convert Blender transformation to Kailash
                    Matrix4 temp_matrix = EngineHelper.blender2Kailash(EngineHelper.createMatrix(
                        new Vector3(node.Translate[0].Value()[0], node.Translate[0].Value()[1], node.Translate[0].Value()[2]),
                        new Vector3(node.Rotate[2].Value()[3], node.Rotate[1].Value()[3], node.Rotate[0].Value()[3]),
                        new Vector3(node.Scale[0].Value()[0], node.Scale[0].Value()[1], node.Scale[0].Value()[2])
                    ));

                    // Load unique Mesh and its transformation
                    DAE_Mesh m;
                    if (mesh_collection.TryGetValue(mesh_id, out m))
                    {
                        UniqueMesh temp_unique_mesh_test = new UniqueMesh(visual_id, m, temp_matrix);
                        unique_mesh_collection.Add(visual_id, temp_unique_mesh_test);
                    }
                }
            }



            // Clear dictionaries. dat mesh is loooaded
            image_collection.Clear();
            material_collection.Clear();
            mesh_collection.Clear();
            object_animator_collection.Clear();
            skeleton_animator_collection.Clear();
            mesh_visual_collection.Clear();
            controlled_visual_collection.Clear();
            light_visual_collection.Clear();
            skeleton_dictionary.Clear();
            mesh_2_skeleton.Clear();
        }



        private static int[] loadVAO(Mesh mesh)
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
