using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.World.Role;

namespace KailashEngine.World.View
{
    class Camera : WorldObject
    {



        public Camera()
            : base (new Vector3(), new Vector3(), new Vector3())
        { }

        public Camera(Vector3 position, Vector3 look, Vector3 up)
            : base (position, look, up)
        { }


        public void followCharacter(Character character)
        {
            try
            {
                this.spatial = character.spatial;
            }
            catch(Exception e)
            {
                Debug.DebugHelper.logError("followCharacter", e.Message);
            }

        }

    }
}
