using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World.Model
{
    class UniqueLightMesh : UniqueMesh
    {

        private Vector3 _color;
        public Vector3 color
        {
            get { return _color; }
            set { _color = value; }
        }



        public UniqueLightMesh(string id, Mesh mesh, Matrix4 transformation, Vector3 color)
            : base(id, mesh, transformation)
        {
            _color = color;
        }

    }
}
