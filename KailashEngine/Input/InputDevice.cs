using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.Input
{
    class InputDevice
    {


        protected bool getInput(Enum input, Dictionary<Enum, bool> input_store)
        {
            bool temp;
            input_store.TryGetValue(input, out temp);
            return temp;
        }

    }
}
