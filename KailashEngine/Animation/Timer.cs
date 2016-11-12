using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.Animation
{
    class Timer
    {
        protected Stopwatch _stopwatch;

        protected bool _paused;


        public float minutes
        {
            get
            {
                return _stopwatch.ElapsedMilliseconds / 1000.0f / 60.0f;
            }
        }

        public float seconds
        {
            get
            {
                return _stopwatch.ElapsedMilliseconds / 1000.0f;
            }
        }

        public float milliseconds
        {
            get
            {
                return _stopwatch.ElapsedMilliseconds;
            }
        }


        //------------------------------------------------------
        // Constructor
        //------------------------------------------------------

        public Timer()
        {
            _stopwatch = new Stopwatch();
            _paused = true;
        }



        //------------------------------------------------------
        // Methods
        //------------------------------------------------------

        public void start()
        {
            _paused = false;
            _stopwatch.Start();
        }

        public void stop()
        {
            _paused = true;
            _stopwatch.Stop();
        }

        public void restart()
        {
            _stopwatch.Restart();
        }

        public void pause()
        {
            if(_paused)
            {
                start();
            }
            else
            {
                stop();
            }
        }
        
    }
}
