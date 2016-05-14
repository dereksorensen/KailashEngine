using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.World.Role
{
    class NonPlayableCharacter : Character
    {

        public NonPlayableCharacter(string name, float movement_speed_walk, float movement_speed_run, float look_sensitivity)
            : base(name, movement_speed_walk, movement_speed_run, look_sensitivity)
        { }
    }
}
