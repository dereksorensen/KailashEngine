using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using grendgine_collada;

namespace KailashEngine.World.Model
{
    class DAE_Material : Material
    {




        public DAE_Material(string id)
            : base (id)
        { }


        public void load(Grendgine_Collada_Effect effect, Dictionary<string, string> image_collection)
        {

        }

    }
}
