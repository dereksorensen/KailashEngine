using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World.Model
{
    class Mesh
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public Vector3 position;
            public Vector2 uv;
            public Vector3 normal;
            public Vector3 tangent;
        }

        private string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }



    }
}
