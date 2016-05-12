using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.World.Model
{
    class Mesh_DAE : Mesh
    {



        private List<Polylist_DAE> _polylist;
        public List<Polylist_DAE> polylist
        {
            get { return _polylist; }
            set { _polylist = value; }
        }


        public Mesh_DAE()
        {

        }

    }
}
