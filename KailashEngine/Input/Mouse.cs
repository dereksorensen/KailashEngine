using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Input;

using KailashEngine.World.View;

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



        public Mouse()
        {
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

        public void buffer_Move()
        {

        }


        private bool getButtonPress(MouseButton mouse_button)
        {
            return getInput(mouse_button, _buttons);
        }

        public void buffer_Button()
        {
            if (getButtonPress(MouseButton.Left))
            {
                Console.WriteLine("Left Click");
            }

            if (getButtonPress(MouseButton.Right))
            {
                Console.WriteLine("Right Click");
            }
        }

        public void buffer()
        {
            buffer_Move();
            buffer_Button();
        }



    }
}
