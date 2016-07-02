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


        private PlayableCharacter _character;
        public PlayableCharacter character
        {
            get { return _character; }
            set { _character = value; }
        }



        public Player(PlayableCharacter character)
        {
            _character = character;
        }

    }
}
