using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;





namespace KailashEngine.World.Model
{
    class Mesh : WorldObject
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public Vector3 position;
            public Vector2 uv;
            public Vector3 normal;
            public Vector3 tangent;
        }


        protected string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }


        // Mesh can contain submeshes with their own materials
        protected int _submesh_count;
        public int submesh_count
        {
            get { return _submesh_count; }
            set { _submesh_count = value; }
        }

        protected List<Mesh> _submeshes;
        public List<Mesh> submeshes
        {
            get { return _submeshes; }
            set { _submeshes = value; }
        }

        // Material
        protected Material _material;
        public Material material
        {
            get { return _material; }
            set { _material = value; }
        }



        // Vertex Buffer Object
        protected int _vbo;
        public int vbo
        {
            get { return _vbo; }
            set { _vbo = value; }
        }
        // Index Buffer Object
        protected int _ibo;
        public int ibo
        {
            get { return _ibo; }
            set { _ibo = value; }
        }
        // Vertex Array Object
        protected int _vao;
        public int vao
        {
            get { return _vao; }
            set { _vao = value; }
        }

        // Mesh Vertices
        protected List<Vertex> _vertices;
        public List<Vertex> vertices
        {
            get { return _vertices; }
            set { _vertices = value; }
        }
        // Mesh Indices
        protected List<uint> _indices;
        public List<uint> indices
        {
            get { return _indices; }
            set { _indices = value; }
        }
        // Vertex to Index Mapper Dictionary
        protected Dictionary<Vertex, uint> _vtoi;

        // VAO Helpers
        protected Vertex[] _vertex_data;
        public Vertex[] vertex_data
        {
            get { return _vertex_data; }
            set { _vertex_data = value; }
        }
        protected int _vertex_data_size;
        public int vertex_data_size
        {
            get { return _vertex_data_size; }
            set { _vertex_data_size = value; }
        }
        protected uint[] _index_data;
        public uint[] index_data
        {
            get { return _index_data; }
            set { _index_data = value; }
        }
        protected int _index_data_size;
        public int index_data_size
        {
            get { return _index_data_size; }
            set { _index_data_size = value; }
        }

        
        public Mesh(string id)
        {
            _id = id;
        }

        public void setBufferIDs(int[] buffers)
        {
            _vbo = buffers[0];
            _ibo = buffers[1];
            _vao = buffers[2];
        }
    }
}
