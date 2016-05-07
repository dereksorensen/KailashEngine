using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World.View
{
    class Camera : WorldObject
    {

        public Camera()
            : base (new Vector3(), new Vector3(), new Vector3())
        { }

        public Camera(Vector3 position, Vector3 look, Vector3 up)
        {
            _position = position;
            _look = look;
            _up = up;
        }



    }
}
