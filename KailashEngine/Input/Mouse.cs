using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Input;

using KailashEngine.World.Role;

namespace KailashEngine.Input
{
    class Mouse : InputDevice
    {

        private float _sensitivity;
        public float sensitivity
        {
            get { return _sensitivity; }
            set { _sensitivity = value; }
        }


        private bool _locked;
        public bool locked
        {
            get { return _locked; }
            set { _locked = value; }
        }


        private Dictionary<Enum, bool> _buttons;
        public Dictionary<Enum, bool> buttons
        {
            get { return _buttons; }
            set { _buttons = value; }
        }


        private Point _position_current;
        public Point position_current
        {
            get { return _position_current; }
            set { _position_current = value; }
        }

        private Point _position_previous;
        public Point position_previous
        {
            get { return _position_previous; }
            set { _position_previous = value; }
        }


        private Vector3 _delta_current;
        public Vector3 delta_current
        {
            get { return _delta_current; }
            set { _delta_current = value; }
        }

        private Vector3 _delta_previous;
        public Vector3 delta_previous
        {
            get { return _delta_previous; }
            set { _delta_previous = value; }
        }

        private Vector3 _delta_total;
        public Vector3 delta_total
        {
            get { return _delta_total; }
            set { _delta_total = value; }
        }



        public Mouse(float sensitivity, bool locked)
        {
            _sensitivity = sensitivity;
            _locked = locked;
            _buttons = new Dictionary<Enum, bool>();
        }


        public void buttonUp(MouseButtonEventArgs e)
        {
            _buttons[e.Button] = false;
        }

        public void buttonDown(MouseButtonEventArgs e)
        {
            _buttons[e.Button] = true;
        }

        public void wheel(MouseWheelEventArgs e)
        {

        }


        public bool getButtonPress(MouseButton mouse_button)
        {
            return getInput(mouse_button, _buttons);
        }


    }
}
