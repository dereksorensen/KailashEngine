using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Input;

using KailashEngine.World.View;

namespace KailashEngine.Input
{
    class Keyboard : InputDevice
    {

        private bool _repeat;
        public bool repeat
        {
            get { return _repeat; }
            set { _repeat = value; }
        }


        private Dictionary<Enum, bool> _keys;
        public Dictionary<Enum, bool> keys
        {
            get { return _keys; }
            set { _keys = value; }
        }




        public Keyboard(bool key_repeat)
        {
            _repeat = key_repeat;
            _keys = new Dictionary<Enum, bool>();
        }


        public void keyUp(KeyboardKeyEventArgs e)
        {
            _keys[e.Key] = false;

            switch (e.Key)
            {

            }
        }


        public void keyDown(KeyboardKeyEventArgs e)
        {
            _keys[e.Key] = true;

            switch (e.Key)
            {
                
            }
        }


        private bool getKeyPress(Key key)
        {
            return getInput(key, _keys);
        }


        public void buffer()
        {
            //------------------------------------------------------
            // Player Movement
            //------------------------------------------------------

            if (getKeyPress(Key.W))
            {
                Console.WriteLine("Forward");
            }

            if (getKeyPress(Key.S))
            {
                Console.WriteLine("Backward");
            }

            if (getKeyPress(Key.A))
            {
                Console.WriteLine("Left");
            }

            if (getKeyPress(Key.D))
            {
                Console.WriteLine("Right");
            }

            if (getKeyPress(Key.Space))
            {
                Console.WriteLine("Jump");
            }

            if (getKeyPress(Key.ControlLeft))
            {
                Console.WriteLine("Crouch");
            }

            if (getKeyPress(Key.ShiftLeft))
            {
                Console.WriteLine("Run");
            }

            if (getKeyPress(Key.AltLeft))
            {
                Console.WriteLine("Sprint");
            }

        }

    }
}
