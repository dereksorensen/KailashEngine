using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using grendgine_collada;

namespace KailashEngine.World.Model
{
    class DAE_Mesh : Mesh
    {

        // DAE Objects
        private Grendgine_Collada_Geometry _geometry;



        public DAE_Mesh(string id, Grendgine_Collada_Geometry geometry)
            : base (id)
        {
            _geometry = geometry;
            _submesh_count = geometry.Mesh.Polylist.Count();

            _submeshes = new List<Mesh>();
        }

        public void load(Dictionary<string, DAE_Material> material_collection)
        {

            List<Vector3> temp_position = new List<Vector3>();         
            List<Vector3> temp_normal = new List<Vector3>();
            List<Vector2> temp_uv = new List<Vector2>();

            // Exit if mesh doesn't have all required data sources
            if (_geometry.Mesh.Source.Count() < 3)
            {
                throw new Exception(Debug.DebugHelper.format("\t[ FAILED ] Mesh: " + _id, "Mesh must include position/normal/texcoord)"));
            }

            //------------------------------------------------------
            // Load Position / Normal / UV from Mesh
            //------------------------------------------------------
            foreach (Grendgine_Collada_Source s in _geometry.Mesh.Source)
            {
                Grendgine_Collada_Accessor a = s.Technique_Common.Accessor;

                uint group_count = a.Count;
                uint group_stride = a.Stride;
                int count = s.Float_Array.Count;

                float[] temp_array = s.Float_Array.Value();

                for (uint i = 0; i < count; i += group_stride)
                {
                    
                    if (s.ID.Contains("position"))
                    {
                        Vector3 current_position = new Vector3(temp_array[i], temp_array[i + 1], temp_array[i + 2]);
                        temp_position.Add(current_position);
                    }
                    else if (s.ID.Contains("normal"))
                    {
                        temp_normal.Add(new Vector3(temp_array[i], temp_array[i + 1], temp_array[i + 2]));
                    }
                    else if (s.ID.Contains("map"))
                    {
                        temp_uv.Add(new Vector2(temp_array[i], 1.0f - temp_array[i + 1]));
                    }
                }
            }

            //------------------------------------------------------
            // Load Submeshes
            //------------------------------------------------------
            int polylist_counter = 0;
            foreach (Grendgine_Collada_Polylist p in _geometry.Mesh.Polylist)
            {
                // Create new polylist
                DAE_Polylist temp_polylist = new DAE_Polylist(_id + "-sub" + polylist_counter);

                try
                {
                    temp_polylist.load(p, material_collection, temp_position, temp_uv, temp_normal);
                }
                catch (Exception e)
                {
                    throw new Exception(Debug.DebugHelper.format("\t[ FAILED ] Submesh: " + temp_polylist.id, e.Message));
                }

                _submeshes.Add(temp_polylist);
                polylist_counter++;
            }
        }


        public void load(Dictionary<string, DAE_Material> material_collection, DAE_Skeleton skeleton)
        {

            List<Vector3> temp_position = new List<Vector3>();
            List<Vector3> temp_normal = new List<Vector3>();
            List<Vector2> temp_uv = new List<Vector2>();
            List<Vector4> temp_bone_ids = new List<Vector4>();
            List<Vector4> temp_bone_weights = new List<Vector4>();

            // Exit if mesh doesn't have all required data sources
            if (_geometry.Mesh.Source.Count() < 3)
            {
                throw new Exception(Debug.DebugHelper.format("\t[ FAILED ] Mesh: " + _id, "Mesh must include position/normal/texcoord)"));
            }

            this.skeleton = skeleton;

            //------------------------------------------------------
            // Load Position / Normal / UV from Mesh
            //------------------------------------------------------
            foreach (Grendgine_Collada_Source s in _geometry.Mesh.Source)
            {
                Grendgine_Collada_Accessor a = s.Technique_Common.Accessor;

                uint group_count = a.Count;
                uint group_stride = a.Stride;
                int count = s.Float_Array.Count;

                float[] temp_array = s.Float_Array.Value();

                for (uint i = 0; i < count; i += group_stride)
                {
                    int v_index = (int)i / 3;
                    if (s.ID.Contains("position"))
                    {
                        // Add bone ids and weights to list
                        Vector4 ids = new Vector4();
                        Vector4 weights = new Vector4();
                        for (int w = 0; w < Math.Min(skeleton.vertex_weights[v_index].Length, 4); w++)
                        {
                            ids[w] = skeleton.vertex_weights[v_index][w].bone_id;
                            weights[w] = skeleton.vertex_weights[v_index][w].vertex_weight;
                        }
                        temp_bone_ids.Add(ids);
                        temp_bone_weights.Add(weights);

                        // Premultiply position with BSM and add to list
                        Vector3 vertex_position = new Vector3(temp_array[i], temp_array[i + 1], temp_array[i + 2]);
                        vertex_position = Vector4.Transform(new Vector4(vertex_position, 1.0f), skeleton.BSM).Xyz;
                        temp_position.Add(vertex_position);
                    }
                    else if (s.ID.Contains("normal"))
                    {
                        Vector3 vertex_normal = new Vector3(temp_array[i], temp_array[i + 1], temp_array[i + 2]);
                        //vertex_normal = Vector4.Transform(new Vector4(vertex_normal, 0.0f), skeleton.BSM).Xyz;
                        temp_normal.Add(vertex_normal);
                    }
                    else if (s.ID.Contains("map"))
                    {
                        temp_uv.Add(new Vector2(temp_array[i], 1.0f - temp_array[i + 1]));
                    }
                }
            }

            //------------------------------------------------------
            // Load Submeshes
            //------------------------------------------------------
            int polylist_counter = 0;
            foreach (Grendgine_Collada_Polylist p in _geometry.Mesh.Polylist)
            {
                // Create new polylist
                DAE_Polylist temp_polylist = new DAE_Polylist(_id + "-sub" + polylist_counter);

                try
                {
                    temp_polylist.load(p, material_collection, temp_position, temp_uv, temp_normal, temp_bone_ids, temp_bone_weights);
                }
                catch (Exception e)
                {
                    throw new Exception(Debug.DebugHelper.format("\t[ FAILED ] Submesh: " + temp_polylist.id, e.Message));
                }

                _submeshes.Add(temp_polylist);
                polylist_counter++;
            }

            temp_position.Clear();
            temp_normal.Clear();
            temp_uv.Clear();
            temp_bone_ids.Clear();
            temp_bone_weights.Clear();
        }

    }
}
