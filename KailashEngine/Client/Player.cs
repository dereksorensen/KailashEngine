using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;


using KailashEngine.World;
using KailashEngine.World.Role;
using KailashEngine.World.View;

namespace KailashEngine.Client
{
    class Player
    {
        //------------------------------------------------------
        // Player Objects
        //------------------------------------------------------


        private ControllableWorldObject _character;
        public ControllableWorldObject character
        {
            get { return _character; }
        }

        private Camera _camera;
        public Camera camera
        {
            get { return _camera; }
        }


        private bool _enable_flashlight;
        public bool enable_flashlight
        {
            get { return _enable_flashlight; }
            set { _enable_flashlight = value; }
        }




        public Player()
        { }


        public void controlAndWatch(Camera camera)
        {
            camera.unfollowCharacter();
            _camera = camera;
            _character = camera;
        }

        public void controlAndWatch(ControllableWorldObject character, Camera camera)
        {
            _character = character;
            _camera = camera;
            _camera.followCharacter(_character);
        }


    }
}
