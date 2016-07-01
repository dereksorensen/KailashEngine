using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;


using KailashEngine.World.View;
using KailashEngine.World.Role;

namespace KailashEngine.Client
{
    class Player
    {
        //------------------------------------------------------
        // Player Objects
        //------------------------------------------------------

        private Camera _camera;
        public Camera camera
        {
            get { return _camera; }
            set { _camera = value; }
        }

        private PlayableCharacter _character;
        public PlayableCharacter character
        {
            get { return _character; }
            set { _character = value; }
        }



        public Player(PlayableCharacter character)
        {
            _camera = new Camera();
            _character = character;
        }


        public void load(float fov, float aspect, Vector2 near_far)
        {
            updatePerspective(fov, aspect, near_far);
            _camera.followCharacter(_character);
            
        }

        public void updatePerspective(float fov, float aspect, Vector2 near_far)
        {
            _character.spatial.perspective = Matrix4.CreatePerspectiveFieldOfView(fov, aspect, near_far.X, near_far.Y);
        }
    }
}
