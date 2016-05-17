using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.Render
{
    class UBOManager
    {

        private Dictionary<int, UBO> _ubos;
        public Dictionary<int, UBO> ubos
        {
            get { return _ubos; }
            set { _ubos = value; }
        }


        public UBOManager()
        {
            _ubos = new Dictionary<int, UBO>();
            
        }

        public void addUBO(UBO ubo)
        {
            
        }

    }
}
