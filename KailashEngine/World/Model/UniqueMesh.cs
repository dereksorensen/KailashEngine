using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World.Model
{
    class UniqueMesh : Mesh
    {

        protected Matrix4 _transformation;
        public Matrix4 transformation
        {
            get { return _transformation; }
            set { _transformation = value; }
        }


        private Mesh _mesh;
        public Mesh mesh
        {
            get { return _mesh; }
            set { _mesh = value; }
        }


        public UniqueMesh(string id, Mesh mesh, Matrix4 transformation)
            : base(id)
        {
            _mesh = mesh;
            _transformation = transformation;
        }

    }
}
