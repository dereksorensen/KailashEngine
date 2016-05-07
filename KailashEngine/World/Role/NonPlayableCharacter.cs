using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.World.Role
{
    class NonPlayableCharacter : Character
    {

        public NonPlayableCharacter(string name, float movement_speed, float look_sensitivity)
            : base(name, movement_speed, look_sensitivity)
        { }
    }
}
