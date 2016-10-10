using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BulletSharp;
using OpenTK;

namespace KailashEngine.Physics
{
    class RigidBodyObject : PhysicsObject
    {





        public RigidBodyObject(string id, RigidBody body, Vector3 scale)
            : base(id, body, scale)
        {

        }

    }
}
