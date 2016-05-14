using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.World.Model
{
    class Material
    {

        protected string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }

        public Material(string id)
        {
            _id = id;
        }
    }
}
