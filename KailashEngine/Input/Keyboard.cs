using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Input;

using KailashEngine.World.View;
using KailashEngine.World.Role;

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


        public Keyboard()
            : this(false)
        { }

        public Keyboard(bool key_repeat)
        {
            _repeat = key_repeat;
            _keys = new Dictionary<Enum, bool>();
        }


        public void keyUp(KeyboardKeyEventArgs e)
        {
            _keys[e.Key] = false;


        }


        public void keyDown(KeyboardKeyEventArgs e)
        {
            _keys[e.Key] = true;

            switch (e.Key)
            {
                
            }
        }


        public bool getKeyPress(Key key)
        {
            return getInput(key, _keys);
        }

    }
}
