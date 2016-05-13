using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.World.Model
{
    abstract class ModelLoader
    {


        abstract public Dictionary<string, Mesh> load(string filename);


    }
}
