using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Input;

namespace KailashEngine.Input
{
    class Keyboard
    {

        private bool _repeat;
        public bool repeat
        {
            get { return _repeat; }
            set { _repeat = value; }
        }







        public Keyboard(bool key_repeat)
        {
            _repeat = key_repeat;
        }


        public void keyUp(KeyboardKeyEventArgs e)
        {

        }


        public void keyDown(KeyboardKeyEventArgs e)
        {

        }

        public void buffer()
        {

        }

    }
}
