using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.World.Role
{
    class NonPlayableCharacter : Character
    {

        public NonPlayableCharacter(string name, SpatialData spatial_data, float movement_speed_walk, float movement_speed_run)
            : base(name, spatial_data, movement_speed_walk, movement_speed_run)
        { }
    }
}
