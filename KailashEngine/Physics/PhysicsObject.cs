using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BulletSharp;
using OpenTK;

namespace KailashEngine.Physics
{
    class PhysicsObject
    {

        private string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }

        private Vector3 _scale;
        public Vector3 scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        private RigidBody _body;
        public RigidBody body
        {
            get { return _body; }
            set { _body = value; }
        }

        public PhysicsObject(string id, RigidBody body, Vector3 scale)
        {
            _id = id;
            _body = body;
            _scale = scale;
        }

    }
}
