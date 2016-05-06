using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Input;

namespace KailashEngine.Input
{
    class Mouse
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




        public Mouse()
        {

        }



        public void buttonUp(MouseButtonEventArgs e)
        {
            
        }

        public void buttonDown(MouseButtonEventArgs e)
        {

        }

        public void wheel(MouseWheelEventArgs e)
        {

        }

        public void buffer_Move()
        {

        }

        public void buffer_Button()
        {

        }

        public void buffer()
        {
            buffer_Move();
            buffer_Button();
        }



    }
}
