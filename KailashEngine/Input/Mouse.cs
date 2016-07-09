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

        public Vector3 position_delta
        {
            get
            {
                return new Vector3(
                    (-_position_current.Y + _position_previous.Y) * _sensitivity,
                    (-_position_current.X + _position_previous.X) * _sensitivity,
                    (-_position_current.X + _position_previous.X) * _sensitivity
                );
            }
        }
    

        public Mouse(float sensitivity, bool locked)
        {
            _sensitivity = sensitivity;
            _locked = locked;
            _buttons = new Dictionary<Enum, bool>();
            hide();
        }


        public bool getButtonPress(MouseButton mouse_button)
        {
            return getInput(mouse_button, _buttons);
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


        public void hide()
        {
            if (_locked)
            {
                System.Windows.Forms.Cursor.Hide();
            }
            else
            {
                System.Windows.Forms.Cursor.Show();
            }
        }

        public void toggleLock()
        {
            _locked = !_locked;
            hide();
        }

    }
}
