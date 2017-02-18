using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using grendgine_collada;


namespace KailashEngine.World.Model
{
    class DAE_Polylist : Mesh
    {


        public DAE_Polylist(string id)
            : base(id)
        {
            setBufferIDs(new int[] { 0, 0, 0 });

            _vertices = new List<Vertex>();
            _indices = new List<uint>();
            _vtoi = new Dictionary<Vertex, uint>();

            _vertex_data = null;
            _vertex_data_size = 0;
            _index_data = null;
            _index_data_size = 0;
        }

        public void load(
            Grendgine_Collada_Polylist polylist,
            Dictionary<string, DAE_Material> material_collection,
            List<Vector3> mesh_positions, List<Vector2> mesh_uvs, List<Vector3> mesh_normals, List<Vector4> mesh_bone_ids = null, List<Vector4> mesh_bone_weights = null)
        {
            // Exit if faces are not triangulated
            if (!polylist.VCount.Value_As_String.Contains("3"))
            {
                throw new Exception("Faces must be triangulated");
            }

            int[] temp_indeces = polylist.P.Value();
            int face_count = polylist.Count;

            int lastIndex = 0;
            uint temp_index;

            //------------------------------------------------------
            // Load Faces
            //------------------------------------------------------
            for (int i = 0; i < (face_count * 9); i += 9)
            {
                Vector3[] temp_face =
                        {
                            new Vector3(temp_indeces[i], temp_indeces[i + 1], temp_indeces[i + 2]),
                            new Vector3(temp_indeces[i + 3], temp_indeces[i + 4], temp_indeces[i + 5]),
                            new Vector3(temp_indeces[i + 6], temp_indeces[i + 7], temp_indeces[i + 8]),
                        };

                //------------------------------------------------------
                // Calculate Tangent and Bitangents
                //------------------------------------------------------
                Vector3 e1 = mesh_positions[(int)temp_face[1].X] - mesh_positions[(int)temp_face[0].X];
                Vector3 e2 = mesh_positions[(int)temp_face[2].X] - mesh_positions[(int)temp_face[0].X];

                float dU1 = mesh_uvs[(int)temp_face[1].Z].X - mesh_uvs[(int)temp_face[0].Z].X;
                float dV1 = mesh_uvs[(int)temp_face[1].Z].Y - mesh_uvs[(int)temp_face[0].Z].Y;
                float dU2 = mesh_uvs[(int)temp_face[2].Z].X - mesh_uvs[(int)temp_face[0].Z].X;
                float dV2 = mesh_uvs[(int)temp_face[2].Z].Y - mesh_uvs[(int)temp_face[0].Z].Y;

                float f = 1.0f / (dU1 * dV2 - dU2 * dV1);

                Vector3 tan;

                tan.X = f * (dV2 * e1.X - dV1 * e2.X);
                tan.Y = f * (dV2 * e1.Y - dV1 * e2.Y);
                tan.Z = f * (dV2 * e1.Z - dV1 * e2.Z);

                tan = Vector3.Normalize(tan);

                //Vector3 bitan;

                //bitan.X = f * (-dU2 * e1.X + dU1 * e2.X);
                //bitan.Y = f * (-dU2 * e1.Y + dU1 * e2.Y);
                //bitan.Z = f * (-dU2 * e1.Z + dU1 * e2.Z);

                //bitan = Vector3.Normalize(bitan);

                //------------------------------------------------------
                // Fill vtoi dictionary
                //------------------------------------------------------
                for (int j = 0; j < 3; j++)
                {
                    int x = (int)temp_face[j].X;
                    int y = (int)temp_face[j].Y;
                    int z = (int)temp_face[j].Z;

                    if(mesh_bone_ids == null && mesh_bone_weights == null)
                    {
                        _vertices.Add(new Vertex
                        {
                            position = mesh_positions[x],
                            normal = mesh_normals[y],
                            uv = mesh_uvs[z],
                            tangent = tan
                        });
                    }
                    else // Add bone data if it exists
                    {
                        _vertices.Add(new Vertex
                        {
                            position = mesh_positions[x],
                            normal = mesh_normals[y],
                            uv = mesh_uvs[z],
                            tangent = tan,
                            bone_ids = mesh_bone_ids[x],
                            bone_weights = mesh_bone_weights[x]
                        });
                    }

                    
                    //See if the vertex dictionary already contains an identical vertex
                    if (_vtoi.TryGetValue(_vertices.Last(), out temp_index))
                    {
                        //If so, add the existing index
                        indices.Add(temp_index);
                    }
                    else
                    {
                        //No similar V exists, add a new index
                        _vtoi.Add(_vertices.Last(), (uint)lastIndex);
                        indices.Add((uint)lastIndex);

                        //We need a new index now
                        lastIndex++;
                    }
                }
            }

            _vertices.Clear();
            _vertices.AddRange(_vtoi.Keys);

            _vertex_data = _vertices.ToArray();
            _vertex_data_size = (_vertex_data.Length * Marshal.SizeOf(typeof(Vertex)));

            _index_data = indices.ToArray();
            _index_data_size = _index_data.Length * sizeof(uint);

            _vertices.Clear();
            _indices.Clear();
            _vtoi.Clear();

            //------------------------------------------------------
            // Load Material
            //------------------------------------------------------
            try
            {
                string effect_id = polylist.Material.Replace("-material", "-effect");
                DAE_Material material;
                if (material_collection.TryGetValue(effect_id, out material))
                {
                    _material = material;
                }
                else
                {
                    _material = new DAE_Material("empty");
                }
            }
            catch (Exception)
            {
                // If this polylist doesn't have a material, load an empty one
                _material = new DAE_Material("empty");
            }

        }

    }
}
