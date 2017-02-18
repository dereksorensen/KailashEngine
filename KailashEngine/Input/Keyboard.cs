using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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


        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        public void turnOffCapLock()
        {
            if (Control.IsKeyLocked(Keys.CapsLock))
            {
                const int KEYEVENTF_EXTENDEDKEY = 0x1;
                const int KEYEVENTF_KEYUP = 0x2;

                keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
            }
        }

    }
}
