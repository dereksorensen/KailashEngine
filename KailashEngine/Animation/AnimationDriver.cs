using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.Animation
{
    class AnimationDriver
    {

        private Timer _timer;
        public Timer timer
        {
            get { return _timer; }
        }


        public AnimationDriver()
        {
            _timer = new Timer();
        }




    }
}
