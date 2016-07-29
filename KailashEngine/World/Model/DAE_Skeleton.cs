using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.World.Model
{
    class DAE_Skeleton
    {

        private Dictionary<string, DAE_Bone> _bones;
        public Dictionary<string, DAE_Bone> bones
        {
            get { return _bones; }
        }



        public DAE_Skeleton()
        {
            
        }
    }
}
