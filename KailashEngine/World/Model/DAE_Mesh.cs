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
            Debug.DebugHelper.logInfo(3, "\tLoading Mesh", _id);


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
                        temp_position.Add(new Vector3(temp_array[i], temp_array[i + 1], temp_array[i + 2]));
                    }
                    else if (s.ID.Contains("normal"))
                    {
                        temp_normal.Add(new Vector3(temp_array[i], temp_array[i + 1], temp_array[i + 2]));
                    }
                    else if (s.ID.Contains("map"))
                    {
                        temp_uv.Add(new Vector2(temp_array[i], temp_array[i + 1]));
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


    }
}
